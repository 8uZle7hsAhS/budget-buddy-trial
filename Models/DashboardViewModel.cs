using budget_buddy_trial.Models;
using System.Collections.Generic;

namespace budget_buddy_trial.Models
{
    public class DashboardViewModel
    {
        public decimal TotalExpenses { get; set; }
        public decimal WalletBalance { get; set; }
        public List<BudgetCategory> Categories { get; set; }
        public List<Transaction> RecentTransactions { get; set; }
        public List<decimal> WeeklyExpenses { get; set; }
    }
}