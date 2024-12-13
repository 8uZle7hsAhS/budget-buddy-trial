using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace budget_buddy_trial.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [ForeignKey("BudgetCategory")]
        public int BudgetCategoryId { get; set; }

        public BudgetCategory BudgetCategory { get; set; } // Navigation property

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Transaction amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(255)]
        public string Description { get; set; }

    }
}
