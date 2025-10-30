using ExpenseSplitter.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseSplitter.Web.Pages;

public class BalancesModel : PageModel
{
    private readonly GroupState _state;

    public record Settlement(string From, string To, decimal Amount);

    public List<Settlement> Settlements { get; private set; } = new();
    public List<ExpenseSplitter.Core.Models.Member> Members => _state.Group.Members;

    public BalancesModel(GroupState state)
    {
        _state = state;
    }

    public void OnGet()
    {
        var debtors = Members
            .Where(m => m.Balance > 0.01m)
            .Select(m => new { Member = m, Amount = m.Balance })
            .OrderByDescending(x => x.Amount)
            .ToList();
        var creditors = Members
            .Where(m => m.Balance < -0.01m)
            .Select(m => new { Member = m, Amount = -m.Balance })
            .OrderByDescending(x => x.Amount)
            .ToList();

        int i = 0, j = 0;
        while (i < debtors.Count && j < creditors.Count)
        {
            var pay = Math.Min(debtors[i].Amount, creditors[j].Amount);
            Settlements.Add(new Settlement(debtors[i].Member.Name, creditors[j].Member.Name, pay));
            debtors[i] = new { debtors[i].Member, Amount = debtors[i].Amount - pay };
            creditors[j] = new { creditors[j].Member, Amount = creditors[j].Amount - pay };
            if (debtors[i].Amount <= 0.01m) i++;
            if (creditors[j].Amount <= 0.01m) j++;
        }
    }
}
