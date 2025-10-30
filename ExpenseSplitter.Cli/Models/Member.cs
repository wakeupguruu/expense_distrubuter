using System;

namespace ExpenseSplitter.Cli.Models;

public class Member
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public decimal Balance { get; set; } = 0m; // Negative => they are owed; Positive => they owe

    public override string ToString() => $"{Name} (Balance: {Balance:+0.00;-0.00;0.00})";
}
