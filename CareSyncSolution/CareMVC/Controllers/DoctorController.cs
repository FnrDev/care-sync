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
    }
}