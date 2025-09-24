using System.Linq;
using CMCS.Prototype.Data;
using CMCS.Prototype.Models;
using Microsoft.AspNetCore.Mvc;

namespace CMCS.Prototype.Controllers
{
    public class ManagerController : Controller
    {
        private readonly AppDbContext _db;
        public ManagerController(AppDbContext db) { _db = db; }

        public IActionResult Index()
        {
            var vm = new ManagerVm
            {
                Pending = _db.Claims.Where(c => c.Status == DecisionStatus.Pending)
                                     .OrderByDescending(c => c.CreatedOn).ToList(),
                Approved = _db.Claims.Where(c => c.Status == DecisionStatus.Approved)
                                     .OrderByDescending(c => c.CreatedOn).ToList(),
                Rejected = _db.Claims.Where(c => c.Status == DecisionStatus.Rejected)
                                     .OrderByDescending(c => c.CreatedOn).ToList()
            };
            return View(vm);
        }
    }

    // simple view model in same file for convenience
    public class ManagerVm
    {
        public List<Claim> Pending { get; set; } = new();
        public List<Claim> Approved { get; set; } = new();
        public List<Claim> Rejected { get; set; } = new();
    }
}
