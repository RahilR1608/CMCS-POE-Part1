using System;
using System.IO;                           // >>> for saving files
using System.Linq;
using System.Threading.Tasks;
using CMCS.Prototype.Data;
using CMCS.Prototype.Models;
using Microsoft.AspNetCore.Hosting;         // >>> IWebHostEnvironment
using Microsoft.AspNetCore.Http;            // >>> IFormFile
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CMCS.Prototype.Controllers
{
    public class ClaimsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;   // >>> hosting env to get wwwroot

        public ClaimsController(AppDbContext context, IWebHostEnvironment env)   // >>> add env
        {
            _context = context;
            _env = env;                                  // >>>
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

        // POST: /Claims/Decide  (Coordinator/Manager approve or reject)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decide(Guid id, DecisionStatus decision, string? comment)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            // Use the seeded Coordinator/Manager as approver for now
            var approver = await _context.Users
                .Where(u => u.Role != UserRole.Lecturer)
                .OrderBy(u => u.Role) // Coordinator first
                .FirstAsync();

            _context.Approvals.Add(new Approval
            {
                ApprovalId = Guid.NewGuid(),
                ClaimId = id,
                ApproverId = approver.UserId,
                Decision = decision,
                DecisionDate = DateTime.UtcNow,
                Comment = string.IsNullOrWhiteSpace(comment) ? "Reviewed" : comment
            });

            claim.Status = decision; // reflect latest decision on claim
            await _context.SaveChangesAsync();

            TempData["Msg"] = $"Claim {decision}.";
            return RedirectToAction("Index", "Coordinator");
        }

        // >>> POST: /Claims/Upload  (Document Upload with validation + error handling)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(Guid claimId, IFormFile file)
        {
            // Validate claim exists
            var claim = await _context.Claims.Include(c => c.Documents).FirstOrDefaultAsync(c => c.ClaimId == claimId);
            if (claim == null) return NotFound();

            // Basic validations
            if (file == null || file.Length == 0)
            {
                TempData["Err"] = "No file selected.";
                return RedirectToAction(nameof(Details), new { id = claimId });
            }

            // Allowed types + size
            var allowed = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".jpeg", ".png" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext))
            {
                TempData["Err"] = "Only PDF, DOCX, XLSX, JPG, JPEG, PNG files are allowed.";
                return RedirectToAction(nameof(Details), new { id = claimId });
            }
            if (file.Length > 5 * 1024 * 1024) // 5 MB
            {
                TempData["Err"] = "File too large. Maximum allowed size is 5 MB.";
                return RedirectToAction(nameof(Details), new { id = claimId });
            }

            try
            {
                // Ensure uploads folder exists
                var uploadsRoot = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsRoot);

                // Unique file name
                var uniqueName = $"{Guid.NewGuid()}{ext}";
                var savePath = Path.Combine(uploadsRoot, uniqueName);

                using (var stream = System.IO.File.Create(savePath))
                {
                    await file.CopyToAsync(stream);
                }

                // Save DB record
                _context.SupportingDocuments.Add(new SupportingDocument
                {
                    DocumentId = Guid.NewGuid(),
                    ClaimId = claimId,
                    FileName = file.FileName,
                    FilePath = $"/uploads/{uniqueName}",
                    UploadedOn = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();

                TempData["Msg"] = "File uploaded successfully.";
            }
            catch
            {
                TempData["Err"] = "Upload failed. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id = claimId });
        }
    }
}
