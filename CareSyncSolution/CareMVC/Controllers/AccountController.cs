using CareMVC.Helpers;
using CareMVC.Models.ViewModels;
using CareMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace CareMVC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly IApiService _api;

        public AccountController(IApiService api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (IsAuthenticated)
                return RedirectToDashboard();

            return View(new LoginViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var raw = await _api.PostRawAsync(
                "/api/auth/login",
                new { model.Email, model.Password });

            if (!raw.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            var json = await raw.Content.ReadAsStringAsync();
            var response = System.Text.Json.JsonSerializer.Deserialize<LoginResponse>(
                json, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (response == null)
            {
                ModelState.AddModelError("", "Invalid email or password");
                return View(model);
            }

            // Store in session
            HttpContext.Session.SetString(SessionKeys.JwtToken, response.Token);
            HttpContext.Session.SetString(SessionKeys.UserId, response.User.Id);
            HttpContext.Session.SetString(SessionKeys.UserEmail, response.User.Email);
            HttpContext.Session.SetString(SessionKeys.UserFullName, response.User.FullName);

            var role = response.User.Roles.FirstOrDefault() ?? "";
            HttpContext.Session.SetString(SessionKeys.UserRole, role);

            return RedirectToDashboard();
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        private IActionResult RedirectToDashboard()
        {
            return UserRole switch
            {
                "Patient" => RedirectToAction("Dashboard", "Patient"),
                "Receptionist" => RedirectToAction("Dashboard", "Receptionist"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}
