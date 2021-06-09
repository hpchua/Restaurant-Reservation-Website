using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Infrastructure.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantReservation.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DatabaseContext db;
        private readonly UserManager<ApplicationUser> userManager;

        public UserRepository(DatabaseContext context,
                              UserManager<ApplicationUser> userManager)
        {
            db = context;
            this.userManager = userManager;
        }

        public async Task<bool> CheckPassword(ApplicationUser user, string password)
        {
            return await userManager.CheckPasswordAsync(user, password);
        }

        public async Task<List<ApplicationUser>> GetAll()
        {
            return await db.ApplicationUsers.AsNoTracking().OrderByDescending(a => a.Id).ToListAsync();
        }

        public async Task<List<IdentityUserRole<string>>> GetAllUserRole()
        {
            return await db.UserRoles.AsNoTracking().ToListAsync();
        }

        public async Task<List<IdentityRole>> GetAllRole()
        {
            return await db.Roles.AsNoTracking().ToListAsync();
        }

        public async Task<ApplicationUser> GetByID(string userID)
        {
            return await db.ApplicationUsers.FindAsync(userID);
        }

        public async Task<ApplicationUser> GetUserByID(string id)
        {
            return await userManager.FindByIdAsync(id);
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await userManager.FindByEmailAsync(email);
        }

        public async Task<ApplicationUser> GetUserByUsername(string username)
        {
            return await userManager.FindByNameAsync(username);
        }

        public async Task<IList<string>> GetUserRole(ApplicationUser user)
        {
            return await userManager.GetRolesAsync(user);
        }

        public async Task<string> GetRoleNameByUserID(string Id)
        {
            var userRoleID = (await db.UserRoles.FirstOrDefaultAsync(u => u.UserId == Id)).RoleId;
            return (await db.Roles.FirstOrDefaultAsync(r => r.Id == userRoleID)).Name;
        }

        public async Task<IdentityResult> Add(ApplicationUser user, string password)
        {
            return await userManager.CreateAsync(user, password);
        }

        public async Task AddUserRole(ApplicationUser user, string role)
        {
            await userManager.AddToRoleAsync(user, role);
            await db.SaveChangesAsync();
        }

        public async Task<string> GenerateResetPasswordToken(ApplicationUser user)
        {
            return await userManager.GeneratePasswordResetTokenAsync(user);
        }

        public async Task<IdentityResult> ChangePassword(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ResetPassword(ApplicationUser user, string token, string newPassword)
        {
            return await userManager.ResetPasswordAsync(user, token, newPassword);
        }

        public async Task Update(ApplicationUser user)
        {
            ApplicationUser updatedUser = null;
            var oldValue = db.ApplicationUsers.First(a => a.Id == user.Id);

            updatedUser = ProfileViewModel.UpdateData(oldValue, user);

            db.Entry(oldValue).CurrentValues.SetValues(updatedUser);
            await db.SaveChangesAsync();
        }

        public async Task<IdentityResult> Delete(ApplicationUser user)
        {
            return await userManager.DeleteAsync(user);
        }
    }
}
