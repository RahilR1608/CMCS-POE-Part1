using System.Text;
using CMCS.Prototype.Data;
using CMCS.Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Prototype.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _db;
        public HRController(AppDbContext db) => _db = db;

        // GET: /HR
        public async Task<IActionResult> Index()
        {
            var report = await _db.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == DecisionStatus.Approved)
                .GroupBy(c => new { Lecturer = c.Lecturer.FullName, c.Month, c.Year })
                .Select(g => new HRClaimSummaryVM
                {
                    Lecturer = g.Key.Lecturer,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Claims = g.Count(),
                    TotalHours = g.Sum(x => x.HoursWorked),
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.Month)
                .ThenBy(x => x.Lecturer)
                .ToListAsync();

            return View(report);
        }

        // GET: /HR/ExportCsv
        public async Task<FileResult> ExportCsv()
        {
            var data = await _db.Claims
                .Include(c => c.Lecturer)
                .Where(c => c.Status == DecisionStatus.Approved)
                .GroupBy(c => new { Lecturer = c.Lecturer.FullName, c.Month, c.Year })
                .Select(g => new HRClaimSummaryVM
                {
                    Lecturer = g.Key.Lecturer,
                    Month = g.Key.Month,
                    Year = g.Key.Year,
                    Claims = g.Count(),
                    TotalHours = g.Sum(x => x.HoursWorked),
                    TotalAmount = g.Sum(x => x.TotalAmount)
                })
                .OrderBy(x => x.Lecturer)
                .ThenBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("Lecturer,Month,Year,Claims,TotalHours,TotalAmount");
            foreach (var r in data)
                sb.AppendLine($"{r.Lecturer},{r.Month},{r.Year},{r.Claims},{r.TotalHours:0.##},{r.TotalAmount:0.00}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var fileName = $"CMCS_ApprovedClaims_{DateTime.UtcNow:yyyyMMdd}.csv";
            return File(bytes, "text/csv", fileName);
        }
    }

    // ViewModel kept here for convenience
    public class HRClaimSummaryVM
    {
        public string Lecturer { get; set; } = "";
        public int Month { get; set; }
        public int Year { get; set; }
        public int Claims { get; set; }
        public decimal TotalHours { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
