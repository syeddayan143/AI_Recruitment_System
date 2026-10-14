using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using AIRecruitmentSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Recruiter")]
    public class RecruiterDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;
        private readonly JobMatchingService _jobMatchingService;

        public RecruiterDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment,
            JobMatchingService jobMatchingService)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
            _jobMatchingService = jobMatchingService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var recruiterId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var jobs = _context.Jobs
                .Where(job => job.RecruiterId == recruiterId);

            var model = new RecruiterDashboardViewModel
            {
                TotalJobs = await jobs.CountAsync(),

                ActiveJobs = await jobs.CountAsync(
                    job => job.IsActive),

                TotalApplications = await _context.JobApplications
                    .CountAsync(application =>
                        application.Job != null &&
                        application.Job.RecruiterId == recruiterId),

                RecentJobs = await jobs
                    .OrderByDescending(job => job.PostedAt)
                    .Take(5)
                    .Select(job => new RecruiterRecentJobViewModel
                    {
                        Id = job.Id,
                        Title = job.Title,
                        Location = job.Location,
                        IsActive = job.IsActive,
                        PostedAt = job.PostedAt
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Applications()
        {
            var recruiterId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Include(application => application.Candidate)
                .Include(application => application.Job)
                .Where(application =>
                    application.Job != null &&
                    application.Job.RecruiterId == recruiterId)
                .OrderByDescending(application => application.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var recruiterId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var application = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Candidate)
                .Include(a => a.Job)
                .Include(a => a.Resume)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.Job != null &&
                    a.Job.RecruiterId == recruiterId);

            if (application == null)
                return NotFound();

            var candidateProfile = await _context.CandidateProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p =>
                    p.UserId == application.CandidateId);

            ViewBag.CandidateProfile = candidateProfile;

            if (application.Resume != null &&
                application.Resume.CandidateId != application.CandidateId)
            {
                application.Resume = null;
            }

            // Calculate the skill match without changing the database.
            if (application.Resume != null &&
                application.Job != null)
            {
                ViewBag.JobMatch = _jobMatchingService.CalculateMatch(
                    application.Resume.DetectedSkills,
                    application.Job.Requirements);
            }
            else
            {
                ViewBag.JobMatch = null;
            }

            ViewBag.Interviews = await _context.Interviews
                .AsNoTracking()
                .Where(i => i.JobApplicationId == application.Id)
                .OrderByDescending(i => i.ScheduledAt)
                .ToListAsync();

            return View(application);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateApplicationStatus(
            int id,
            string status)
        {
            var allowedStatuses = new[]
            {
                "Applied",
                "Under Review",
                "Shortlisted",
                "Interview Scheduled",
                "Rejected",
                "Hired"
            };

            if (string.IsNullOrWhiteSpace(status) ||
                !allowedStatuses.Contains(status))
            {
                TempData["Error"] = "Invalid application status.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var recruiterId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null ||
                application.Job == null ||
                application.Job.RecruiterId != recruiterId)
            {
                return NotFound();
            }

            application.Status = status;
            application.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Application status updated successfully.";

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleInterview(
            int applicationId,
            DateTime scheduledAt,
            string interviewType,
            string? meetingLink)
        {
            var recruiterId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var application = await _context.JobApplications
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a =>
                    a.Id == applicationId &&
                    a.Job != null &&
                    a.Job.RecruiterId == recruiterId);

            if (application == null)
                return NotFound();

            if (scheduledAt <= DateTime.Now)
            {
                TempData["Error"] =
                    "Please select a future interview date and time.";

                return RedirectToAction(
                    nameof(Details), new { id = applicationId });
            }

            var allowedInterviewTypes = new[]
            {
                "Online",
                "In-Person",
                "Phone"
            };

            if (string.IsNullOrWhiteSpace(interviewType) ||
                !allowedInterviewTypes.Contains(interviewType))
            {
                TempData["Error"] = "Please select a valid interview type.";

                return RedirectToAction(
                    nameof(Details), new { id = applicationId });
            }

            if (!string.IsNullOrWhiteSpace(meetingLink))
            {
                meetingLink = meetingLink.Trim();

                if (meetingLink.Length > 500)
                {
                    TempData["Error"] =
                        "The meeting link cannot exceed 500 characters.";

                    return RedirectToAction(
                        nameof(Details), new { id = applicationId });
                }

                if (!Uri.TryCreate(
                        meetingLink,
                        UriKind.Absolute,
                        out var meetingUri) ||
                    (meetingUri.Scheme != Uri.UriSchemeHttps &&
                     meetingUri.Scheme != Uri.UriSchemeHttp))
                {
                    TempData["Error"] =
                        "Please enter a valid HTTP or HTTPS meeting link.";

                    return RedirectToAction(
                        nameof(Details), new { id = applicationId });
                }
            }

            if (interviewType == "Online" &&
                string.IsNullOrWhiteSpace(meetingLink))
            {
                TempData["Error"] =
                    "Please enter the online meeting link.";

                return RedirectToAction(
                    nameof(Details), new { id = applicationId });
            }

            var interview = new Interview
            {
                JobApplicationId = application.Id,
                ScheduledAt = scheduledAt,
                InterviewType = interviewType,
                MeetingLink = string.IsNullOrWhiteSpace(meetingLink)
                    ? null
                    : meetingLink,
                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            _context.Interviews.Add(interview);

            application.Status = "Interview Scheduled";
            application.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Interview scheduled successfully.";

            return RedirectToAction(
                nameof(Details), new { id = applicationId });
        }

        [HttpGet]
        public async Task<IActionResult> ViewResume(int id)
        {
            var recruiterId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(recruiterId))
                return Challenge();

            var application = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.Resume)
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.Job != null &&
                    a.Job.RecruiterId == recruiterId);

            if (application == null)
                return NotFound();

            var resume = application.Resume;

            if (resume == null ||
                resume.CandidateId != application.CandidateId)
            {
                return NotFound(
                    "Resume is not available for this application.");
            }

            var storageFolder = Path.Combine(
                _environment.ContentRootPath,
                "PrivateUploads",
                "Resumes");

            var safeStoredFileName =
                Path.GetFileName(resume.StoredFileName);

            var fullPath = Path.Combine(
                storageFolder,
                safeStoredFileName);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(
                    "Resume file was not found on the server.");
            }

            var extension = Path.GetExtension(
                resume.OriginalFileName).ToLowerInvariant();

            if (extension == ".pdf")
            {
                Response.Headers["Content-Disposition"] =
                    "inline; filename=\"resume.pdf\"";

                return PhysicalFile(fullPath, "application/pdf");
            }

            if (extension == ".docx")
            {
                return PhysicalFile(
                    fullPath,
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    Path.GetFileName(resume.OriginalFileName));
            }

            return BadRequest("Unsupported resume file type.");
        }
    }

    public class RecruiterDashboardViewModel
    {
        public int TotalJobs { get; set; }

        public int ActiveJobs { get; set; }

        public int TotalApplications { get; set; }

        public List<RecruiterRecentJobViewModel> RecentJobs { get; set; }
            = new();
    }

    public class RecruiterRecentJobViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Location { get; set; }

        public bool IsActive { get; set; }

        public DateTime PostedAt { get; set; }
    }
}