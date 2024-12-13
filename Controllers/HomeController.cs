using Microsoft.AspNetCore.Mvc;
using budget_buddy_trial.Data;
using budget_buddy_trial.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;

namespace budget_buddy_trial.Controllers
{
    public class HomeController : Controller
    {
        private readonly BudgetDbContext _context;

        public HomeController(BudgetDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // Check if the user is logged in by checking session
            if (HttpContext.Session.GetString("Email") == null)
            {
                // If not authenticated, redirect to login page
                return RedirectToAction("Index", "Log");
            }

            // Fetch UserId from session
            int userId = HttpContext.Session.GetInt32("UserId") ?? 0;

            // Prepare ViewModel
            var viewModel = new DashboardViewModel
            {
                // Calculate Total Expenses (last 30 days)
                TotalExpenses = _context.Transactions
                    .Include(t => t.BudgetCategory)
                    .Where(t => t.BudgetCategory.UserId == userId &&
                                t.Date >= DateTime.Now.AddDays(-30))
                    .Sum(t => t.Amount),

                // Calculate Wallet Balance
                WalletBalance = _context.BudgetCategories
                    .Where(bc => bc.UserId == userId)
                    .Sum(bc => bc.RemainingBudget),

                // Get Budget Categories
                Categories = _context.BudgetCategories
                    .Where(bc => bc.UserId == userId)
                    .ToList(),

                // Get Recent Transactions (last 5)
                RecentTransactions = _context.Transactions
                    .Include(t => t.BudgetCategory)
                    .Where(t => t.BudgetCategory.UserId == userId)
                    .OrderByDescending(t => t.Date)
                    .Take(5)
                    .ToList(),

                // Get Monthly Expenses for Chart (last 6 months)
                WeeklyExpenses = _context.Transactions
                     .Include(t => t.BudgetCategory)
                     .Where(t => t.BudgetCategory.UserId == userId)
                     .GroupBy(t => new {
                      Year = t.Date.Year,
                      Week = (t.Date.DayOfYear - 1) / 7 + 1 // Calculate week number manually
                      })
                     .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Week)
                     .Select(g => g.Sum(t => t.Amount))
                     .Take(6)
                     .ToList(),
            };

            return View(viewModel);
        }
    }
}
