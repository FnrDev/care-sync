using CareMVC.Models.ViewModels;
using CareMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareMVC.Controllers
{
    public class TrackingController : Controller
    {
        private readonly IApiService _api;

        public TrackingController(IApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(new TrackingViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Index(TrackingViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.CPR))
            {
                model.ErrorMessage = "CPR is required.";
                return View(model);
            }

            var url = $"/api/appointments/lookup?cpr={model.CPR}";
            if (!string.IsNullOrWhiteSpace(model.RefNumber))
                url += $"&refNumber={model.RefNumber}";

            var result = await _api.GetAsync<TrackingResultViewModel>(url);

            if (result == null)
            {
                model.ErrorMessage = "No patient found with the provided CPR and reference number.";
                return View(model);
            }

            model.Result = result;
            return View(model);
        }
    }
}
