using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseSplitter.Web.Models
{
    public class TripMember
    {
        public int TripMemberId { get; set; }

        [Required]
        public int TripId { get; set; }
        public Trip? Trip { get; set; }

        [Required]
        [MaxLength(191)]
        public string UserId { get; set; } = string.Empty; // FK to ApplicationUser.Id

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999999.99)]
        public decimal PersonalBudget { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentBalance { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
