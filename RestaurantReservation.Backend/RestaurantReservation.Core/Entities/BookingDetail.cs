using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Booking_Detail")]
    public class BookingDetail
    {
        [Key]
        public long BookingDetailID { get; set; }

        [Required]
        public long BookingID { get; set; }

        [ForeignKey("BookingID")]
        public Booking Booking { get; set; }

        [Required]
        public long ScheduleID { get; set; }

        [ForeignKey("ScheduleID")]
        public RestaurantSchedule RestaurantSchedule { get; set; }

        public int Pax { get; set; }
    }
}
