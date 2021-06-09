using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Core.ViewModels.Members;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface IBookingService
    {
        public Task<Int32> AdminGetPendingCount();
        public Task<IEnumerable<Booking>> GetAll(string token, string status);
        public Task<BookingDetailVM> GetBookingDetailsByNumber(string token, string bookingNo);
        public Task<BookingHistoryVM> GetAllBookingsByUserID(string token, string userID, string status);
        public Task<Booking> Add(string token, MakeBookingVM makeBookingVM);
        public Task<Boolean> Update(string token, Booking booking);
    }
}
