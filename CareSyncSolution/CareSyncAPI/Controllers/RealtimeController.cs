using CareSyncAPI.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace CareSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RealtimeController : ControllerBase
    {
        private readonly IHubContext<AppointmentHub> _hub;

        public RealtimeController(IHubContext<AppointmentHub> hub)
        {
            _hub = hub;
        }

        public record AppointmentStatusBroadcast(int AppointmentId, string NewStatus);

        // POST /api/realtime/appointment-status
        [HttpPost("appointment-status")]
        public async Task<IActionResult> BroadcastAppointmentStatus(
            [FromBody] AppointmentStatusBroadcast payload)
        {
            await _hub.Clients.Group("ClinicBoard").SendAsync("AppointmentStatusChanged", new
            {
                appointmentId = payload.AppointmentId,
                newStatus = payload.NewStatus
            });

            return NoContent();
        }
    }
}
