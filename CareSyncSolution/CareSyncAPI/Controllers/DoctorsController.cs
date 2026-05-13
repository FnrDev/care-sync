using CareSyncAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DoctorsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/doctors
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _db.DoctorProfiles
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Where(d => d.IsActive)
                .ToListAsync();

            var userIds = doctors.Select(d => d.UserId).ToList();
            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = doctors.Select(d => new
            {
                d.Id,
                Name = users.GetValueOrDefault(d.UserId, "Unknown"),
                d.LicenseNumber,
                d.ConsultationDurationMin,
                d.Bio,
                Specializations = d.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name).ToList()
            });

            return Ok(result);
        }

        // GET /api/doctors/by-specialization/{specId}
        [HttpGet("by-specialization/{specId}")]
        public async Task<IActionResult> GetBySpecialization(int specId)
        {
            var doctorSpecs = await _db.DoctorSpecializations
                .Include(ds => ds.DoctorProfile)
                    .ThenInclude(dp => dp.DoctorSpecializations)
                        .ThenInclude(ds2 => ds2.Specialization)
                .Where(ds => ds.SpecializationId == specId && ds.DoctorProfile.IsActive)
                .ToListAsync();

            var doctorProfiles = doctorSpecs.Select(ds => ds.DoctorProfile).DistinctBy(d => d.Id).ToList();

            var userIds = doctorProfiles.Select(d => d.UserId).ToList();
            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = doctorProfiles.Select(d => new
            {
                d.Id,
                Name = users.GetValueOrDefault(d.UserId, "Unknown"),
                d.LicenseNumber,
                d.ConsultationDurationMin,
                d.Bio,
                Specializations = d.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name).ToList()
            });

            return Ok(result);
        }

        // GET /api/doctors/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _db.DoctorProfiles
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .Include(d => d.DoctorAvailabilities)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            var user = await _db.Users.FindAsync(doctor.UserId);

            return Ok(new
            {
                doctor.Id,
                Name = user?.FullName ?? "Unknown",
                doctor.LicenseNumber,
                doctor.ConsultationDurationMin,
                doctor.Bio,
                Specializations = doctor.DoctorSpecializations
                    .Select(ds => ds.Specialization.Name).ToList(),
                Availability = doctor.DoctorAvailabilities.Select(a => new
                {
                    a.DayOfWeek,
                    a.StartTime,
                    a.EndTime
                }).ToList()
            });
        }
    }
}