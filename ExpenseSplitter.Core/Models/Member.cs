using System;

namespace ExpenseSplitter.Core.Models;

public class Member
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public decimal Balance { get; set; } = 0m; // Negative => they are owed; Positive => they owe
}
