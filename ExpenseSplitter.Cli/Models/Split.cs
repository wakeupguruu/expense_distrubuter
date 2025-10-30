namespace ExpenseSplitter.Cli.Models;

public class Split
{
    public required Member Participant { get; init; }
    public required decimal Amount { get; init; }
}
