using RestaurantReservation.Core.Entities;

namespace RestaurantReservation.Core.DTO
{
    /// <summary>
    /// For the API Refresh JWT Purpose Everytime the user access the API Services
    /// </summary>
    public class UserToken
    {
        public UserToken(ApplicationUser applicationUser, string token)
        {
            ApplicationUser = applicationUser;
            this.token = token;
        }

        public ApplicationUser ApplicationUser { get; set; }

        private string token;

        public string Token
        {
            get { return token; }
            set { token = value; }
        }
    }

    public struct RefreshRequest
    {
        public RefreshRequest(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }

        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
