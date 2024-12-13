using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using budget_buddy_trial.Data;
using budget_buddy_trial.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace budget_buddy_trial.Controllers
{
    public class AdminController : Controller
    {
        private readonly BudgetDbContext _context;

        public AdminController(BudgetDbContext context)
        {
            _context = context;
        }

        public IActionResult BudgetAnalysis()
        {
            var viewModel = new AdminBudgetAnalysisViewModel
            {
                TotalUsers = _context.Users.Count(),
                TotalBudgetCategories = _context.BudgetCategories.Count(),
                PlatformUtilizationRate = CalculatePlatformUtilizationRate(),
                TopCategories = GetTopCategories(),
                AllCategories = GetAllCategoryDetails()
            };

            return View(viewModel);
        }

        public IActionResult SpendingTrends()
        {
            var viewModel = new AdminSpendingTrendsViewModel
            {
                TotalPlatformSpending = CalculateTotalPlatformSpending(),
                TotalTransactions = _context.Transactions.Count(),
                AnalysisStartDate = DateTime.Now.AddMonths(-3),
                AnalysisEndDate = DateTime.Now,
                TopSpendingPeriods = GetTopSpendingPeriods(),
                TopSpendingCategories = GetTopSpendingCategories(),
                UserSpendingTiers = CalculateUserSpendingTiers()
            };

            return View(viewModel);
        }

        // Utility Methods for Budget Analysis
        private decimal CalculatePlatformUtilizationRate()
        {
            var categories = _context.BudgetCategories.ToList();
            var totalBudget = categories.Sum(b => b.TotalBudget);
            var spentBudget = categories.Sum(b => b.TotalBudget - b.RemainingBudget);
            return totalBudget > 0 ? (spentBudget / totalBudget) * 100 : 0;
        }

        private List<PopularCategoryDetail> GetTopCategories()
        {
            return _context.BudgetCategories
                .GroupBy(b => b.Name)
                .Select(g => new PopularCategoryDetail
                {
                    Name = g.Key,
                    UserCount = g.Select(x => x.UserId).Distinct().Count(),
                    AverageBudget = g.Average(b => b.TotalBudget)
                })
                .OrderByDescending(c => c.UserCount)
                .Take(5)
                .ToList();
        }

        private List<BudgetCategoryDetail> GetAllCategoryDetails()
        {
            return _context.BudgetCategories
                .GroupBy(b => b.Name)
                .Select(g => new BudgetCategoryDetail
                {
                    Name = g.Key,
                    TotalBudget = g.Sum(b => b.TotalBudget),
                    RemainingBudget = g.Sum(b => b.RemainingBudget),
                    Status = g.First().Status, // Assuming status is consistent
                    UserCount = g.Select(x => x.UserId).Distinct().Count()
                })
                .ToList();
        }

        // Spending Trends Utility Methods
        private decimal CalculateTotalPlatformSpending()
        {
            return _context.Transactions
                .Sum(t => t.Amount);
        }

        private List<PeakSpendingPeriod> GetTopSpendingPeriods()
        {
            return _context.Transactions
    .Where(t => t.Date >= DateTime.Now.AddMonths(-3)) // Limit to the last 3 months
    .GroupBy(t => new {
        Year = t.Date.Year,
        Week = (t.Date.DayOfYear / 7)
    })
    .Select(g => new PeakSpendingPeriod
    {
        Period = new DateTime(g.Key.Year, 1, 1).AddDays(g.Key.Week * 7),
        TotalAmount = g.Sum(t => t.Amount),
        TransactionCount = g.Count()
    })
    .AsEnumerable()
    .OrderByDescending(p => p.TotalAmount)
    .Take(5)
    .ToList();

        }

        private List<CategorySpendingDetail> GetTopSpendingCategories()
        {
            var totalPlatformSpending = CalculateTotalPlatformSpending();

            return _context.Transactions
    .GroupBy(t => t.BudgetCategory.Name)
    .Select(g => new CategorySpendingDetail
    {
        CategoryName = g.Key,
        TotalSpent = g.Sum(t => t.Amount),
        PercentageOfTotalSpending = Math.Round(
            (g.Sum(t => t.Amount) / totalPlatformSpending) * 100, 2)
    })
    .OrderByDescending(c => (double)c.TotalSpent)
    .Take(10)
    .ToList();
        }

        private List<UserSpendingBreakdown> CalculateUserSpendingTiers()
        {
            var userSpending = _context.Transactions
                .GroupBy(t => t.UserId)
                .Select(g => new
                {
                    UserId = g.Key,
                    TotalSpending = g.Sum(t => t.Amount)
                })
                .ToList();

            var tiers = new[]
            {
                new { Name = "Low Spenders (0-₱100)", Min = 0m, Max = 100m },
                new { Name = "Moderate Spenders (₱100-₱500)", Min = 100m, Max = 500m },
                new { Name = "High Spenders (₱500-₱1000)", Min = 500m, Max = 1000m },
                new { Name = "Power Spenders (₱1000+)", Min = 1000m, Max = decimal.MaxValue }
            };

            return tiers.Select(tier => new UserSpendingBreakdown
            {
                SpendingTier = tier.Name,
                UserCount = userSpending.Count(u =>
                    u.TotalSpending >= tier.Min && u.TotalSpending < tier.Max),
                TotalSpending = userSpending
                    .Where(u => u.TotalSpending >= tier.Min && u.TotalSpending < tier.Max)
                    .Sum(u => u.TotalSpending)
            }).ToList();
        }
    }
}