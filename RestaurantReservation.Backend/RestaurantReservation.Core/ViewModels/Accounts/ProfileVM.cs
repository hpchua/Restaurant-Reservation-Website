using RestaurantReservation.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace RestaurantReservation.Core.ViewModels.Accounts
{
    public static class ProfileViewModel
    {
        public static ApplicationUser FetchData(ProfileVM input)
        {
            return new ApplicationUser
            {
                Id = input.UserID,
                UserName = input.Username,
                Name = input.Name,
                PhoneNumber = input.PhoneNumber,
                Email = input.Email,
                isSubscriber = input.Subscribe,
            };
        }

        public static ApplicationUser UpdateData(ApplicationUser oldValue, ApplicationUser newValue)
        {
            return new ApplicationUser
            {
                Id = oldValue.Id,
                UserName = oldValue.UserName,
                NormalizedUserName = oldValue.NormalizedUserName,
                Email = newValue.Email,
                NormalizedEmail = newValue.Email.ToUpper(),
                PasswordHash = oldValue.PasswordHash,
                PhoneNumber = newValue.PhoneNumber,
                Name = newValue.Name,
                JoinedDate = oldValue.JoinedDate,
                isSubscriber = newValue.isSubscriber,
            };
        }
    }

    public class ProfileVM
    {
        public string UserID { get; set; }
        public string Username { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public bool Subscribe { get; set; }
    }
}
