using RestaurantReservation.Core.Entities;
using System.Collections.Generic;

namespace RestaurantReservation.Core.ViewModels.Members
{
    public static class BookingViewModel
    {

    }

    //  Member Booking History
    public class BookingHistoryVM
    {
        public IEnumerable<Booking> Bookings { get; set; }
        public List<BookingDetail> BookingDetails { get; set; }
    }

    //  Member Make Booking
    public class MakeBookingVM
    {
        public Booking Booking { get; set; }
        public long ScheduleID { get; set; }
        public int Pax { get; set; }
    }
}
