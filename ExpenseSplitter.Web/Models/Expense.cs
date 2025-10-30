using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseSplitter.Web.Models
{
    public enum ExpenseCategory
    {
        Food,
        Transport,
        Accommodation,
        Entertainment,
        Shopping,
        Other
    }

    public class Expense
    {
        public int ExpenseId { get; set; }

        [Required]
        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Range(0, 999999999999.99)]
        public decimal TotalAmount { get; set; }

        [Required]
        [MaxLength(191)]
        public string PaidBy { get; set; } = string.Empty; // FK to ApplicationUser.Id

        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;

        [Required]
        public ExpenseCategory Category { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        public bool IsSettled { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Currency support
        [MaxLength(3)]
        public string? CurrencyCode { get; set; }
        public decimal? OriginalAmount { get; set; }
        public decimal? ExchangeRate { get; set; }

        public List<ExpenseSplit> Splits { get; set; } = new();
    }
}
