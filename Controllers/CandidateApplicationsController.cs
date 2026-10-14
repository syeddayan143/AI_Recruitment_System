
using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Candidate")]
    public class CandidateApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CandidateApplicationsController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var candidateId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(candidateId))
            {
                return Challenge();
            }

            var applications = await _context.JobApplications
                .AsNoTracking()
                .Include(a => a.Job)
                .Include(a => a.Resume)
                .Include(a => a.Interviews)
                .Where(a => a.CandidateId == candidateId)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync();

            return View(applications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RespondToInterview(
            int interviewId,
            string response)
        {
            var candidateId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(candidateId))
            {
                return Challenge();
            }

            if (response != "Confirmed" && response != "Declined")
            {
                TempData["InterviewResponseError"] =
                    "Invalid interview response.";

                return RedirectToAction(nameof(Index));
            }

            var interview = await _context.Interviews
                .Include(i => i.JobApplication)
                .FirstOrDefaultAsync(i => i.Id == interviewId);

            if (interview == null ||
                interview.JobApplication == null ||
                interview.JobApplication.CandidateId != candidateId)
            {
                return NotFound();
            }

            if (!string.Equals(
                interview.CandidateResponse,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["InterviewResponseError"] =
                    "You have already responded to this interview.";

                return RedirectToAction(nameof(Index));
            }

            interview.CandidateResponse = response;
            interview.CandidateRespondedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            TempData["InterviewResponseSuccess"] =
                response == "Confirmed"
                    ? "Interview confirmed successfully."
                    : "Interview declined successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
