using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using AIRecruitmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class JobMatchingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly GroqJobMatchingService _matchingService;

        public JobMatchingController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            GroqJobMatchingService matchingService)
        {
            _context = context;
            _userManager = userManager;
            _matchingService = matchingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            var model = await BuildPageModelAsync(userId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Analyze(
            JobMatchingPageViewModel model,
            CancellationToken cancellationToken)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
                return Challenge();

            // Verify that the selected resume belongs to this candidate.
            var resume = await _context.Resumes
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    r => r.Id == model.ResumeId &&
                         r.CandidateId == userId,
                    cancellationToken);

            // Only active jobs can be compared.
            var job = await _context.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    j => j.Id == model.JobId && j.IsActive,
                    cancellationToken);

            if (resume == null || job == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select one of your own resumes and an active job.");

                var invalidModel = await BuildPageModelAsync(userId);
                invalidModel.ResumeId = model.ResumeId;
                invalidModel.JobId = model.JobId;

                return View("Index", invalidModel);
            }

            if (string.IsNullOrWhiteSpace(resume.ExtractedText))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected resume has no readable text. " +
                    "Please upload a text-based PDF or DOCX resume.");

                var emptyResumeModel = await BuildPageModelAsync(userId);
                emptyResumeModel.ResumeId = resume.Id;
                emptyResumeModel.JobId = job.Id;

                return View("Index", emptyResumeModel);
            }

            try
            {
                var result = await _matchingService.AnalyzeAsync(
                    resume.ExtractedText,
                    job.Title,
                    job.Description,
                    job.Requirements,
                    cancellationToken);

                var resultModel = await BuildPageModelAsync(userId);

                resultModel.ResumeId = resume.Id;
                resultModel.JobId = job.Id;
                resultModel.SelectedResumeName = resume.OriginalFileName;
                resultModel.SelectedJobTitle = job.Title;
                resultModel.Result = result;

                return View("Index", resultModel);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "AI job matching could not be completed. Check the Groq " +
                    "configuration, model availability, internet connection " +
                    "and account quota.");

                var errorModel = await BuildPageModelAsync(userId);
                errorModel.ResumeId = resume.Id;
                errorModel.JobId = job.Id;

                return View("Index", errorModel);
            }
        }

        private async Task<JobMatchingPageViewModel> BuildPageModelAsync(
            string userId)
        {
            var resumes = await _context.Resumes
                .AsNoTracking()
                .Where(r => r.CandidateId == userId)
                .OrderByDescending(r => r.UploadedAt)
                .Select(r => new
                {
                    r.Id,
                    r.OriginalFileName
                })
                .ToListAsync();

            var jobs = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Department,
                    j.Location
                })
                .ToListAsync();

            return new JobMatchingPageViewModel
            {
                Resumes = resumes.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.OriginalFileName
                }).ToList(),

                Jobs = jobs.Select(j => new SelectListItem
                {
                    Value = j.Id.ToString(),
                    Text = $"{j.Title} — {j.Department} ({j.Location})"
                }).ToList()
            };
        }
    }
}