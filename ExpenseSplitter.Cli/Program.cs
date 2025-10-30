using System.Globalization;
using ExpenseSplitter.Cli.Models;
using ExpenseSplitter.Cli.Services;

var culture = CultureInfo.InvariantCulture;
Console.OutputEncoding = System.Text.Encoding.UTF8;

var manager = new ExpenseManager();
var group = new Group { Name = Prompt("Enter group name: ") };

Console.WriteLine($"\nGroup '{group.Name}' created.\n");

while (true)
{
    Console.WriteLine("Choose an option:");
    Console.WriteLine("1) Add member");
    Console.WriteLine("2) List members");
    Console.WriteLine("3) Add expense");
    Console.WriteLine("4) Show balances");
    Console.WriteLine("5) Show expenses");
    Console.WriteLine("6) Settlement suggestion");
    Console.WriteLine("0) Exit");
    Console.Write("> ");
    var choice = Console.ReadLine();
    Console.WriteLine();

    switch (choice)
    {
        case "1":
            AddMemberFlow();
            break;
        case "2":
            ListMembers();
            break;
        case "3":
            AddExpenseFlow();
            break;
        case "4":
            ShowBalances();
            break;
        case "5":
            ShowExpenses();
            break;
        case "6":
            ShowSettlement();
            break;
        case "0":
            return;
        default:
            Console.WriteLine("Invalid choice.\n");
            break;
    }
}

void AddMemberFlow()
{
    var name = Prompt("Member name: ").Trim();
    try
    {
        manager.AddMember(group, name);
        Console.WriteLine("Member added.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n");
    }
}

void ListMembers()
{
    if (group.Members.Count == 0)
    {
        Console.WriteLine("No members yet.\n");
        return;
    }
    for (int i = 0; i < group.Members.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {group.Members[i].Name}");
    }
    Console.WriteLine();
}

void AddExpenseFlow()
{
    if (group.Members.Count == 0)
    {
        Console.WriteLine("Add members first.\n");
        return;
    }

    var desc = Prompt("Description: ");
    var amount = ReadDecimal("Total amount: ");
    var payer = SelectSingleMember("Select payer by number: ");

    Console.WriteLine("Split options:");
    Console.WriteLine("1) Split equally among ALL members");
    Console.WriteLine("2) Split equally among SELECTED members");
    Console.WriteLine("3) Custom amounts for SELECTED members");
    Console.Write("> ");
    var mode = Console.ReadLine();

    var splits = new List<Split>();
    List<Member> selected;

    switch (mode)
    {
        case "1":
            selected = group.Members.ToList();
            splits = EqualSplit(selected, amount);
            break;
        case "2":
            selected = SelectMultipleMembers();
            if (selected.Count == 0)
            {
                Console.WriteLine("No participants selected.\n");
                return;
            }
            splits = EqualSplit(selected, amount);
            break;
        case "3":
            selected = SelectMultipleMembers();
            if (selected.Count == 0)
            {
                Console.WriteLine("No participants selected.\n");
                return;
            }
            decimal remaining = amount;
            foreach (var m in selected)
            {
                var a = ReadDecimal($"Amount for {m.Name} (remaining {remaining:0.00}): ", allowZero: true);
                if (a > remaining)
                {
                    Console.WriteLine("Amount exceeds remaining. Try again.\n");
                    return;
                }
                splits.Add(new Split { Participant = m, Amount = a });
                remaining -= a;
            }
            if (Math.Abs(remaining) > 0.01m)
            {
                Console.WriteLine($"Split does not sum to total. Remaining {remaining:0.00}.\n");
                return;
            }
            var zeroPeople = splits.Where(s => s.Amount == 0m).Select(s => s.Participant.Name).ToList();
            if (zeroPeople.Count > 0)
            {
                Console.Write($"Warning: You selected {string.Join(", ", zeroPeople)} but assigned 0. Proceed? (y/n): ");
                var confirm = Console.ReadLine()?.Trim().ToLowerInvariant();
                if (confirm != "y") return;
            }
            break;
        default:
            Console.WriteLine("Invalid split option.\n");
            return;
    }

    try
    {
        manager.AddExpense(group, payer, amount, desc, splits);
        Console.WriteLine("Expense recorded.\n");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}\n");
    }
}

void ShowBalances()
{
    if (group.Members.Count == 0)
    {
        Console.WriteLine("No members.\n");
        return;
    }
    Console.WriteLine("Balances (negative => they are owed, positive => they owe):");
    foreach (var m in group.Members)
    {
        Console.WriteLine($"- {m.Name}: {m.Balance:+0.00;-0.00;0.00}");
    }
    Console.WriteLine();
}

void ShowExpenses()
{
    if (group.Expenses.Count == 0)
    {
        Console.WriteLine("No expenses yet.\n");
        return;
    }
    foreach (var e in group.Expenses)
    {
        var parts = string.Join(", ", e.Splits.Select(s => $"{s.Participant.Name}:{s.Amount:0.00}"));
        Console.WriteLine($"- {e.Date:g} | {e.Description} | Paid by {e.Payer.Name} {e.Amount:0.00} | Split: {parts}");
    }
    Console.WriteLine();
}

void ShowSettlement()
{
    var debtors = group.Members
        .Where(m => m.Balance > 0.01m)
        .Select(m => new { Member = m, Amount = m.Balance })
        .OrderByDescending(x => x.Amount)
        .ToList();
    var creditors = group.Members
        .Where(m => m.Balance < -0.01m)
        .Select(m => new { Member = m, Amount = -m.Balance }) // convert to positive owed-to amount
        .OrderByDescending(x => x.Amount)
        .ToList();

    if (debtors.Count == 0 && creditors.Count == 0)
    {
        Console.WriteLine("All settled.\n");
        return;
    }

    Console.WriteLine("Suggested transfers:");
    int i = 0, j = 0;
    while (i < debtors.Count && j < creditors.Count)
    {
        var pay = Math.Min(debtors[i].Amount, creditors[j].Amount);
        Console.WriteLine($"- {debtors[i].Member.Name} pays {creditors[j].Member.Name} {pay:0.00}");
        debtors[i] = new { debtors[i].Member, Amount = debtors[i].Amount - pay };
        creditors[j] = new { creditors[j].Member, Amount = creditors[j].Amount - pay };
        if (debtors[i].Amount <= 0.01m) i++;
        if (creditors[j].Amount <= 0.01m) j++;
    }
    Console.WriteLine();
}

Member SelectSingleMember(string prompt)
{
    while (true)
    {
        for (int i = 0; i < group.Members.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {group.Members[i].Name}");
        }
        var idx = ReadInt(prompt);
        if (idx >= 1 && idx <= group.Members.Count) return group.Members[idx - 1];
        Console.WriteLine("Invalid selection.\n");
    }
}

List<Member> SelectMultipleMembers()
{
    Console.WriteLine("Select participants (comma-separated numbers), or 'all':");
    for (int i = 0; i < group.Members.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {group.Members[i].Name}");
    }
    Console.Write("> ");
    var line = Console.ReadLine()?.Trim() ?? string.Empty;
    if (string.Equals(line, "all", StringComparison.OrdinalIgnoreCase))
        return group.Members.ToList();
    var set = new HashSet<int>();
    foreach (var token in line.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
    {
        if (int.TryParse(token, out var n) && n >= 1 && n <= group.Members.Count)
            set.Add(n - 1);
    }
    return set.Select(i => group.Members[i]).ToList();
}

List<Split> EqualSplit(List<Member> selected, decimal total)
{
    var per = Math.Round(total / selected.Count, 2, MidpointRounding.AwayFromZero);
    var splits = selected.Select(m => new Split { Participant = m, Amount = per }).ToList();
    // Adjust rounding on last participant
    var diff = total - splits.Sum(s => s.Amount);
    if (Math.Abs(diff) > 0.001m)
    {
        splits[^1] = new Split { Participant = splits[^1].Participant, Amount = splits[^1].Amount + diff };
    }
    return splits;
}

string Prompt(string text)
{
    Console.Write(text);
    return Console.ReadLine() ?? string.Empty;
}

int ReadInt(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (int.TryParse(s, out var n)) return n;
        Console.WriteLine("Enter a valid integer.\n");
    }
}

decimal ReadDecimal(string prompt, bool allowZero = false)
{
    while (true)
    {
        Console.Write(prompt);
        var s = Console.ReadLine();
        if (decimal.TryParse(s, NumberStyles.Number, culture, out var d))
        {
            if (d < 0m || (!allowZero && d == 0m))
            {
                Console.WriteLine("Enter a positive amount.\n");
                continue;
            }
            return Math.Round(d, 2, MidpointRounding.AwayFromZero);
        }
        Console.WriteLine("Enter a valid amount.\n");
    }
}
