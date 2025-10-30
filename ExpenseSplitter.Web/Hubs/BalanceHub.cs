using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ExpenseSplitter.Web.Hubs
{
    [Authorize]
    public class BalanceHub : Hub
    {
        public async Task JoinTripGroup(string tripId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"trip-{tripId}");
        }

        public async Task LeaveTripGroup(string tripId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"trip-{tripId}");
        }
    }
}
