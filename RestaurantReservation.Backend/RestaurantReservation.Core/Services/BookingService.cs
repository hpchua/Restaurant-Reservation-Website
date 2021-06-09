using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            this.bookingRepository = bookingRepository;
        }

        public async Task<int> AdminGetPendingBookingCount()
        {
            return await bookingRepository.AdminGetPendingBookingCount();
        }

        public async Task<IEnumerable<Booking>> GetAll()
        {
            return await bookingRepository.GetAll();
        }

        public async Task<Booking> GetByNumber(string bookingNo)
        {
            return await bookingRepository.GetByNumber(bookingNo);
        }

        public async Task<Booking> GetByID(long bookingID)
        {
            return await bookingRepository.GetByID(bookingID);
        }

        public async Task<BookingDetail> GetDetailsByID(long bookingID)
        {
            return await bookingRepository.GetDetailsByID(bookingID);
        }

        public async Task<IEnumerable<Booking>> GetByUserID(string userID, string status)
        {
            return await bookingRepository.GetByUserID(userID, status);
        }

        public async Task<Booking> Add(Booking booking)
        {
            return await bookingRepository.Add(booking);
        }

        public async Task CreateBookingDetail(BookingDetail bookingDetail)
        {
            await bookingRepository.CreateBookingDetail(bookingDetail);
        }

        public async Task Update(Booking booking)
        {
            await bookingRepository.Update(booking);
        }

        public async Task Delete(Booking booking)
        {
            await bookingRepository.Delete(booking);
        }
    }
}
