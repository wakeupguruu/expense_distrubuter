using System;
using System.ComponentModel.DataAnnotations;

namespace ExpenseSplitter.Web.Models
{
    public class Settlement
    {
        public int SettlementId { get; set; }

        [Required]
        public int TripId { get; set; }

        [Required]
        public string FromUserId { get; set; } = string.Empty;

        [Required]
        public string ToUserId { get; set; } = string.Empty;

        [Range(0, 999999999999.99)]
        public decimal Amount { get; set; }

        public DateTime SettledAt { get; set; } = DateTime.UtcNow;
    }
}
