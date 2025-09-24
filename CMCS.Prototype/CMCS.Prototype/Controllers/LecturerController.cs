using System.Linq;
using System.Threading.Tasks;
using CMCS.Prototype.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Prototype.Controllers
{
    public class LecturerController : Controller
    {
        private readonly AppDbContext _db;
        public LecturerController(AppDbContext db) { _db = db; }

        // Shows real claims from the database (latest first).
        public async Task<IActionResult> Index()
        {
            var claims = await _db.Claims
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();

            return View(claims); // Pass the list to the view
        }
    }
}
