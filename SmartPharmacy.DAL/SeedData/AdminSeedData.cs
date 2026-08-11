using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartPharmacy.DAL.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.SeedData
{
    /// <summary>
    /// Creates the very first Admin account. Without it the admin-only endpoints would be
    /// unreachable, since every user registered through the API starts out as a Patient.
    /// Credentials come from configuration (use dotnet user-secrets, never appsettings.json).
    /// </summary>
    public class AdminSeedData : ISeedData
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminSeedData> _logger;

        public AdminSeedData(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            ILogger<AdminSeedData> logger)
        {
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SeedData()
        {
            var admins = await _userManager.GetUsersInRoleAsync(Roles.Admin);
            if (admins.Any())
                return;

            var email = _configuration["AdminUser:Email"];
            var password = _configuration["AdminUser:Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning(
                    "No Admin account exists and AdminUser:Email / AdminUser:Password are not configured. " +
                    "Set them with dotnet user-secrets, otherwise the admin-only endpoints stay unreachable.");
                return;
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null)
            {
                // The account is already there but not in the Admin role yet (e.g. it registered
                // through the API first) - promote it instead of failing on a duplicate email.
                await _userManager.AddToRoleAsync(existing, Roles.Admin);
                _logger.LogInformation("Promoted existing user {Email} to Admin.", email);
                return;
            }

            var admin = new ApplicationUser
            {
                FullName = _configuration["AdminUser:FullName"] ?? "System Administrator",
                UserName = _configuration["AdminUser:UserName"] ?? email,
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
            {
                _logger.LogError(
                    "Failed to create the seeded Admin account: {Errors}",
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRoleAsync(admin, Roles.Admin);
            _logger.LogInformation("Seeded the initial Admin account {Email}.", email);
        }
    }
}
