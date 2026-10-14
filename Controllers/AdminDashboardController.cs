
using AIRecruitmentSystem.Data;
using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public AdminDashboardController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users;

            var recruiters = await _userManager
                .GetUsersInRoleAsync("Recruiter");

            var candidates = await _userManager
                .GetUsersInRoleAsync("Candidate");

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await users.CountAsync(),

                ActiveUsers = await users
                    .CountAsync(user => user.IsActive),

                TotalRecruiters = recruiters.Count,

                TotalCandidates = candidates.Count,

                TotalJobs = await _context.Jobs.CountAsync(),

                TotalApplications = await _context.JobApplications.CountAsync(),

                RecentUsers = await users
                    .OrderByDescending(user => user.CreatedAt)
                    .Take(8)
                    .Select(user => new AdminRecentUserViewModel
                    {
                        FullName = user.FullName ?? "Unnamed User",
                        Email = user.Email ?? string.Empty,
                        CreatedAt = user.CreatedAt,
                        IsActive = user.IsActive
                    })
                    .ToListAsync()
            };

            return View(model);
        }
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }

        public int ActiveUsers { get; set; }

        public int TotalRecruiters { get; set; }

        public int TotalCandidates { get; set; }

        public int TotalJobs { get; set; }

        public int TotalApplications { get; set; }

        public List<AdminRecentUserViewModel> RecentUsers { get; set; }
            = new();
    }

    public class AdminRecentUserViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
