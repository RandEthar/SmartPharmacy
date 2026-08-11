using Microsoft.AspNetCore.Identity;
using SmartPharmacy.DAL.Models;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.SeedData
{
    public class RoleSeedData : ISeedData
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public RoleSeedData(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }
        public async Task SeedData()
        {
            // Checked per role rather than "are there any roles at all", so a role added
            // later still gets created on an existing database.
            foreach (var role in Roles.All)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole { Name = role });
                }
            }
        }
    }
}
