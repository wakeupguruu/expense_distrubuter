using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseSplitter.Web.Models
{
    public class ExpenseSplit
    {
        public int SplitId { get; set; }

        [Required]
        public int ExpenseId { get; set; }
        public Expense? Expense { get; set; }

        [Required]
        [MaxLength(191)]
        public string UserId { get; set; } = string.Empty; // FK to ApplicationUser.Id

        [Range(0, 999999999999.99)]
        public decimal AmountOwed { get; set; }

        [Range(0, 999999999999.99)]
        public decimal AmountPaid { get; set; }

        public bool IsPaid { get; set; }
        public DateTime? SettledAt { get; set; }
    }
}
