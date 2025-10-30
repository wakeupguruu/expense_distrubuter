using System.Linq;
using ExpenseSplitter.Core.Services;
using ExpenseSplitter.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseSplitter.Web.Pages;

public class IndexModel : PageModel
{
    private readonly GroupState _state;
    private readonly ExpenseManager _manager;

    [BindProperty]
    public string GroupName { get; set; } = string.Empty;

    [BindProperty]
    public string NewMemberName { get; set; } = string.Empty;

    public IReadOnlyList<string> Members => _state.Group.Members.Select(m => m.Name).ToList();

    public IndexModel(GroupState state, ExpenseManager manager)
    {
        _state = state;
        _manager = manager;
    }

    public void OnGet()
    {
        GroupName = _state.Group.Name;
    }

    public IActionResult OnPostSetGroupName()
    {
        if (string.IsNullOrWhiteSpace(GroupName))
        {
            ModelState.AddModelError(nameof(GroupName), "Group name is required");
            return Page();
        }
        _state.SetGroupName(GroupName.Trim());
        TempData["Msg"] = "Group name updated";
        return RedirectToPage();
    }

    public IActionResult OnPostAddMember()
    {
        try
        {
            _manager.AddMember(_state.Group, NewMemberName);
            TempData["Msg"] = "Member added";
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }
        return RedirectToPage();
    }
}
