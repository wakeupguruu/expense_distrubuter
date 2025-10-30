using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseSplitter.Core.Models;

namespace ExpenseSplitter.Core.Services;

public class ExpenseManager
{
    public void AddMember(Group group, string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required");
        if (group.Members.Any(m => string.Equals(m.Name, name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Member with same name already exists");
        group.Members.Add(new Member { Name = name.Trim() });
    }

    public Expense AddExpense(
        Group group,
        Member payer,
        decimal amount,
        string description,
        IEnumerable<Split> splits)
    {
        if (amount <= 0) throw new ArgumentException("Amount must be positive");
        var splitList = splits.ToList();
        if (splitList.Count == 0) throw new ArgumentException("At least one participant required");

        var sum = splitList.Sum(s => s.Amount);
        if (Math.Abs(sum - amount) > 0.01m)
            throw new ArgumentException($"Split total {sum:0.00} does not equal expense amount {amount:0.00}");

        var expense = new Expense
        {
            Description = description,
            Payer = payer,
            Amount = amount
        };
        expense.Splits.AddRange(splitList);

        payer.Balance -= amount;
        foreach (var s in splitList)
        {
            s.Participant.Balance += s.Amount;
        }

        group.Expenses.Add(expense);
        return expense;
    }
}
