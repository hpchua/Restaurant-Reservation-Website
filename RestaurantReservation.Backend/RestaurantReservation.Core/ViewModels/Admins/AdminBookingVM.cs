using RestaurantReservation.Core.Entities;

namespace RestaurantReservation.Core.ViewModels.Admins
{
    //  Member Booking Details
    public class BookingDetailVM
    {
        public Booking Booking { get; set; }
        public BookingDetail BookingDetail { get; set; }
    }
}
