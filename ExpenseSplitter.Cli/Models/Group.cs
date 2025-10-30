using System.Collections.Generic;

namespace ExpenseSplitter.Cli.Models;

public class Group
{
    public required string Name { get; init; }
    public List<Member> Members { get; } = new();
    public List<Expense> Expenses { get; } = new();
}
