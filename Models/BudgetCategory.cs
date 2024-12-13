using System.ComponentModel.DataAnnotations;

namespace budget_buddy_trial.Models
{
    public class BudgetCategory
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Range(0, double.MaxValue)]
        public decimal TotalBudget { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RemainingBudget { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public BudgetStatus Status { get; set; }

    }

    public enum BudgetStatus
    {
        Active,
        Warning,
        Exceeded
    }
}
