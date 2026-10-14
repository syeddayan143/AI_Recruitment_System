
using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class CandidateProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CandidateProfileController(
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

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    UserId = userId
                };
            }

            return View(profile);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CandidateProfile model)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Challenge();

            // The signed-in candidate can edit only their own profile.
            ModelState.Remove(nameof(model.UserId));
            ModelState.Remove(nameof(model.User));

            if (!ModelState.IsValid)
                return View(model);

            var profile = await _context.CandidateProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (profile == null)
            {
                profile = new CandidateProfile
                {
                    UserId = userId
                };

                _context.CandidateProfiles.Add(profile);
            }

            profile.CurrentJobTitle = model.CurrentJobTitle;
            profile.HighestEducation = model.HighestEducation;
            profile.ExperienceYears = model.ExperienceYears;
            profile.PreferredLocation = model.PreferredLocation;
            profile.Skills = model.Skills;
            profile.LinkedInUrl = model.LinkedInUrl;
            profile.PortfolioUrl = model.PortfolioUrl;
            profile.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your profile has been saved successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
