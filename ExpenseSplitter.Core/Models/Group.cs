using System.Collections.Generic;

namespace ExpenseSplitter.Core.Models;

public class Group
{
    public required string Name { get; set; }
    public List<Member> Members { get; } = new();
    public List<Expense> Expenses { get; } = new();
}
