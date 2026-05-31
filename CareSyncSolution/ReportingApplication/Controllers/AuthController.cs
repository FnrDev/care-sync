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
            var token = await _apiService.LoginAsync(email, password);

            if (token == null)
            {
                ViewBag.Error = "Invalid email or password";
                return View();
            }

            HttpContext.Session.SetString("JwtToken", token);

            return RedirectToAction("Index", "Reports");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


    }
}