using CareSyncAPI.Data;
using CareSyncAPI.Models;
using CareMVC.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareMVC.Controllers
{
    public class ClinicManagerController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public ClinicManagerController(ApplicationDbContext db)
        {
            _db = db;
        }

        private bool IsClinicManagerAuthorized() =>
            IsAuthenticated && UserRole == "Manager";

        // GET /ClinicManager/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var totalDoctors = await _db.DoctorProfiles.CountAsync(d => d.IsActive);
            var totalPatients = await _db.PatientProfiles.CountAsync();
            var todayAppointments = await _db.Appointments
                .CountAsync(a => a.AppointmentDate.Date == DateTime.Today);
            var pendingAppointments = await _db.Appointments
                .CountAsync(a => a.StatusId == 1 || a.StatusId == 2);

            ViewBag.TotalDoctors = totalDoctors;
            ViewBag.TotalPatients = totalPatients;
            ViewBag.TodayAppointments = todayAppointments;
            ViewBag.PendingAppointments = pendingAppointments;

            return View();
        }
        // GET /ClinicManager/Doctors
        public async Task<IActionResult> Doctors()
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var doctors = await _db.DoctorProfiles
                .Include(d => d.DoctorSpecializations)
                    .ThenInclude(ds => ds.Specialization)
                .ToListAsync();

            var userIds = doctors.Select(d => d.UserId).ToList();
            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.FullName);

            ViewBag.Users = users;
            return View(doctors);
        }

        // POST /ClinicManager/ToggleDoctorStatus
        [HttpPost]
        public async Task<IActionResult> ToggleDoctorStatus(int id)
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var doctor = await _db.DoctorProfiles
                .Include(d => d.DoctorAvailabilities)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound();

            doctor.IsActive = !doctor.IsActive;

            foreach (var avail in doctor.DoctorAvailabilities)
                avail.IsActive = doctor.IsActive;

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Doctor status updated to {(doctor.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Doctors");
        }

        // GET /ClinicManager/Users
        public async Task<IActionResult> Users()
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var users = await _db.Users.ToListAsync();

            // Get roles for each user
            var userRoles = new Dictionary<string, string>();
            foreach (var user in users)
            {
                var roles = await _db.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();
                userRoles[user.Id] = roles.FirstOrDefault() ?? "No Role";
            }

            // CPR number keyed by UserId — only patients have a profile row
            var patientCprs = await _db.PatientProfiles
                .ToDictionaryAsync(p => p.UserId, p => p.CPR);

            ViewBag.UserRoles  = userRoles;
            ViewBag.PatientCPRs = patientCprs;
            return View(users);
        }
        // GET /ClinicManager/Schedules
    public async Task<IActionResult> Schedules()
    {
        if (!IsClinicManagerAuthorized()) return RedirectToLogin();

        var doctors = await _db.DoctorProfiles
            .Include(d => d.DoctorAvailabilities)
            .ToListAsync();

        var userIds = doctors.Select(d => d.UserId).ToList();
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.FullName);

        ViewBag.Users = users;
        return View(doctors);
    }

    // POST /ClinicManager/UpdateAvailability
    [HttpPost]
    public async Task<IActionResult> UpdateAvailability(int doctorProfileId, int dayOfWeek,
        TimeOnly startTime, TimeOnly endTime, bool isActive)
    {
        if (!IsClinicManagerAuthorized()) return RedirectToLogin();

        var availability = await _db.DoctorAvailabilities
            .FirstOrDefaultAsync(a => a.DoctorProfileId == doctorProfileId
                                    && a.DayOfWeek == dayOfWeek);

        if (availability == null)
        {
            _db.DoctorAvailabilities.Add(new DoctorAvailability
            {
                DoctorProfileId = doctorProfileId,
                DayOfWeek = dayOfWeek,
                StartTime = startTime,
                EndTime = endTime,
                IsActive = isActive
            });
        }
        else
        {
            availability.StartTime = startTime;
            availability.EndTime = endTime;
            availability.IsActive = isActive;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Schedule updated successfully.";
        return RedirectToAction("Schedules");
    }

        // GET /ClinicManager/EditDoctor/5
        public async Task<IActionResult> EditDoctor(int id)
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var doctor = await _db.DoctorProfiles
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == doctor.UserId);
            var specializations = await _db.Specializations.ToListAsync();

            ViewBag.DoctorName = user?.FullName ?? "Unknown";
            ViewBag.Specializations = specializations;
            ViewBag.SelectedSpecs = doctor.DoctorSpecializations.Select(ds => ds.SpecializationId).ToList();

            return View(doctor);
        }

        // POST /ClinicManager/EditDoctor
        [HttpPost]
        public async Task<IActionResult> EditDoctor(int id, string licenseNumber,
            int consultationDurationMin, string bio, List<int> specializationIds)
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var doctor = await _db.DoctorProfiles
                .Include(d => d.DoctorSpecializations)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (doctor == null) return NotFound();

            doctor.LicenseNumber = licenseNumber;
            doctor.ConsultationDurationMin = consultationDurationMin;
            doctor.Bio = bio;

            // Update specializations
            _db.DoctorSpecializations.RemoveRange(doctor.DoctorSpecializations);
            foreach (var specId in specializationIds)
            {
                _db.DoctorSpecializations.Add(new DoctorSpecialization
                {
                    DoctorProfileId = doctor.Id,
                    SpecializationId = specId
                });
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = "Doctor updated successfully.";
            return RedirectToAction("Doctors");
        }
        // POST /ClinicManager/ToggleUserStatus
        [HttpPost]
        public async Task<IActionResult> ToggleUserStatus(string id)
        {
            if (!IsClinicManagerAuthorized()) return RedirectToLogin();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            user.IsActive = !user.IsActive;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"User status updated to {(user.IsActive ? "Active" : "Inactive")}.";
            return RedirectToAction("Users");
        }

    }
}