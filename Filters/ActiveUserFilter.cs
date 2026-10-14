using AIRecruitmentSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AIRecruitmentSystem.Filters
{
    public class ActiveUserFilter : IAsyncActionFilter
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ActiveUserFilter(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var httpContext = context.HttpContext;

            // Allow anonymous requests such as Login and Register.
            if (httpContext.User.Identity?.IsAuthenticated != true)
            {
                await next();
                return;
            }

            var user = await _userManager.GetUserAsync(httpContext.User);

            if (user == null || !user.IsActive)
            {
                await _signInManager.SignOutAsync();

                context.Result = new RedirectToActionResult(
                    "Login",
                    "Account",
                    new { message = "Your account is inactive. Please contact the administrator." });

                return;
            }

            await next();
        }
    }
}