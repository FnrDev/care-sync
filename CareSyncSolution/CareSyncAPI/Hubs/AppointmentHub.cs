using Microsoft.AspNetCore.SignalR;

namespace CareSyncAPI.Hubs
{
    public class AppointmentHub : Hub
    {
        public async Task JoinClinicBoard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ClinicBoard");
        }
    }
}
