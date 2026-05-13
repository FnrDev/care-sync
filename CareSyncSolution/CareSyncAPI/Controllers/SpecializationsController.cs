using CareSyncAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareSyncAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SpecializationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SpecializationsController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/specializations
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var specializations = await _db.Specializations
                .OrderBy(s => s.Name)
                .Select(s => new { s.Id, s.Name, s.Description })
                .ToListAsync();

            return Ok(specializations);
        }
    }
}
