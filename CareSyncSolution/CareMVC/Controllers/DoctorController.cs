using CareSyncAPI.Data;
using CareMVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareMVC.Controllers
{
    public class DoctorController : BaseController
    {
        private readonly ApplicationDbContext _db;
        public DoctorController(ApplicationDbContext db)
        {
            _db = db;
        }

        private bool IsDoctorAuthorized() =>
            IsAuthenticated && UserRole == "Doctor";

        public async Task<IActionResult> Dashboard()
        {
            if (!IsDoctorAuthorized()) return RedirectToLogin();

            var doctorProfile = await _db.DoctorProfiles
                .FirstOrDefaultAsync(d => d.UserId == UserId);

            if (doctorProfile == null)
                return NotFound("Doctor profile not found.");

            var today = DateTime.Today;

            var todaysAppointments = await _db.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.Specialization)
                .Include(a => a.Status)
                .Include(a => a.VisitRecord)
                .Where(a => a.DoctorProfileId == doctorProfile.Id
                         && a.AppointmentDate.Date == today)
                .OrderBy(a => a.StartTime)
                .ToListAsync();

            var patientUserIds = todaysAppointments
                .Select(a => a.PatientProfile.UserId)
                .Distinct().ToList();

            var patientUsers = await _db.Users
                .Where(u => patientUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var appointmentRows = todaysAppointments.Select(a => new DashboardAppointmentRow
            {
                Id = a.Id,
                PatientName = patientUsers.GetValueOrDefault(a.PatientProfile.UserId, "Unknown"),
                PatientCPR = a.PatientProfile.CPR,
                PatientProfileId = a.PatientProfileId,
                Specialization = a.Specialization.Name,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                StatusName = a.Status.Name,
                HasVisitRecord = a.VisitRecord != null
            }).ToList();

            var patientGroups = await _db.Appointments
                .Include(a => a.PatientProfile)
                .Where(a => a.DoctorProfileId == doctorProfile.Id)
                .GroupBy(a => a.PatientProfileId)
                .Select(g => new
                {
                    PatientProfileId = g.Key,
                    UserId = g.First().PatientProfile.UserId,
                    CPR = g.First().PatientProfile.CPR,
                    PatientRefNumber = g.First().PatientProfile.PatientRefNumber,
                    TotalAppointments = g.Count(),
                    LastVisit = (DateTime?)g.Max(a => a.AppointmentDate)
                })
                .ToListAsync();

            var allPatientUserIds = patientGroups.Select(p => p.UserId).ToList();
            var allPatientUsers = await _db.Users
                .Where(u => allPatientUserIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            var patientRows = patientGroups.Select(p => new DashboardPatientRow
            {
                PatientProfileId = p.PatientProfileId,
                FullName = allPatientUsers.GetValueOrDefault(p.UserId, "Unknown"),
                CPR = p.CPR,
                PatientRefNumber = p.PatientRefNumber,
                TotalAppointments = p.TotalAppointments,
                LastVisit = p.LastVisit
            })
            .OrderByDescending(p => p.LastVisit)
            .ToList();

            var vm = new DoctorDashboardViewModel
            {
                DoctorFullName = UserFullName ?? "Doctor",
                LicenseNumber = doctorProfile.LicenseNumber,
                DoctorProfileId = doctorProfile.Id,
                TodaysAppointments = appointmentRows,
                MyPatients = patientRows
            };

            return View(vm);
        }

        // POST /Doctor/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int appointmentId, int newStatusId, string? cancellationReason)
        {
            if (!IsDoctorAuthorized()) return RedirectToLogin();

            var appointment = await _db.Appointments
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                TempData["Error"] = "Appointment not found.";
                return RedirectToAction("Dashboard");
            }

            // Doctor owns this appointment
            var doctorProfile = await _db.DoctorProfiles
                .FirstOrDefaultAsync(d => d.UserId == UserId);

            if (doctorProfile == null || appointment.DoctorProfileId != doctorProfile.Id)
            {
                TempData["Error"] = "Unauthorized.";
                return RedirectToAction("Dashboard");
            }

             // transitions a Doctor can make
            var allowedTransitions = new Dictionary<int, List<int>>
            {
                { 2, new List<int> { 4, 6 } },   // Confirmed → InProgress or Cancelled
                { 3, new List<int> { 4, 6 } },   // CheckedIn → InProgress or Cancelled
                { 4, new List<int> { 5 } },       // InProgress → Completed
            };

            if (!allowedTransitions.TryGetValue(appointment.StatusId, out var allowed)
                || !allowed.Contains(newStatusId))
            {
                TempData["Error"] = $"Invalid status transition from '{appointment.Status.Name}'.";
                return RedirectToAction("Dashboard");
            }

            var previousStatusId = appointment.StatusId;

            appointment.StatusId = newStatusId;
            appointment.UpdatedAt = DateTime.UtcNow;

            if (newStatusId == 6 && !string.IsNullOrWhiteSpace(cancellationReason))
                appointment.CancellationReason = cancellationReason;

            // Log history
            _db.AppointmentStatusHistories.Add(new CareSyncAPI.Models.AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatusId = previousStatusId,
                NewStatusId = newStatusId,
                ChangedAt = DateTime.UtcNow,
                ChangedById = UserId!,
                Notes = newStatusId == 6 ? cancellationReason : null
            });

            // Fire notification to patient
            _db.Notifications.Add(new CareSyncAPI.Models.Notification
            {
                UserId = (await _db.PatientProfiles
                    .FirstOrDefaultAsync(p => p.Id == appointment.PatientProfileId))!.UserId,
                Title = GetNotificationTitle(newStatusId),
                Message = GetNotificationMessage(newStatusId, UserFullName ?? "Your doctor"),
                Type = "AppointmentUpdate",
                RelatedEntityId = appointment.Id,
                RelatedEntityType = "Appointment",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Appointment status updated to '{GetStatusName(newStatusId)}'.";
            return RedirectToAction("Dashboard");
        }
        // GET /Doctor/CompleteVisit/5
        public async Task<IActionResult> CompleteVisit(int id)
        {
            if (!IsDoctorAuthorized()) return RedirectToLogin();

            var appointment = await _db.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.Specialization)
                .Include(a => a.Status)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null) return NotFound();

            var patientUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == appointment.PatientProfile.UserId);

            var vm = new VisitRecordViewModel
            {
                AppointmentId = appointment.Id,
                PatientName = patientUser?.FullName ?? "Unknown",
                PatientCPR = appointment.PatientProfile.CPR,
                Specialization = appointment.Specialization.Name
            };

            return View(vm);
        }

        // POST /Doctor/CompleteVisit
        [HttpPost]
        public async Task<IActionResult> CompleteVisit(VisitRecordViewModel vm)
        {
            if (!IsDoctorAuthorized()) return RedirectToLogin();

            if (!ModelState.IsValid)
                return View(vm);

            var appointment = await _db.Appointments
                .Include(a => a.PatientProfile)
                .FirstOrDefaultAsync(a => a.Id == vm.AppointmentId);

            if (appointment == null) return NotFound();

            var doctorProfile = await _db.DoctorProfiles
                .FirstOrDefaultAsync(d => d.UserId == UserId);

            if (doctorProfile == null) return NotFound();

            // Create Visit Record
            var visitRecord = new CareSyncAPI.Models.VisitRecord
            {
                AppointmentId = appointment.Id,
                DoctorProfileId = doctorProfile.Id,
                PatientProfileId = appointment.PatientProfileId,
                Diagnosis = vm.Diagnosis,
                DoctorNotes = vm.DoctorNotes,
                Treatment = vm.Treatment,
                CreatedAt = DateTime.UtcNow
            };

            _db.VisitRecords.Add(visitRecord);
            await _db.SaveChangesAsync();

            // Add Prescription if any 
            var validItems = vm.Items
                .Where(i => !string.IsNullOrWhiteSpace(i.MedicationName))
                .ToList();

            if (validItems.Any())
            {
                var prescription = new CareSyncAPI.Models.Prescription
                {
                    VisitRecordId = visitRecord.Id,
                    DoctorProfileId = doctorProfile.Id,
                    PatientProfileId = appointment.PatientProfileId,
                    DateIssued = DateTime.UtcNow,
                    Notes = vm.PrescriptionNotes
                };

                _db.Prescriptions.Add(prescription);
                await _db.SaveChangesAsync();

                foreach (var item in validItems)
                {
                    _db.PrescriptionItems.Add(new CareSyncAPI.Models.PrescriptionItem
                    {
                        PrescriptionId = prescription.Id,
                        MedicationName = item.MedicationName,
                        Dosage = item.Dosage,
                        Frequency = item.Frequency,
                        DurationDays = item.DurationDays,
                        Instructions = item.Instructions
                    });
                }
            }

            // Appointment Completed
            var previousStatusId = appointment.StatusId;
            appointment.StatusId = 5;
            appointment.UpdatedAt = DateTime.UtcNow;

            // Log history
            _db.AppointmentStatusHistories.Add(new CareSyncAPI.Models.AppointmentStatusHistory
            {
                AppointmentId = appointment.Id,
                PreviousStatusId = previousStatusId,
                NewStatusId = 5,
                ChangedAt = DateTime.UtcNow,
                ChangedById = UserId!,
                Notes = "Visit completed with record"
            });

            // Notify patient
            _db.Notifications.Add(new CareSyncAPI.Models.Notification
            {
                UserId = appointment.PatientProfile.UserId,
                Title = "Visit completed",
                Message = "Your visit is complete. Your records and prescription are ready.",
                Type = "VisitCompleted",
                RelatedEntityId = appointment.Id,
                RelatedEntityType = "Appointment",
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["Success"] = "Visit completed and record saved.";
            return RedirectToAction("Dashboard");
        }

        // GET /Doctor/VisitRecord/5
        public async Task<IActionResult> VisitRecord(int id)
        {
            if (!IsDoctorAuthorized()) return RedirectToLogin();

            var appointment = await _db.Appointments
                .Include(a => a.PatientProfile)
                .Include(a => a.Specialization)
                .Include(a => a.VisitRecord)
                    .ThenInclude(v => v!.Prescription)
                        .ThenInclude(p => p!.PrescriptionItems)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null || appointment.VisitRecord == null)
                return NotFound();

            var patientUser = await _db.Users
                .FirstOrDefaultAsync(u => u.Id == appointment.PatientProfile.UserId);

            ViewBag.PatientName = patientUser?.FullName ?? "Unknown";
            ViewBag.PatientCPR = appointment.PatientProfile.CPR;
            ViewBag.Specialization = appointment.Specialization.Name;
            ViewBag.AppointmentDate = appointment.AppointmentDate;

            return View(appointment.VisitRecord);
        }

        // ── Helpers ──
        private static string GetStatusName(int statusId) => statusId switch
        {
            4 => "InProgress",
            5 => "Completed",
            6 => "Cancelled",
            _ => "Unknown"
        };

        private static string GetNotificationTitle(int statusId) => statusId switch
        {
            4 => "Your visit has started",
            5 => "Visit completed",
            6 => "Appointment cancelled",
            _ => "Appointment update"
        };

        private static string GetNotificationMessage(int statusId, string doctorName) => statusId switch
        {
            4 => $"Dr. {doctorName} has started your visit.",
            5 => $"Your visit with Dr. {doctorName} is complete. Check your records for notes.",
            6 => $"Your appointment with Dr. {doctorName} has been cancelled.",
            _ => "Your appointment status has been updated."
        };
    }
}