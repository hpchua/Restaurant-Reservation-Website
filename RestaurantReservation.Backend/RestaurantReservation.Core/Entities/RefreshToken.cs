using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Refresh_Token")]
    public class RefreshToken
    {
        [Key]
        public long TokenID { get; set; }

        [Required]
        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser ApplicationUser { get; set; }

        [StringLength(200)]
        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
