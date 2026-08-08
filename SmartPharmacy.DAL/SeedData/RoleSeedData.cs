using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            string[] roles =
             {
                "Patient",
                "Admin",
                "Pharmacist"
            };
            // Check if roles already exist in the database
            if (!await _roleManager.Roles.AnyAsync())
            {
                foreach (var item in roles)
                {
                    await _roleManager.CreateAsync(new IdentityRole
                    {
                        Name = item,
                    });
                    
                }
            }
        }
    }
}
