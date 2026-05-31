using Microsoft.AspNetCore.Mvc;

namespace ReportingApplication.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            // If logged in -> go directly to reports dashboard
            var token = HttpContext.Session.GetString("JwtToken");

            if (!string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Index", "Reports");
            }

            // Otherwise go to login page
            return RedirectToAction("Login", "Auth");
        }
    }
}