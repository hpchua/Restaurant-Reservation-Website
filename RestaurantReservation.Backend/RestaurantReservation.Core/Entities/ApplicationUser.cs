using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }

        public DateTime JoinedDate { get; set; }

        [NotMapped]
        public string Role { get; set; }

        public bool isSubscriber { get; set; }
    }
}
