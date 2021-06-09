using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Core.ViewModels.Accounts
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Username is required")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
