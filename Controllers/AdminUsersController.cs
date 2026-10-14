
using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIRecruitmentSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? search, string? role)
        {
            var users = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var rows = new List<AdminUserRowViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                if (!string.IsNullOrWhiteSpace(role) &&
                    !roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    bool matches =
                        (user.FullName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (user.Email?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false);

                    if (!matches)
                    {
                        continue;
                    }
                }

                rows.Add(new AdminUserRowViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName ?? "Unnamed User",
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    Roles = roles.ToList()
                });
            }

            ViewBag.Search = search;
            ViewBag.SelectedRole = role;

            return View(rows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Prevent accidental deactivation of the currently logged-in Admin.
            if (user.Id == _userManager.GetUserId(User))
            {
                TempData["Error"] = "You cannot deactivate your own account.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Prevent changing another Admin's status from this screen.
            if (roles.Contains("Admin"))
            {
                TempData["Error"] = "Admin accounts cannot be changed from this screen.";
                return RedirectToAction(nameof(Index));
            }

            user.IsActive = !user.IsActive;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                TempData["Error"] = string.Join(
                    " ",
                    result.Errors.Select(e => e.Description));

                return RedirectToAction(nameof(Index));
            }

            TempData["Success"] = user.IsActive
                ? "User account activated successfully."
                : "User account deactivated successfully.";

            return RedirectToAction(nameof(Index));
        }
    }

    public class AdminUserRowViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}
