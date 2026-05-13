using CareMVC.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CareMVC.Controllers
{
    public abstract class BaseController : Controller
    {
        protected string? JwtToken => HttpContext.Session.GetString(SessionKeys.JwtToken);
        protected string? UserRole => HttpContext.Session.GetString(SessionKeys.UserRole);
        protected string? UserFullName => HttpContext.Session.GetString(SessionKeys.UserFullName);
        protected string? UserId => HttpContext.Session.GetString(SessionKeys.UserId);
        protected bool IsAuthenticated => !string.IsNullOrEmpty(JwtToken);

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            ViewBag.IsAuthenticated = IsAuthenticated;
            ViewBag.UserRole = UserRole;
            ViewBag.UserFullName = UserFullName;
            base.OnActionExecuting(context);
        }

        protected IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Account");
        }
    }
}
