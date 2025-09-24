using System.Linq;
using CMCS.Prototype.Data;
using CMCS.Prototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS.Prototype.Controllers
{
    public class CoordinatorController : Controller
    {
        private readonly AppDbContext _db;
        public CoordinatorController(AppDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var pending = _db.Claims
                .Where(c => c.Status == DecisionStatus.Pending)
                .OrderByDescending(c => c.CreatedOn)
                .ToList();

            return View(pending);
        }
    }
}
