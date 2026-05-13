using CareMVC.Models.ViewModels;
using CareMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareMVC.Controllers
{
    public class ReceptionistController : BaseController
    {
        private readonly IApiService _api;

        public ReceptionistController(IApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (!IsAuthenticated || UserRole != "Receptionist")
                return RedirectToLogin();

            var appointments = await _api.GetAsync<List<AppointmentViewModel>>(
                "/api/appointments/today", JwtToken);

            var list = appointments ?? new();

            var model = new ReceptionistDashboardViewModel
            {
                TodayAppointments = list,
                RequestedCount = list.Count(a => a.Status == "Requested"),
                ConfirmedCount = list.Count(a => a.Status == "Confirmed"),
                CheckedInCount = list.Count(a => a.Status == "CheckedIn"),
                CompletedCount = list.Count(a => a.Status == "Completed"),
                CancelledCount = list.Count(a => a.Status == "Cancelled")
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmAppointment(int id)
        {
            if (!IsAuthenticated || UserRole != "Receptionist")
                return RedirectToLogin();

            var response = await _api.PutRawAsync(
                $"/api/appointments/{id}/status",
                new { NewStatusId = 2, Notes = "Confirmed by receptionist" },
                JwtToken);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode ? "Appointment confirmed." : "Failed to confirm appointment.";

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CheckIn(int id)
        {
            if (!IsAuthenticated || UserRole != "Receptionist")
                return RedirectToLogin();

            var response = await _api.PutRawAsync(
                $"/api/appointments/{id}/status",
                new { NewStatusId = 3, Notes = "Patient checked in" },
                JwtToken);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode ? "Patient checked in." : "Failed to check in patient.";

            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> CancelAppointment(int id, string? reason)
        {
            if (!IsAuthenticated || UserRole != "Receptionist")
                return RedirectToLogin();

            var response = await _api.PutRawAsync(
                $"/api/appointments/{id}/status",
                new { NewStatusId = 6, CancellationReason = reason ?? "Cancelled by receptionist", Notes = "Cancelled by receptionist" },
                JwtToken);

            TempData[response.IsSuccessStatusCode ? "Success" : "Error"] =
                response.IsSuccessStatusCode ? "Appointment cancelled." : "Failed to cancel appointment.";

            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public IActionResult BookForPatient()
        {
            if (!IsAuthenticated || UserRole != "Receptionist")
                return RedirectToLogin();

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SearchPatient(string cpr)
        {
            if (!IsAuthenticated || UserRole != "Receptionist")
                return Unauthorized();

            var result = await _api.GetAsync<PatientSearchResult>(
                $"/api/patients/search?cpr={cpr}", JwtToken);

            if (result == null)
                return Json(new { found = false });

            return Json(new { found = true, patient = result });
        }
    }
}
