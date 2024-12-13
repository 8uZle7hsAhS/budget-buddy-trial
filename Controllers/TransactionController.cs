using Microsoft.AspNetCore.Mvc;
using budget_buddy_trial.Data;
using budget_buddy_trial.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace budget_buddy_trial.Controllers
{
    public class TransactionController : Controller
    {
        private readonly BudgetDbContext _context;

        public TransactionController(BudgetDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Fetch UserId from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "User is not logged in.";
                return RedirectToAction("Log", "Index");
            }

            // Fetch user-specific transactions and categories
            var transactions = _context.Transactions
                .Include(t => t.BudgetCategory)
                .Where(t => t.BudgetCategory.UserId == userId) // Compare with session-based userId
                .OrderByDescending(t => t.Date)
                .ToList();

            // Fetch budget categories for the user
            ViewBag.BudgetCategories = _context.BudgetCategories
                .Where(bc => bc.UserId == userId) // Compare with session-based userId
                .ToList();

            // Calculate wallet balance for the user
            ViewBag.WalletBalance = CalculateWalletBalance(userId);

            return View(transactions);
        }

        // Helper method to calculate wallet balance for the logged-in user
        private decimal CalculateWalletBalance(int userId)
        {
            return _context.BudgetCategories
                .Where(bc => bc.UserId == userId) // Compare with session-based userId
                .Sum(bc => bc.RemainingBudget);
        }

        [HttpPost]
        public IActionResult AddTransaction(Transaction transaction)
        {
            // Fetch UserId from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                TempData["ErrorMessage"] = "User is not logged in.";
                return RedirectToAction("Login", "Account");
            }

            // Validate the transaction
            if (transaction.BudgetCategoryId <= 0)
            {
                TempData["ErrorMessage"] = "Please select a valid budget category.";
                return RedirectToAction("Index");
            }

            var category = _context.BudgetCategories
                .FirstOrDefault(bc => bc.Id == transaction.BudgetCategoryId && bc.UserId == userId);

            if (category == null)
            {
                TempData["ErrorMessage"] = "Selected category does not exist.";
                return RedirectToAction("Index");
            }

            if (transaction.Amount <= 0)
            {
                TempData["ErrorMessage"] = "Transaction amount must be greater than zero.";
                return RedirectToAction("Index");
            }

            if (transaction.Amount > category.RemainingBudget)
            {
                TempData["ErrorMessage"] = "Transaction amount exceeds the remaining budget for this category.";
                return RedirectToAction("Index");
            }

            // Set default date if not provided
            if (transaction.Date == default)
            {
                transaction.Date = DateTime.Now;
            }

            // Update RemainingBudget
            category.RemainingBudget -= transaction.Amount;

            // Update category status
            UpdateCategoryStatus(category);

            // Save changes
            _context.Update(category);
            _context.Add(transaction);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Transaction added successfully!";
            return RedirectToAction("Index");
        }

        // Helper method to update category status based on remaining budget
        private void UpdateCategoryStatus(BudgetCategory category)
        {
            decimal warningThreshold = category.TotalBudget * 0.2m; // 20% remaining
            decimal criticalThreshold = category.TotalBudget * 0.1m; // 10% remaining

            if (category.RemainingBudget <= criticalThreshold)
            {
                category.Status = BudgetStatus.Warning;
            }
            else if (category.RemainingBudget <= warningThreshold)
            {
                category.Status = BudgetStatus.Warning;
            }
            else
            {
                category.Status = BudgetStatus.Active;
            }
        }

        public async Task<IActionResult> GetRecentTransactions(int count = 5)
        {
            // Fetch UserId from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                return Json(new { error = "User not logged in" });
            }

            var recentTransactions = await _context.Transactions
                .Include(t => t.BudgetCategory)
                .Where(t => t.BudgetCategory.UserId == userId) // Compare with session-based userId
                .OrderByDescending(t => t.Date)
                .Take(count)
                .Select(t => new
                {
                    CategoryName = t.BudgetCategory.Name,
                    Date = t.Date,
                    Amount = t.Amount
                })
                .ToListAsync();

            return Json(recentTransactions);
        }

        public IActionResult GetMonthlyExpenses()
        {
            // Fetch UserId from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                return Json(new { error = "User not logged in" });
            }

            var monthlyExpenses = _context.Transactions
                .Where(t => t.BudgetCategory.UserId == userId) // Compare with session-based userId
                .GroupBy(t => t.Date.Month)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Month = g.Key,
                    TotalExpense = g.Sum(t => t.Amount)
                })
                .ToList();

            return Json(monthlyExpenses);
        }

        public IActionResult ExportTransactions()
        {
            // Fetch UserId from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            if (userId == 0)
            {
                return Json(new { error = "User not logged in" });
            }

            var transactions = _context.Transactions
                .Include(t => t.BudgetCategory)
                .Where(t => t.BudgetCategory.UserId == userId) // Compare with session-based userId
                .OrderByDescending(t => t.Date)
                .Select(t => new
                {
                    Category = t.BudgetCategory.Name,
                    Date = t.Date,
                    Amount = t.Amount,
                    Description = t.Description
                })
                .ToList();

            // Implement CSV or Excel export logic here
            // For now, we'll return JSON
            return Json(transactions);
        }
    }
}
