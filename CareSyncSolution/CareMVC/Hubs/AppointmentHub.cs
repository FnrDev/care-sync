using Microsoft.AspNetCore.SignalR;

namespace CareMVC.Hubs
{
    public class AppointmentHub : Hub
    {
        // Called when a client connects
        public async Task JoinClinicBoard()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "ClinicBoard");
        }
    }
}