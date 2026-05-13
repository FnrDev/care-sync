using CareSyncAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CareSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PatientsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/patients/me
        [HttpGet("me")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _db.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found" });

            var user = await _db.Users.FindAsync(userId);

            return Ok(new
            {
                patient.Id,
                FullName = user?.FullName ?? "Unknown",
                Email = user?.Email ?? "",
                patient.CPR,
                patient.PatientRefNumber,
                patient.DateOfBirth,
                patient.Gender,
                patient.Address,
                patient.BloodType,
                patient.EmergencyContact,
                patient.EmergencyPhone
            });
        }

        // GET /api/patients/me/medical-records
        [HttpGet("me/medical-records")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyMedicalRecords()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _db.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found" });

            var visits = await _db.VisitRecords
                .Include(v => v.DoctorProfile)
                .Include(v => v.Appointment)
                .Include(v => v.Prescription)
                    .ThenInclude(p => p!.PrescriptionItems)
                .Where(v => v.PatientProfileId == patient.Id)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            var doctorUserIds = visits.Select(v => v.DoctorProfile.UserId).Distinct().ToList();
            var doctorUsers = await _db.Users
                .Where(u => doctorUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = visits.Select(v => new
            {
                v.Id,
                AppointmentDate = v.Appointment.AppointmentDate,
                DoctorName = doctorUsers.GetValueOrDefault(v.DoctorProfile.UserId, "Unknown"),
                v.Diagnosis,
                v.DoctorNotes,
                v.Treatment,
                v.CreatedAt,
                Prescription = v.Prescription == null ? null : new
                {
                    v.Prescription.Id,
                    v.Prescription.DateIssued,
                    v.Prescription.Notes,
                    Items = v.Prescription.PrescriptionItems.Select(pi => new
                    {
                        pi.MedicationName,
                        pi.Dosage,
                        pi.Frequency,
                        pi.DurationDays,
                        pi.Instructions
                    })
                }
            });

            return Ok(result);
        }

        // GET /api/patients/search?cpr=123
        [HttpGet("search")]
        [Authorize(Roles = "Receptionist")]
        public async Task<IActionResult> Search([FromQuery] string cpr)
        {
            if (string.IsNullOrWhiteSpace(cpr))
                return BadRequest(new { message = "CPR is required" });

            var patient = await _db.PatientProfiles
                .FirstOrDefaultAsync(p => p.CPR == cpr);

            if (patient == null)
                return NotFound(new { message = "No patient found with this CPR" });

            var user = await _db.Users.FindAsync(patient.UserId);

            return Ok(new
            {
                PatientProfileId = patient.Id,
                FullName = user?.FullName ?? "Unknown",
                patient.CPR,
                patient.PatientRefNumber
            });
        }
    }
}
