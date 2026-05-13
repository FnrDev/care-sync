using CareMVC.Models.ViewModels;
using CareMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareMVC.Controllers
{
    public class PatientController : BaseController
    {
        private readonly IApiService _api;

        public PatientController(IApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated || UserRole != "Patient")
                return RedirectToLogin();

            var profile = await _api.GetAsync<PatientProfileViewModel>("/api/patients/me", JwtToken);
            var appointments = await _api.GetAsync<List<AppointmentViewModel>>("/api/appointments/my", JwtToken);

            var model = new PatientDashboardViewModel
            {
                Profile = profile ?? new PatientProfileViewModel(),
                UpcomingAppointments = appointments?
                    .Where(a => a.AppointmentDate.Date >= DateTime.Today && a.Status != "Cancelled")
                    .OrderBy(a => a.AppointmentDate)
                    .ToList() ?? new(),
                PastAppointments = appointments?
                    .Where(a => a.AppointmentDate.Date < DateTime.Today || a.Status == "Completed" || a.Status == "Cancelled")
                    .OrderByDescending(a => a.AppointmentDate)
                    .Take(10)
                    .ToList() ?? new()
            };

            return View(model);
        }

        public async Task<IActionResult> MedicalRecords()
        {
            if (!IsAuthenticated || UserRole != "Patient")
                return RedirectToLogin();

            var records = await _api.GetAsync<List<MedicalRecordViewModel>>(
                "/api/patients/me/medical-records", JwtToken);

            return View(records ?? new List<MedicalRecordViewModel>());
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            if (!IsAuthenticated || UserRole != "Patient")
                return RedirectToLogin();

            var response = await _api.PutRawAsync(
                $"/api/appointments/{id}/status",
                new { NewStatusId = 6, CancellationReason = "Cancelled by patient" },
                JwtToken);

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Appointment cancelled successfully.";
            else
                TempData["Error"] = "Failed to cancel appointment.";

            return RedirectToAction("Dashboard");
        }
    }
}
