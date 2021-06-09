using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface IUserService
    {
        public Task<IEnumerable<ApplicationUser>> GetAll(string role, string token);
        public Task<ApplicationUser> GetByID(string userID, string token);
        public Task<ApplicationUser> CheckExistingEmail(string email, string token);
        public Task<Boolean> ChangePassword(string token, ChangePasswordVM changePasswordVM);
        public Task<Boolean> Update(ProfileVM input, string token);
    }
}
