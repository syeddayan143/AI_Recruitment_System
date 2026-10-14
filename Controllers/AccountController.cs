
using AIRecruitmentSystem.Models;
using AIRecruitmentSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AIRecruitmentSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private static readonly string[] AllowedRoles =
        {
            "Candidate",
            "Recruiter"
        };

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Public registration must never allow Admin creation.
            if (!AllowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(
                    nameof(model.Role),
                    "Please select a valid account type.");

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email.Trim(),
                Email = model.Email.Trim(),
                FullName = model.FullName.Trim(),
                IsActive = true,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            // Roles will be seeded by the controlled setup step.
            if (!await _userManager.IsInRoleAsync(user, model.Role))
            {
                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    model.Role);

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    foreach (var error in roleResult.Errors)
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);

                    return View(model);
                }
            }

            return RedirectToAction(nameof(RegisterSuccess));
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult RegisterSuccess()
        {
            return View(new AccountResultViewModel
            {
                Title = "Registration Submitted",
                Message = "Your account has been created. Sign in after your email and account verification requirements are completed.",
                IsSuccess = true
            });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(
                model.Email.Trim());

            if (user == null || !user.IsActive)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid login attempt or account unavailable.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid login attempt or account unavailable.");

                return View(model);
            }

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl)
                && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            if (await _userManager.IsInRoleAsync(user, "Admin"))
                return RedirectToAction("Index", "AdminDashboard");

            if (await _userManager.IsInRoleAsync(user, "Recruiter"))
                return RedirectToAction("Index", "RecruiterDashboard");

            return RedirectToAction("Index", "CandidateDashboard");
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View(new AccountResultViewModel
            {
                Title = "Access Denied",
                Message = "You do not have permission to access this page.",
                IsSuccess = false
            });
        }
    }
}
