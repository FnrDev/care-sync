using Microsoft.AspNetCore.Mvc;
using ReportingApplication.Services;

namespace ReportingApplication.Controllers
{
    public class ReportsController : Controller
    {
        private readonly CareSyncApiService _apiService;

        public ReportsController(CareSyncApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login", "Auth");
            }
            //The controller receives the report data and sends it to the view.
            var appointmentStats =
                await _apiService.GetAppointmentStatsAsync(token);

            var doctorUtilization =
                await _apiService.GetDoctorUtilizationAsync(token);

            ViewBag.AppointmentStats = appointmentStats;
            ViewBag.DoctorUtilization = doctorUtilization;

            return View();
        }
    }
}