using CareMVC.Models.ViewModels;
using CareMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareMVC.Controllers
{
    public class BookingController : BaseController
    {
        private readonly IApiService _api;

        public BookingController(IApiService api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index(int? patientProfileId, string? patientName)
        {
            if (!IsAuthenticated || (UserRole != "Patient" && UserRole != "Receptionist"))
                return RedirectToLogin();

            var specializations = await _api.GetAsync<List<SpecializationItem>>(
                "/api/specializations", JwtToken);

            var model = new BookingViewModel
            {
                Specializations = specializations ?? new(),
                PatientProfileId = patientProfileId,
                PatientName = patientName
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctors(int specId)
        {
            if (!IsAuthenticated) return Unauthorized();

            var doctors = await _api.GetAsync<List<DoctorItem>>(
                $"/api/doctors/by-specialization/{specId}", JwtToken);

            return Json(doctors ?? new());
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, string date)
        {
            if (!IsAuthenticated) return Unauthorized();

            var slots = await _api.GetAsync<List<TimeSlotItem>>(
                $"/api/appointments/available-slots?doctorId={doctorId}&date={date}", JwtToken);

            return Json(slots ?? new());
        }

        [HttpPost]
        public async Task<IActionResult> Confirm(int doctorProfileId, int specializationId,
            DateTime appointmentDate, string startTime, int? patientProfileId)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var body = new
            {
                DoctorProfileId = doctorProfileId,
                SpecializationId = specializationId,
                AppointmentDate = appointmentDate,
                StartTime = startTime,
                PatientProfileId = patientProfileId
            };

            var response = await _api.PostRawAsync("/api/appointments", body, JwtToken);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Appointment booked successfully!";

                if (UserRole == "Receptionist")
                    return RedirectToAction("Dashboard", "Receptionist");

                return RedirectToAction("Dashboard", "Patient");
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["Error"] = error.Contains("already booked")
                ? "This time slot is already booked. Please choose another."
                : "Failed to book appointment. Please try again.";

            return RedirectToAction("Index");
        }
    }
}
