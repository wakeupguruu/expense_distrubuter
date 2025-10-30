using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ExpenseSplitter.Web.Models
{
    public class Trip
    {
        public int TripId { get; set; }

        [Required, MaxLength(200)]
        public string TripName { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Range(0, 999999999999.99)]
        public decimal TotalBudget { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(191)]
        public string CreatedBy { get; set; } = string.Empty; // FK to ApplicationUser.Id

        public Guid ShareableLink { get; set; } = Guid.NewGuid();

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<TripMember> Members { get; set; } = new();
        public List<Expense> Expenses { get; set; } = new();
    }
}
