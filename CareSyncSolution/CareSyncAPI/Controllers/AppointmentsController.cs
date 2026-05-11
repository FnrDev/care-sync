using CareSyncAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // PUBLIC - no auth required
        // GET /api/appointments/lookup?cpr=123&refNumber=PAT-001
        [HttpGet("lookup")]
        [AllowAnonymous]
        public async Task<IActionResult> Lookup([FromQuery] string cpr, [FromQuery] string refNumber)
        {
            if (string.IsNullOrWhiteSpace(cpr) || string.IsNullOrWhiteSpace(refNumber))
                return BadRequest(new { message = "CPR and reference number are required" });

            var patient = await _db.PatientProfiles
                .FirstOrDefaultAsync(p => p.CPR == cpr && p.PatientRefNumber == refNumber);

            if (patient == null)
                return NotFound(new { message = "No patient found with the provided CPR and reference number" });

            var upcoming = await _db.Appointments
                .Include(a => a.DoctorProfile)
                .Include(a => a.Specialization)
                .Include(a => a.Status)
                .Where(a => a.PatientProfileId == patient.Id && a.AppointmentDate >= DateTime.Today)
                .OrderBy(a => a.AppointmentDate).ThenBy(a => a.StartTime)
                .Take(5)
                .ToListAsync();

            // Get doctor names separately
            var doctorIds = upcoming.Select(a => a.DoctorProfile.UserId).Distinct().ToList();
            var doctorUsers = await _db.Users
                .Where(u => doctorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var upcomingResult = upcoming.Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.StartTime,
                a.EndTime,
                Doctor = doctorUsers.GetValueOrDefault(a.DoctorProfile.UserId, "Unknown"),
                Specialization = a.Specialization.Name,
                Status = a.Status.Name
            });

            var recentVisits = await _db.VisitRecords
                .Include(v => v.DoctorProfile)
                .Where(v => v.PatientProfileId == patient.Id)
                .OrderByDescending(v => v.CreatedAt)
                .Take(3)
                .ToListAsync();

            var visitDoctorIds = recentVisits.Select(v => v.DoctorProfile.UserId).Distinct().ToList();
            var visitDoctorUsers = await _db.Users
                .Where(u => visitDoctorIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var visitsResult = recentVisits.Select(v => new
            {
                v.Id,
                v.CreatedAt,
                Doctor = visitDoctorUsers.GetValueOrDefault(v.DoctorProfile.UserId, "Unknown"),
                v.Diagnosis,
                v.DoctorNotes
            });

            return Ok(new
            {
                patient = new { patient.Id, patient.CPR, patient.PatientRefNumber },
                upcomingAppointments = upcomingResult,
                recentVisits = visitsResult
            });
        }

        // PROTECTED - requires JWT
        // GET /api/appointments
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _db.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.DoctorProfile)
                .Include(a => a.Specialization)
                .Include(a => a.Status)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var userIds = appointments
                .SelectMany(a => new[] { a.PatientProfile.UserId, a.DoctorProfile.UserId })
                .Distinct().ToList();

            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = appointments.Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.StartTime,
                a.EndTime,
                Patient = users.GetValueOrDefault(a.PatientProfile.UserId, "Unknown"),
                Doctor = users.GetValueOrDefault(a.DoctorProfile.UserId, "Unknown"),
                Specialization = a.Specialization.Name,
                Status = a.Status.Name
            });

            return Ok(result);
        }
    }
}