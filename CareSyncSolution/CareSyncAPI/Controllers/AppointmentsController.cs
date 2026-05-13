using CareSyncAPI.Data;
using CareSyncAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
        public async Task<IActionResult> Lookup([FromQuery] string cpr, [FromQuery] string? refNumber = null)
        {
            if (string.IsNullOrWhiteSpace(cpr))
                return BadRequest(new { message = "CPR is required" });

            var query = _db.PatientProfiles.Where(p => p.CPR == cpr);

            if (!string.IsNullOrWhiteSpace(refNumber))
                query = query.Where(p => p.PatientRefNumber == refNumber);

            var patient = await query.FirstOrDefaultAsync();

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

        // GET /api/appointments/my - patient's own appointments
        [HttpGet("my")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _db.PatientProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (patient == null)
                return NotFound(new { message = "Patient profile not found" });

            var appointments = await _db.Appointments
                .Include(a => a.DoctorProfile)
                .Include(a => a.Specialization)
                .Include(a => a.Status)
                .Where(a => a.PatientProfileId == patient.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            var doctorUserIds = appointments.Select(a => a.DoctorProfile.UserId).Distinct().ToList();
            var doctorUsers = await _db.Users
                .Where(u => doctorUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var result = appointments.Select(a => new
            {
                a.Id,
                a.AppointmentDate,
                a.StartTime,
                a.EndTime,
                Doctor = doctorUsers.GetValueOrDefault(a.DoctorProfile.UserId, "Unknown"),
                Specialization = a.Specialization.Name,
                Status = a.Status.Name,
                a.StatusId,
                a.CancellationReason
            });

            return Ok(result);
        }

        // GET /api/appointments/today - today's queue for receptionist
        [HttpGet("today")]
        [Authorize(Roles = "Receptionist,Admin")]
        public async Task<IActionResult> GetToday()
        {
            var today = DateTime.Today;

            var appointments = await _db.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.DoctorProfile)
                .Include(a => a.Specialization)
                .Include(a => a.Status)
                .Where(a => a.AppointmentDate.Date == today)
                .OrderBy(a => a.StartTime)
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
                Status = a.Status.Name,
                a.StatusId
            });

            return Ok(result);
        }

        // GET /api/appointments/available-slots?doctorId=1&date=2026-05-15
        [HttpGet("available-slots")]
        [Authorize]
        public async Task<IActionResult> GetAvailableSlots([FromQuery] int doctorId, [FromQuery] DateTime date)
        {
            var doctor = await _db.DoctorProfiles.FindAsync(doctorId);
            if (doctor == null)
                return NotFound(new { message = "Doctor not found" });

            // Check if doctor is on approved leave
            var dateOnly = DateOnly.FromDateTime(date);
            var onLeave = await _db.DoctorLeaves
                .AnyAsync(l => l.DoctorProfileId == doctorId
                    && l.IsApproved
                    && l.StartDate <= dateOnly
                    && l.EndDate >= dateOnly);

            if (onLeave)
                return Ok(Array.Empty<object>());

            // Get doctor's availability for this day of week
            int dayOfWeek = (int)date.DayOfWeek;
            var availability = await _db.DoctorAvailabilities
                .Where(a => a.DoctorProfileId == doctorId
                    && a.DayOfWeek == dayOfWeek
                    && a.IsActive)
                .ToListAsync();

            if (!availability.Any())
                return Ok(Array.Empty<object>());

            // Get existing booked appointments for this doctor on this date (exclude cancelled)
            var booked = await _db.Appointments
                .Where(a => a.DoctorProfileId == doctorId
                    && a.AppointmentDate.Date == date.Date
                    && a.StatusId != 6) // not cancelled
                .Select(a => new { a.StartTime, a.EndTime })
                .ToListAsync();

            var slots = new List<object>();
            var duration = doctor.ConsultationDurationMin;
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var isToday = date.Date == DateTime.Today;

            foreach (var avail in availability)
            {
                var slotStart = avail.StartTime;
                while (true)
                {
                    var slotEnd = slotStart.AddMinutes(duration);
                    if (slotEnd > avail.EndTime)
                        break;

                    // Skip past slots if today
                    if (isToday && slotStart <= now)
                    {
                        slotStart = slotEnd;
                        continue;
                    }

                    // Check overlap with booked appointments
                    var isBooked = booked.Any(b => slotStart < b.EndTime && slotEnd > b.StartTime);

                    if (!isBooked)
                    {
                        slots.Add(new
                        {
                            StartTime = slotStart.ToString("HH:mm"),
                            EndTime = slotEnd.ToString("HH:mm")
                        });
                    }

                    slotStart = slotEnd;
                }
            }

            return Ok(slots);
        }

        // POST /api/appointments
        [HttpPost]
        [Authorize(Roles = "Patient,Receptionist")]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            int patientProfileId;

            if (userRole == "Patient")
            {
                var patient = await _db.PatientProfiles
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient == null)
                    return BadRequest(new { message = "Patient profile not found" });
                patientProfileId = patient.Id;
            }
            else
            {
                // Receptionist must provide PatientProfileId
                if (request.PatientProfileId == null || request.PatientProfileId <= 0)
                    return BadRequest(new { message = "PatientProfileId is required for receptionist booking" });
                patientProfileId = request.PatientProfileId.Value;
            }

            // Validate doctor exists and is active
            var doctor = await _db.DoctorProfiles.FindAsync(request.DoctorProfileId);
            if (doctor == null || !doctor.IsActive)
                return BadRequest(new { message = "Doctor not found or not active" });

            // Validate specialization exists for this doctor
            var hasSpec = await _db.DoctorSpecializations
                .AnyAsync(ds => ds.DoctorProfileId == request.DoctorProfileId
                    && ds.SpecializationId == request.SpecializationId);
            if (!hasSpec)
                return BadRequest(new { message = "Doctor does not have this specialization" });

            // Parse times
            var startTime = TimeOnly.Parse(request.StartTime);
            var endTime = startTime.AddMinutes(doctor.ConsultationDurationMin);

            // Double-booking check
            var conflict = await _db.Appointments
                .AnyAsync(a => a.DoctorProfileId == request.DoctorProfileId
                    && a.AppointmentDate.Date == request.AppointmentDate.Date
                    && a.StatusId != 6
                    && a.StartTime < endTime
                    && a.EndTime > startTime);

            if (conflict)
                return Conflict(new { message = "This time slot is already booked" });

            var appointment = new Appointment
            {
                PatientProfileId = patientProfileId,
                DoctorProfileId = request.DoctorProfileId,
                SpecializationId = request.SpecializationId,
                BookedById = userId!,
                BookedByRole = userRole!,
                AppointmentDate = request.AppointmentDate.Date,
                StartTime = startTime,
                EndTime = endTime,
                StatusId = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _db.Appointments.Add(appointment);

            // Add initial status history
            _db.AppointmentStatusHistories.Add(new AppointmentStatusHistory
            {
                Appointment = appointment,
                PreviousStatusId = null,
                NewStatusId = 1,
                ChangedAt = DateTime.UtcNow,
                ChangedById = userId!,
                Notes = "Appointment created"
            });

            await _db.SaveChangesAsync();

            return Created($"/api/appointments/{appointment.Id}", new
            {
                appointment.Id,
                appointment.AppointmentDate,
                appointment.StartTime,
                appointment.EndTime,
                Status = "Requested"
            });
        }

        // PUT /api/appointments/{id}/status
        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
        {
            var appointment = await _db.Appointments
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return NotFound(new { message = "Appointment not found" });

            // Validate state transitions
            var validTransitions = new Dictionary<int, int[]>
            {
                { 1, new[] { 2, 6 } },  // Requested -> Confirmed, Cancelled
                { 2, new[] { 3, 6 } },  // Confirmed -> CheckedIn, Cancelled
                { 3, new[] { 5 } }       // CheckedIn -> Completed
            };

            if (!validTransitions.ContainsKey(appointment.StatusId)
                || !validTransitions[appointment.StatusId].Contains(request.NewStatusId))
            {
                return BadRequest(new { message = $"Cannot transition from {appointment.Status.Name} to status {request.NewStatusId}" });
            }

            var previousStatusId = appointment.StatusId;
            appointment.StatusId = request.NewStatusId;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (request.NewStatusId == 6)
                appointment.CancellationReason = request.CancellationReason;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _db.AppointmentStatusHistories.Add(new AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatusId = previousStatusId,
                NewStatusId = request.NewStatusId,
                ChangedAt = DateTime.UtcNow,
                ChangedById = userId!,
                Notes = request.Notes
            });

            await _db.SaveChangesAsync();

            return Ok(new { message = "Status updated successfully" });
        }
    }

    public class CreateAppointmentRequest
    {
        public int? PatientProfileId { get; set; }
        public int DoctorProfileId { get; set; }
        public int SpecializationId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string StartTime { get; set; } = "";
    }

    public class UpdateStatusRequest
    {
        public int NewStatusId { get; set; }
        public string? CancellationReason { get; set; }
        public string? Notes { get; set; }
    }
}
