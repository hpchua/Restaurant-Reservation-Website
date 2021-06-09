using Microsoft.AspNetCore.Identity;
using RestaurantReservation.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IUserService
    {
        public Task<bool> CheckPassword(ApplicationUser user, string password);
        public Task<List<ApplicationUser>> GetAll();
        public Task<List<IdentityUserRole<string>>> GetAllUserRole();
        public Task<List<IdentityRole>> GetAllRole();
        public Task<ApplicationUser> GetByID(string userID);
        public Task<ApplicationUser> GetUserByID(string id);
        public Task<ApplicationUser> GetUserByEmail(string email);
        public Task<ApplicationUser> GetUserByUsername(string username);
        public Task<IList<string>> GetUserRole(ApplicationUser user);
        public Task<string> GetRoleNameByUserID(string Id);
        public Task<IdentityResult> Add(ApplicationUser user, string password);
        public Task AddUserRole(ApplicationUser user, string role);
        public Task<string> GenerateResetPasswordToken(ApplicationUser user);
        public Task<IdentityResult> ChangePassword(ApplicationUser user, string currentPassword, string newPassword);
        public Task<IdentityResult> ResetPassword(ApplicationUser user, string token, string newPassword);
        public Task Update(ApplicationUser user);
        public Task<IdentityResult> Delete(ApplicationUser user);
    }
}
