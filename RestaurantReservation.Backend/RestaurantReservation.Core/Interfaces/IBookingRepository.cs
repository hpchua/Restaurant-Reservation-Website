using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IBookingRepository
    {
        public Task<Int32> AdminGetPendingBookingCount();
        public Task<IEnumerable<Booking>> GetAll();
        public Task<Booking> GetByNumber(string bookingNo);
        public Task<Booking> GetByID(long bookingID);
        public Task<IEnumerable<Booking>> GetByUserID(string userID, string status);
        public Task<BookingDetail> GetDetailsByID(long bookingID);
        public Task<Booking> Add(Booking booking);
        public Task CreateBookingDetail(BookingDetail bookingDetail);
        public Task Update(Booking booking);
        public Task Delete(Booking booking);
    }
}
