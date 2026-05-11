using CareSyncAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ReportsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/reports/appointment-stats
        [HttpGet("appointment-stats")]
        public async Task<IActionResult> AppointmentStats()
        {
            var stats = await _db.Appointments
                .Include(a => a.Status)
                .GroupBy(a => a.Status.Name)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            var total = await _db.Appointments.CountAsync();
            var today = DateTime.Today;
            var todayCount = await _db.Appointments
                .CountAsync(a => a.AppointmentDate == today);

            return Ok(new { total, todayCount, byStatus = stats });
        }

        // GET /api/reports/doctor-utilization
        [HttpGet("doctor-utilization")]
        public async Task<IActionResult> DoctorUtilization()
        {
            var data = await _db.Appointments
                .Include(a => a.DoctorProfile)
                .GroupBy(a => a.DoctorProfileId)
                .Select(g => new
                {
                    DoctorProfileId = g.Key,
                    TotalAppointments = g.Count(),
                    Completed = g.Count(a => a.StatusId == 5),
                    Cancelled = g.Count(a => a.StatusId == 6)
                })
                .ToListAsync();

            var doctorIds = data.Select(d => d.DoctorProfileId).ToList();
            var profiles = await _db.DoctorProfiles
                .Where(d => doctorIds.Contains(d.Id))
                .ToDictionaryAsync(d => d.Id, d => d.UserId);

            var userIds = profiles.Values.ToList();
            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = data.Select(d => new
            {
                Doctor = users.GetValueOrDefault(
                    profiles.GetValueOrDefault(d.DoctorProfileId, ""), "Unknown"),
                d.TotalAppointments,
                d.Completed,
                d.Cancelled
            });

            return Ok(result);
        }
    }
}