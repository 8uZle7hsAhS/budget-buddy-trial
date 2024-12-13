using budget_buddy_trial.Data;
using budget_buddy_trial.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace budget_buddy_trial.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly BudgetDbContext _context;

        public CategoriesController(BudgetDbContext context)
        {
            _context = context;
        }

        // Display the list of categories for the logged-in user
        public async Task<IActionResult> Index()
        {
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var categories = await _context.BudgetCategories
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "User is not logged in.";
                return RedirectToAction("Log", "Index");
            }

            return View(categories);
        }

        // Show the form to create a new category
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // Handle form submission for creating a new category
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BudgetCategory category)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
                category.UserId = userId;

                var categoryExists = await _context.BudgetCategories
                    .AnyAsync(c => c.UserId == userId && c.Name.ToLower() == category.Name.ToLower());

                if (categoryExists)
                {
                    ModelState.AddModelError("Name", "Category name must be unique.");
                    return View(category);
                }

                category.RemainingBudget = category.TotalBudget;
                category.Status = BudgetStatus.Active;

                _context.BudgetCategories.Add(category);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // Edit a category for the logged-in user
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var category = await _context.BudgetCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BudgetCategory category)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (id != category.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCategory = await _context.BudgetCategories
                        .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

                    if (existingCategory == null)
                    {
                        return NotFound();
                    }

                    existingCategory.Name = category.Name;
                    existingCategory.TotalBudget = category.TotalBudget;
                    existingCategory.RemainingBudget = category.RemainingBudget;
                    existingCategory.Status = category.Status;

                    _context.Update(existingCategory);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(category);
        }

        // Delete a category for the logged-in user
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId") ?? 0;
            var category = await _context.BudgetCategories
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (category == null)
            {
                return NotFound();
            }

            var hasTransactions = await _context.Transactions
                .AnyAsync(t => t.BudgetCategoryId == id);

            if (hasTransactions)
            {
                TempData["ErrorMessage"] = "Cannot delete a category with existing transactions.";
                return RedirectToAction(nameof(Index));
            }

            _context.BudgetCategories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Category deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.BudgetCategories.Any(e => e.Id == id);
        }
    }
}