using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class CandidateDashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateDashboardController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrWhiteSpace(userId))
            {
                return Challenge();
            }

            var user = await _userManager.GetUserAsync(User);

            var profile = await _context.CandidateProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var model = new CandidateDashboardViewModel
            {
                FullName = user?.FullName ?? user?.Email ?? "Candidate",
                ProfileExists = profile != null,
                TotalActiveJobs = await _context.Jobs
                    .AsNoTracking()
                    .CountAsync(j => j.IsActive)
            };

            if (profile != null)
            {
                // JobApplication.CandidateId is a string.
                model.TotalApplications = await _context.JobApplications
                    .AsNoTracking()
                    .CountAsync(a => a.CandidateId == userId);

                // Resume.CandidateId is also a string.
                model.TotalResumes = await _context.Resumes
                    .AsNoTracking()
                    .CountAsync(r => r.CandidateId == userId);

                model.ProfileCompletion = CalculateProfileCompletion(profile);

                model.RecentApplications = await _context.JobApplications
                    .AsNoTracking()
                    .Where(a => a.CandidateId == userId)
                    .OrderByDescending(a => a.AppliedAt)
                    .Take(5)
                    .Select(a => new CandidateApplicationRowViewModel
                    {
                        JobTitle = a.Job != null
                            ? a.Job.Title
                            : "Job unavailable",

                        CompanyLocation = a.Job != null
                            ? a.Job.Location
                            : "Not specified",

                        Status = a.Status,
                        AppliedAt = a.AppliedAt,
                        MatchScore = a.MatchScore
                    })
                    .ToListAsync();
            }

            return View(model);
        }

        private static int CalculateProfileCompletion(
            CandidateProfile profile)
        {
            var totalFields = 6;
            var completedFields = 0;

            if (!string.IsNullOrWhiteSpace(profile.CurrentJobTitle))
                completedFields++;

            if (!string.IsNullOrWhiteSpace(profile.HighestEducation))
                completedFields++;

            // ExperienceYears is a non-nullable integer.
            // Zero years is valid for a fresher.
            if (profile.ExperienceYears >= 0)
                completedFields++;

            if (!string.IsNullOrWhiteSpace(profile.PreferredLocation))
                completedFields++;

            if (!string.IsNullOrWhiteSpace(profile.Skills))
                completedFields++;

            if (!string.IsNullOrWhiteSpace(profile.LinkedInUrl) ||
                !string.IsNullOrWhiteSpace(profile.PortfolioUrl))
            {
                completedFields++;
            }

            return (int)Math.Round(
                completedFields * 100.0 / totalFields);
        }
    }

    public class CandidateDashboardViewModel
    {
        public string FullName { get; set; } = "Candidate";

        public bool ProfileExists { get; set; }

        public int TotalActiveJobs { get; set; }

        public int TotalApplications { get; set; }

        public int TotalResumes { get; set; }

        public int ProfileCompletion { get; set; }

        public List<CandidateApplicationRowViewModel> RecentApplications
        {
            get;
            set;
        } = new();
    }

    public class CandidateApplicationRowViewModel
    {
        public string JobTitle { get; set; } = string.Empty;

        public string CompanyLocation { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime AppliedAt { get; set; }

        public decimal? MatchScore { get; set; }
    }
}