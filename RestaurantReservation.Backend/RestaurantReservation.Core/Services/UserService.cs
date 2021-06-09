using Microsoft.AspNetCore.Identity;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public UserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<bool> CheckPassword(ApplicationUser user, string password)
        {
            return await userRepository.CheckPassword(user, password);
        }

        public async Task<List<ApplicationUser>> GetAll()
        {
            return await userRepository.GetAll();
        }

        public async Task<List<IdentityUserRole<string>>> GetAllUserRole()
        {
            return await userRepository.GetAllUserRole();
        }

        public async Task<List<IdentityRole>> GetAllRole()
        {
            return await userRepository.GetAllRole();
        }

        public async Task<ApplicationUser> GetByID(string userID)
        {
            return await userRepository.GetByID(userID);
        }

        public async Task<ApplicationUser> GetUserByID(string id)
        {
            return await userRepository.GetUserByID(id);
        }

        public async Task<ApplicationUser> GetUserByEmail(string email)
        {
            return await userRepository.GetUserByEmail(email);
        }

        public async Task<ApplicationUser> GetUserByUsername(string username)
        {
            return await userRepository.GetUserByUsername(username);
        }

        public async Task<IList<string>> GetUserRole(ApplicationUser user)
        {
            return await userRepository.GetUserRole(user);
        }

        public async Task<string> GetRoleNameByUserID(string Id)
        {
            return await userRepository.GetRoleNameByUserID(Id);
        }

        public async Task<IdentityResult> Add(ApplicationUser user, string password)
        {
            return await userRepository.Add(user, password);
        }

        public async Task AddUserRole(ApplicationUser user, string role)
        {
            await userRepository.AddUserRole(user, role);
        }

        public async Task<string> GenerateResetPasswordToken(ApplicationUser user)
        {
            return await userRepository.GenerateResetPasswordToken(user);
        }

        public async Task<IdentityResult> ChangePassword(ApplicationUser user, string currentPassword, string newPassword)
        {
            return await userRepository.ChangePassword(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ResetPassword(ApplicationUser user, string token, string newPassword)
        {
            return await userRepository.ResetPassword(user, token, newPassword);
        }

        public async Task Update(ApplicationUser user)
        {
            await userRepository.Update(user);
        }

        public async Task<IdentityResult> Delete(ApplicationUser user)
        {
            return await userRepository.Delete(user);
        }
    }
}
