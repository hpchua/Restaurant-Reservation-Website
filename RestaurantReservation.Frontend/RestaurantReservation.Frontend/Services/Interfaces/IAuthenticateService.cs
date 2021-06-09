using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.ViewModels.Accounts;
using System;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface IAuthenticateService
    {
        public Task<AuthenticationResult> Login(LoginVM loginInput);
        public Task<UserToken> RefreshToken(string accessToken, string refreshToken);
        public Task<AuthenticationResult> Register(RegisterVM registerInput);
        public Task<Boolean> ForgotPassword(string email);
        public Task<Boolean> ResetPassword(ResetPasswordVM changePasswordVM);
    }
}
