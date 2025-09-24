using System;
using System.Linq;
using System.Threading.Tasks;
using CMCS.Prototype.Data;
using CMCS.Prototype.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Prototype.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly AppDbContext _context;

        public ClaimsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Claims
        public async Task<IActionResult> Index()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .OrderByDescending(c => c.CreatedOn)
                .ToListAsync();

            return View(claims);
        }

        // GET: /Claims/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.Documents)
                .Include(c => c.Approvals)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null) return NotFound();
            return View(claim);
        }

        // GET: /Claims/Create
        public IActionResult Create()
        {
            ViewData["LecturerId"] = new SelectList(
                _context.Users.Where(u => u.Role == UserRole.Lecturer),
                "UserId", "FullName"
            );
            return View(new Claim { Year = DateTime.UtcNow.Year, Month = DateTime.UtcNow.Month });
        }

        // POST: /Claims/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LecturerId,Month,Year,HoursWorked,HourlyRate")] Claim claim)
        {
            if (!ModelState.IsValid)
            {
                ViewData["LecturerId"] = new SelectList(
                    _context.Users.Where(u => u.Role == UserRole.Lecturer),
                    "UserId", "FullName", claim.LecturerId
                );
                return View(claim);
            }

            claim.ClaimId = Guid.NewGuid();
            claim.CreatedOn = DateTime.UtcNow;
            claim.Status = DecisionStatus.Pending;
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;

            _context.Add(claim);
            await _context.SaveChangesAsync();
            TempData["Msg"] = "Claim created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Claims/Edit/{id}
        public async Task<IActionResult> Edit(Guid id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            ViewData["LecturerId"] = new SelectList(
                _context.Users.Where(u => u.Role == UserRole.Lecturer),
                "UserId", "FullName", claim.LecturerId
            );
            return View(claim);
        }

        // POST: /Claims/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ClaimId,LecturerId,Month,Year,HoursWorked,HourlyRate,Status,CreatedOn")] Claim claim)
        {
            if (id != claim.ClaimId) return NotFound();
            if (!ModelState.IsValid)
            {
                ViewData["LecturerId"] = new SelectList(
                    _context.Users.Where(u => u.Role == UserRole.Lecturer),
                    "UserId", "FullName", claim.LecturerId
                );
                return View(claim);
            }

            // Recalculate total on edit
            claim.TotalAmount = claim.HoursWorked * claim.HourlyRate;
            _context.Update(claim);
            await _context.SaveChangesAsync();

            TempData["Msg"] = "Claim updated.";
            return RedirectToAction(nameof(Index));
        }
    }
}
