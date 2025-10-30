using ExpenseSplitter.Core.Models;

namespace ExpenseSplitter.Web.Services;

public class GroupState
{
    public Group Group { get; private set; } = new Group { Name = "" };

    public void SetGroupName(string name)
    {
        Group.Name = name;
    }
}
