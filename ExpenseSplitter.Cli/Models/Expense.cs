using System;
using System.Collections.Generic;

namespace ExpenseSplitter.Cli.Models;

public class Expense
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Description { get; init; }
    public required Member Payer { get; init; }
    public required decimal Amount { get; init; }
    public DateTime Date { get; init; } = DateTime.Now;
    public List<Split> Splits { get; } = new();
}
