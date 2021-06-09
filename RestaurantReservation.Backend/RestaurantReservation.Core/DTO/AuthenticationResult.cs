using RestaurantReservation.Core.Entities;
using System.Collections.Generic;

namespace RestaurantReservation.Core.DTO
{
    ///// <summary>
    ///// For Authenticating User Purposes
    ///// </summary>
    public class AuthenticationResult
    {
        public ApplicationUser ApplicationUser { get; set; }
        public int StatusCode { get; set; }
        public List<string> Message { get; set; }

        private string token;
        private string refreshToken;

        public string Token
        {
            get { return token; }
            set { token = value; }
        }

        public string RefreshToken
        {
            get { return refreshToken; }
            set { refreshToken = value; }
        }
    }
}
