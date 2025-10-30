using System.ComponentModel.DataAnnotations;
using ExpenseSplitter.Core.Models;
using ExpenseSplitter.Core.Services;
using ExpenseSplitter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseSplitter.Web.Pages;

public class ExpensesModel : PageModel
{
    private readonly GroupState _state;
    private readonly ExpenseManager _manager;

    public List<Member> Members => _state.Group.Members;
    public List<Expense> Expenses => _state.Group.Expenses;

    [BindProperty, Required]
    public string Description { get; set; } = string.Empty;

    [BindProperty, Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [BindProperty]
    public Guid PayerId { get; set; }

    [BindProperty]
    public string SplitMode { get; set; } = "EqualAll"; // EqualAll | EqualSelected | CustomSelected

    [BindProperty]
    public List<Guid> SelectedIds { get; set; } = new();

    [BindProperty]
    public Dictionary<Guid, decimal> CustomAmounts { get; set; } = new();

    public ExpensesModel(GroupState state, ExpenseManager manager)
    {
        _state = state;
        _manager = manager;
    }

    public void OnGet()
    {
        if (Members.Count > 0 && PayerId == Guid.Empty)
        {
            PayerId = Members[0].Id;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var payer = Members.FirstOrDefault(m => m.Id == PayerId);
        if (payer == null)
        {
            ModelState.AddModelError(string.Empty, "Select a valid payer");
            return Page();
        }
        if (Members.Count == 0)
        {
            ModelState.AddModelError(string.Empty, "Add members first");
            return Page();
        }

        var splits = new List<Split>();
        List<Member> participants;
        switch (SplitMode)
        {
            case "EqualAll":
                participants = Members.ToList();
                splits = EqualSplit(participants, Amount);
                break;
            case "EqualSelected":
                participants = Members.Where(m => SelectedIds.Contains(m.Id)).ToList();
                if (participants.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Select at least one participant");
                    return Page();
                }
                splits = EqualSplit(participants, Amount);
                break;
            case "CustomSelected":
                participants = Members.Where(m => SelectedIds.Contains(m.Id)).ToList();
                if (participants.Count == 0)
                {
                    ModelState.AddModelError(string.Empty, "Select at least one participant");
                    return Page();
                }
                decimal remaining = Amount;
                foreach (var m in participants)
                {
                    CustomAmounts.TryGetValue(m.Id, out var a);
                    if (a < 0)
                    {
                        ModelState.AddModelError(string.Empty, $"Amount for {m.Name} cannot be negative");
                        return Page();
                    }
                    if (a > remaining)
                    {
                        ModelState.AddModelError(string.Empty, "One of the custom amounts exceeds remaining");
                        return Page();
                    }
                    splits.Add(new Split { Participant = m, Amount = a });
                    remaining -= a;
                }
                if (Math.Abs(remaining) > 0.01m)
                {
                    ModelState.AddModelError(string.Empty, $"Split does not sum to total. Remaining {remaining:0.00}");
                    return Page();
                }
                var zeroPeople = splits.Where(s => s.Amount == 0m).Select(s => s.Participant.Name).ToList();
                if (zeroPeople.Count > 0)
                {
                    TempData["Warn"] = $"Warning: Selected with 0 amount: {string.Join(", ", zeroPeople)}";
                }
                break;
            default:
                ModelState.AddModelError(string.Empty, "Invalid split mode");
                return Page();
        }

        try
        {
            _manager.AddExpense(_state.Group, payer, Amount, Description, splits);
            TempData["Msg"] = "Expense recorded";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
    }

    private static List<Split> EqualSplit(List<Member> selected, decimal total)
    {
        var per = Math.Round(total / selected.Count, 2, MidpointRounding.AwayFromZero);
        var splits = selected.Select(m => new Split { Participant = m, Amount = per }).ToList();
        var diff = total - splits.Sum(s => s.Amount);
        if (Math.Abs(diff) > 0.001m)
        {
            splits[^1] = new Split { Participant = splits[^1].Participant, Amount = splits[^1].Amount + diff };
        }
        return splits;
    }
}
