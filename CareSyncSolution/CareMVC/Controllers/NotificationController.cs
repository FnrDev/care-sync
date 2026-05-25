using CareSyncAPI.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareMVC.Controllers
{
    public class NotificationController : BaseController
    {
        private readonly ApplicationDbContext _db;

        public NotificationController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /Notification/Index
        public async Task<IActionResult> Index()
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var notifications = await _db.Notifications
                .Where(n => n.UserId == UserId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return View(notifications);
        }

        // POST /Notification/MarkAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId);

            if (notification != null)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

        // POST /Notification/MarkAllAsRead
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            if (!IsAuthenticated) return RedirectToLogin();

            var unread = await _db.Notifications
                .Where(n => n.UserId == UserId && !n.IsRead)
                .ToListAsync();

            unread.ForEach(n => n.IsRead = true);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}