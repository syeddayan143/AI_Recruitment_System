
using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using AIRecruitmentSystem.Services;
using AIRecruitmentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class CandidateJobsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JobMatchingService _jobMatchingService;

        public CandidateJobsController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            JobMatchingService jobMatchingService)
        {
            _context = context;
            _userManager = userManager;
            _jobMatchingService = jobMatchingService;
        }

        // GET: /CandidateJobs
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var candidateSkills = await _context.Resumes
                .Where(r => r.CandidateId == user.Id)
                .OrderByDescending(r => r.UploadedAt)
                .Select(r => r.DetectedSkills)
                .FirstOrDefaultAsync();

            bool hasResume = !string.IsNullOrWhiteSpace(candidateSkills);

            var activeJobs = await _context.Jobs
                .AsNoTracking()
                .Where(j => j.IsActive)
                .OrderByDescending(j => j.PostedAt)
                .ToListAsync();

            var viewModel = new CandidateJobsIndexViewModel
            {
                HasResume = hasResume,
                CandidateSkills = candidateSkills ?? string.Empty
            };

            foreach (var job in activeJobs)
            {
                var requirements = string.IsNullOrWhiteSpace(job.Requirements)
                    ? job.Description
                    : job.Requirements;

                JobMatchResult result;

                if (hasResume)
                {
                    result = _jobMatchingService.CalculateMatch(
                        candidateSkills!,
                        requirements);
                }
                else
                {
                    result = new JobMatchResult
                    {
                        MatchPercentage = 0,
                        Message = "Upload a resume to see your skill match."
                    };
                }

                viewModel.Jobs.Add(new CandidateJobMatchViewModel
                {
                    Id = job.Id,
                    Title = job.Title,
                    Department = job.Department,
                    Location = job.Location,
                    Description = job.Description,
                    Requirements = job.Requirements,
                    EmploymentType = job.EmploymentType,
                    MinimumSalary = job.MinimumSalary,
                    MaximumSalary = job.MaximumSalary,
                    PostedAt = job.PostedAt,
                    MatchPercentage = result.MatchPercentage,
                    MatchedSkills = result.MatchedSkills,
                    MissingSkills = result.MissingSkills,
                    MatchMessage = result.Message
                });
            }

            viewModel.Jobs = viewModel.Jobs
                .OrderByDescending(j => j.MatchPercentage)
                .ThenByDescending(j => j.PostedAt)
                .ToList();

            return View(viewModel);
        }

        // GET: /CandidateJobs/ApplyForm/5
        [HttpGet]
        public async Task<IActionResult> ApplyForm(int jobId)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            var job = await _context.Jobs
                .AsNoTracking()
                .FirstOrDefaultAsync(j => j.Id == jobId && j.IsActive);

            if (job == null)
            {
                TempData["Error"] = "This job is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            bool alreadyApplied = await _context.JobApplications
                .AnyAsync(a => a.CandidateId == user.Id && a.JobId == job.Id);

            if (alreadyApplied)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction(nameof(Index));
            }

            var profile = await _context.CandidateProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            var resumes = await _context.Resumes
                .AsNoTracking()
                .Where(r => r.CandidateId == user.Id)
                .OrderByDescending(r => r.UploadedAt)
                .Select(r => new ResumeOptionViewModel
                {
                    Id = r.Id,
                    FileName = r.OriginalFileName,
                    UploadedAt = r.UploadedAt
                })
                .ToListAsync();

            if (resumes.Count == 0)
            {
                TempData["Error"] = "Please upload your resume before applying.";
                return RedirectToAction("Index", "Resume");
            }

            var model = new JobApplicationFormViewModel
            {
                JobId = job.Id,
                JobTitle = job.Title,
                Department = job.Department,
                CompanyOrRecruiter = await GetRecruiterDisplayNameAsync(job.RecruiterId),
                JobLocation = job.Location,
                EmploymentType = job.EmploymentType,
                JobDescription = job.Description,
                MinimumSalary = job.MinimumSalary,
                MaximumSalary = job.MaximumSalary,

                FullName = user.FullName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber,

                CurrentJobTitle = profile?.CurrentJobTitle,
                HighestEducation = profile?.HighestEducation,
                ExperienceYears = profile?.ExperienceYears ?? 0,
                PreferredLocation = profile?.PreferredLocation,
                Skills = profile?.Skills,
                LinkedInUrl = profile?.LinkedInUrl,
                PortfolioUrl = profile?.PortfolioUrl,

                ResumeId = resumes.FirstOrDefault()?.Id,
                Resumes = resumes
            };

            return View(model);
        }

        // POST: /CandidateJobs/SubmitApplication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitApplication(
            JobApplicationFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Challenge();

            // Always retrieve job details from the database.
            var job = await _context.Jobs
                .FirstOrDefaultAsync(j => j.Id == model.JobId && j.IsActive);

            if (job == null)
            {
                TempData["Error"] = "This job is no longer available.";
                return RedirectToAction(nameof(Index));
            }

            bool alreadyApplied = await _context.JobApplications
                .AnyAsync(a =>
                    a.CandidateId == user.Id &&
                    a.JobId == job.Id);

            if (alreadyApplied)
            {
                TempData["Error"] = "You have already applied for this job.";
                return RedirectToAction(nameof(Index));
            }

            // Verify that the selected resume belongs to this candidate.
            var selectedResume = model.ResumeId.HasValue
                ? await _context.Resumes.FirstOrDefaultAsync(r =>
                    r.Id == model.ResumeId.Value &&
                    r.CandidateId == user.Id)
                : null;

            if (selectedResume == null)
            {
                ModelState.AddModelError(
                    nameof(model.ResumeId),
                    "Please select one of your uploaded resumes.");
            }

            if (!ModelState.IsValid)
            {
                await PopulateApplicationFormAsync(model, job, user);
                return View("ApplyForm", model);
            }

            // Update the candidate's saved profile with submitted details.
            user.FullName = model.FullName.Trim();
            user.PhoneNumber = string.IsNullOrWhiteSpace(model.PhoneNumber)
                ? null
                : model.PhoneNumber.Trim();

            var userUpdate = await _userManager.UpdateAsync(user);

            if (!userUpdate.Succeeded)
            {
                foreach (var error in userUpdate.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateApplicationFormAsync(model, job, user);
                return View("ApplyForm", model);
            }

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    UserId = user.Id
                };

                _context.CandidateProfiles.Add(profile);
            }

            profile.CurrentJobTitle = model.CurrentJobTitle?.Trim();
            profile.HighestEducation = model.HighestEducation?.Trim();
            profile.ExperienceYears = model.ExperienceYears;
            profile.PreferredLocation = model.PreferredLocation?.Trim();
            profile.Skills = model.Skills?.Trim();
            profile.LinkedInUrl = model.LinkedInUrl?.Trim();
            profile.PortfolioUrl = model.PortfolioUrl?.Trim();
            profile.UpdatedAt = DateTime.UtcNow;

            var requirements = string.IsNullOrWhiteSpace(job.Requirements)
                ? job.Description
                : job.Requirements;

            var matchResult = _jobMatchingService.CalculateMatch(
                selectedResume!.DetectedSkills,
                requirements);

            var application = new JobApplication
            {
                CandidateId = user.Id,
                JobId = job.Id,
                ResumeId = selectedResume.Id,
                Status = "Applied",
                MatchScore = matchResult.MatchPercentage,
                AppliedAt = DateTime.UtcNow
            };

            _context.JobApplications.Add(application);

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Your application was submitted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // Refill database-backed details and the candidate's resume choices.
        private async Task PopulateApplicationFormAsync(
            JobApplicationFormViewModel model,
            Job job,
            ApplicationUser user)
        {
            model.JobTitle = job.Title;
            model.Department = job.Department;
            model.CompanyOrRecruiter =
                await GetRecruiterDisplayNameAsync(job.RecruiterId);
            model.JobLocation = job.Location;
            model.EmploymentType = job.EmploymentType;
            model.JobDescription = job.Description;
            model.MinimumSalary = job.MinimumSalary;
            model.MaximumSalary = job.MaximumSalary;
            model.Email = user.Email ?? string.Empty;

            model.Resumes = await _context.Resumes
                .AsNoTracking()
                .Where(r => r.CandidateId == user.Id)
                .OrderByDescending(r => r.UploadedAt)
                .Select(r => new ResumeOptionViewModel
                {
                    Id = r.Id,
                    FileName = r.OriginalFileName,
                    UploadedAt = r.UploadedAt
                })
                .ToListAsync();
        }

        private async Task<string> GetRecruiterDisplayNameAsync(string recruiterId)
        {
            var recruiter = await _userManager.FindByIdAsync(recruiterId);

            if (recruiter == null)
                return "Recruiter";

            if (!string.IsNullOrWhiteSpace(recruiter.FullName))
                return recruiter.FullName;

            return recruiter.Email ?? "Recruiter";
        }
    }
}
