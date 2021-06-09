using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Booking")]
    public class Booking
    {
        [Key]
        [Display(Name = "Booking ID")]
        public long BookingID { get; set; }

        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public ApplicationUser ApplicationUser { get; set; }

        [Display(Name = "Booking Date")]
        public DateTime BookingDate { get; set; }

        [Display(Name = "Check In")]
        public DateTime? CheckIn { get; set; }

        [Display(Name = "Check Out")]
        public DateTime? CheckOut { get; set; }

        [Display(Name = "Booking No.")]
        [StringLength(50)]
        public string BookingNo { get; set; }

        [StringLength(20)]
        public string BookingStatus { get; set; }

        [StringLength(50)]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string PhoneNo { get; set; }

        [StringLength(20)]
        public string EditedBy { get; set; }

        public DateTime EditedDate { get; set; }
    }
}
