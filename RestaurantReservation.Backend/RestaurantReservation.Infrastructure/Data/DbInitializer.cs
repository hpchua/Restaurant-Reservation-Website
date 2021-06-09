using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using System;
using System.Linq;

namespace RestaurantReservation.Infrastructure.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ILogger<DbInitializer> logger;
        private readonly DatabaseContext db;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public DbInitializer(ILogger<DbInitializer> logger,
                             DatabaseContext context,
                             UserManager<ApplicationUser> userManager,
                             RoleManager<IdentityRole> roleManager)
        {
            this.logger = logger;
            db = context;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        public void Initialize()
        {
            try
            {
                if (db.Database.GetPendingMigrations().Count() > 0)
                {
                    db.Database.Migrate();
                }

                if (!db.Roles.Any(r => r.Name == SD.ROLE_ADMIN))
                {
                    AddRolesAndCreateAdminUser();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error executing database initializer.");
            }
        }

        private void AddRolesAndCreateAdminUser()
        {
            roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN)).GetAwaiter().GetResult();
            roleManager.CreateAsync(new IdentityRole(SD.ROLE_MEMBER)).GetAwaiter().GetResult();

            ApplicationUser adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@gmail.com",
                Name = "Admin User ",
                PhoneNumber = "012-3456789",
                JoinedDate = DateTime.Now
            };

            userManager.CreateAsync(adminUser, "testing123").GetAwaiter().GetResult();
            adminUser = userManager.FindByEmailAsync(adminUser.Email).GetAwaiter().GetResult();

            userManager.AddToRoleAsync(adminUser, SD.ROLE_ADMIN).GetAwaiter().GetResult();
        }
    }
}
