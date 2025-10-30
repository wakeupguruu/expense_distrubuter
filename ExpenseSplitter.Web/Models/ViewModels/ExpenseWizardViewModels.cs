using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using ExpenseSplitter.Web.Models;

namespace ExpenseSplitter.Web.Models.ViewModels
{
    public enum SplitMethod
    {
        EqualAll,
        EqualSelected,
        Custom
    }

    public class MemberOption
    {
        public string UserId { get; set; } = string.Empty;
        public string Display { get; set; } = string.Empty;
        public bool Selected { get; set; }
        public decimal? CustomAmount { get; set; }
    }

    public class ExpenseStep1Vm
    {
        [Required]
        public int TripId { get; set; }
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;
        [Range(0, 999999999999.99)]
        public decimal TotalAmount { get; set; }
        [Required]
        public ExpenseCategory Category { get; set; }
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.UtcNow.Date;
        public string? Description { get; set; }
        [Required]
        public string PaidBy { get; set; } = string.Empty;

        public List<MemberOption> Members { get; set; } = new();
    }

    public class ExpenseStep2Vm : ExpenseStep1Vm
    {
        [Required]
        public SplitMethod SplitMethod { get; set; } = SplitMethod.EqualAll;
        public List<string> SelectedUserIds { get; set; } = new();
        public List<MemberOption> CustomSplits { get; set; } = new();
    }

    public class ExpenseStep3Vm : ExpenseStep2Vm
    {
        public bool IsSettled { get; set; }
        public decimal TotalCustom => TotalAmount;
    }
}
