using Microsoft.AspNetCore.Mvc;
using ReportingApplication.Services;

namespace ReportingApplication.Controllers
{
    public class AuthController : Controller
    {
        private readonly CareSyncApiService _apiService;

        public AuthController(CareSyncApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var result = await _apiService.LoginAsync(email, password);

            if (result == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            // Reporting is restricted to clinic managers. Reject anyone else
            // before issuing a session so they get a clear message here rather
            // than empty/failed report panels after logging in.
            if (!result.User.Roles.Contains("Manager"))
            {
                ViewBag.Error = "You do not have permission to access the reporting dashboard. This area is restricted to clinic managers.";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", result.Token);

            return RedirectToAction("Index", "Reports");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


    }
}