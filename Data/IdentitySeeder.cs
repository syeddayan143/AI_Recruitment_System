
using Microsoft.AspNetCore.Identity;
using AIRecruitmentSystem.Models;

namespace AIRecruitmentSystem.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
            RoleManager<IdentityRole> roleManager)
        {
            string[] roles =
            {
                "Admin",
                "Recruiter",
                "Candidate"
            };

            foreach (string role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(
                        new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        string errors = string.Join(
                            "; ",
                            result.Errors.Select(e => e.Description));

                        throw new InvalidOperationException(
                            $"Could not create role '{role}': {errors}");
                    }
                }
            }
        }

        public static async Task SetupAdminAsync(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            bool enabled = configuration.GetValue<bool>(
                "AdminBootstrap:Enabled");

            if (!enabled)
                return;

            string email = "sayeddayan5@gmail.com";

            string? password = configuration[
                "AdminBootstrap:Password"];

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "AdminBootstrap:Password is missing from User Secrets.");
            }

            var existingUser = await userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                bool isAdmin = await userManager.IsInRoleAsync(
                    existingUser, "Admin");

                if (!isAdmin)
                {
                    throw new InvalidOperationException(
                        "The admin email already belongs to a non-admin account. " +
                        "Verify the account before promoting it.");
                }

                return;
            }

            var admin = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = "Sayed Dayan",
                IsActive = true,
                EmailConfirmed = false,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await userManager.CreateAsync(
                admin, password);

            if (!createResult.Succeeded)
            {
                string errors = string.Join(
                    "; ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Admin account creation failed: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!roleResult.Succeeded)
            {
                string errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Admin role assignment failed: {errors}");
            }
        }
    }
}
