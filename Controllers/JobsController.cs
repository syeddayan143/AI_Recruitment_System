using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class JobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<string?> GetRecruiterIdAsync()
        {
            return await Task.FromResult(_userManager.GetUserId(User));
        }

        // GET: Jobs
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var recruiterId = await GetRecruiterIdAsync();

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var jobs = await _context.Jobs
                .Where(j => j.RecruiterId == recruiterId)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            return View(jobs);
        }

        // GET: Jobs/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Job());
        }

        // POST: Jobs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Job job)
        {
            var recruiterId = await GetRecruiterIdAsync();

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            ModelState.Remove(nameof(Job.RecruiterId));
            ModelState.Remove(nameof(Job.Recruiter));
            ModelState.Remove(nameof(Job.PostedAt));
            ModelState.Remove(nameof(Job.IsActive));
            ModelState.Remove(nameof(Job.Applications));

            if (job.MinimumSalary.HasValue &&
                job.MaximumSalary.HasValue &&
                job.MaximumSalary < job.MinimumSalary)
            {
                ModelState.AddModelError(
                    nameof(Job.MaximumSalary),
                    "Maximum salary cannot be less than minimum salary.");
            }

            if (!ModelState.IsValid)
                return View(job);

            job.RecruiterId = recruiterId;
            job.PostedAt = DateTime.UtcNow;
            job.IsActive = true;

            _context.Jobs.Add(job);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job posted successfully!";

            return RedirectToAction(nameof(Index));
        }

        // GET: Jobs/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var recruiterId = await GetRecruiterIdAsync();

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.Id == id && j.RecruiterId == recruiterId);

            if (job == null)
                return NotFound();

            return View(job);
        }

        // POST: Jobs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Job model)
        {
            var recruiterId = await GetRecruiterIdAsync();

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            if (id != model.Id)
                return BadRequest();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.Id == id && j.RecruiterId == recruiterId);

            if (job == null)
                return NotFound();

            ModelState.Remove(nameof(Job.RecruiterId));
            ModelState.Remove(nameof(Job.Recruiter));
            ModelState.Remove(nameof(Job.PostedAt));
            ModelState.Remove(nameof(Job.IsActive));
            ModelState.Remove(nameof(Job.Applications));

            if (model.MinimumSalary.HasValue &&
                model.MaximumSalary.HasValue &&
                model.MaximumSalary < model.MinimumSalary)
            {
                ModelState.AddModelError(
                    nameof(Job.MaximumSalary),
                    "Maximum salary cannot be less than minimum salary.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Update only editable fields.
            job.Title = model.Title;
            job.Department = model.Department;
            job.Location = model.Location;
            job.Description = model.Description;
            job.Requirements = model.Requirements;
            job.MinimumSalary = model.MinimumSalary;
            job.MaximumSalary = model.MaximumSalary;
            job.EmploymentType = model.EmploymentType;
            job.ClosingDate = model.ClosingDate;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Job updated successfully!";

            return RedirectToAction(nameof(Index));
        }

        // POST: Jobs/ToggleStatus/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var recruiterId = await GetRecruiterIdAsync();

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var job = await _context.Jobs
                .FirstOrDefaultAsync(j =>
                    j.Id == id && j.RecruiterId == recruiterId);

            if (job == null)
                return NotFound();

            job.IsActive = !job.IsActive;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = job.IsActive
                ? "Job activated successfully!"
                : "Job deactivated successfully!";

            return RedirectToAction(nameof(Index));
        }
    }
}