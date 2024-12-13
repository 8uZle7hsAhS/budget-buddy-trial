using System.ComponentModel.DataAnnotations;

namespace budget_buddy_trial.Models
{
    // View model for Budget Analysis Admin Page
    public class AdminBudgetAnalysisViewModel
    {
        [Key]
        public int Id { get; set; }
        // Overall Platform Statistics
        public decimal PlatformUtilizationRate { get; set; }
        public int TotalUsers { get; set; }
        public int TotalBudgetCategories { get; set; }

        // Popular Budget Categories
        public List<PopularCategoryDetail> TopCategories { get; set; }

        // Detailed Category Breakdown
        public List<BudgetCategoryDetail> AllCategories { get; set; }

        // Utility method for status coloring (similar to existing implementation)
        public string GetStatusClass(BudgetStatus status) => status switch
        {
            BudgetStatus.Active => "text-green-500",
            BudgetStatus.Warning => "text-yellow-500",
            BudgetStatus.Exceeded => "text-red-500",
            _ => "text-gray-500"
        };
    }

    // Detailed view for popular categories
    public class PopularCategoryDetail
    {
        public string Name { get; set; }
        public int UserCount { get; set; }
        public decimal AverageBudget { get; set; }
    }

    // Detailed view for budget categories
    public class BudgetCategoryDetail
    {
        public string Name { get; set; }
        public decimal TotalBudget { get; set; }
        public decimal RemainingBudget { get; set; }
        public BudgetStatus Status { get; set; }
        public int UserCount { get; set; }
    }

    // View model for Spending Trends Admin Page
    public class AdminSpendingTrendsViewModel
    {
        // Platform-wide Spending Statistics
        public decimal TotalPlatformSpending { get; set; }
        public int TotalTransactions { get; set; }
        public DateTime AnalysisStartDate { get; set; }
        public DateTime AnalysisEndDate { get; set; }

        // Peak Spending Periods
        public List<PeakSpendingPeriod> TopSpendingPeriods { get; set; }

        // High-Spending Categories
        public List<CategorySpendingDetail> TopSpendingCategories { get; set; }

        // User Spending Distribution
        public List<UserSpendingBreakdown> UserSpendingTiers { get; set; }
    }

    // Supporting classes for Admin Spending Trends
    public class PeakSpendingPeriod
    {
        public DateTime Period { get; set; }
        public decimal TotalAmount { get; set; }
        public int TransactionCount { get; set; }
    }

    public class CategorySpendingDetail
    {
        public string CategoryName { get; set; }
        public decimal TotalSpent { get; set; }
        public decimal PercentageOfTotalSpending { get; set; }
    }

    public class UserSpendingBreakdown
    {
        public string SpendingTier { get; set; }
        public int UserCount { get; set; }
        public decimal TotalSpending { get; set; }
    }
}