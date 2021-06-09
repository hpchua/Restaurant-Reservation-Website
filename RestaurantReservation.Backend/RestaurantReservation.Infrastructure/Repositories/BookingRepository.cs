using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantReservation.Infrastructure.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DatabaseContext db;

        public BookingRepository(DatabaseContext context)
        {
            db = context;
        }

        public async Task<Int32> AdminGetPendingBookingCount()
        {
            return await db.Bookings.CountAsync(b => b.BookingStatus.Equals(SD.BookingStatus.PENDING));
        }

        public async Task<IEnumerable<Booking>> GetAll()
        {
            return await db.Bookings.OrderByDescending(b => b.BookingID).ToListAsync();
        }

        public async Task<Booking> GetByNumber(string bookingNo)
        {
            return await db.Bookings.Include(b => b.ApplicationUser).FirstOrDefaultAsync(b => b.BookingNo == bookingNo);
        }

        public async Task<Booking> GetBookingByID(long bookingID)
        {
            return await db.Bookings.Include(b => b.ApplicationUser).FirstOrDefaultAsync(b => b.BookingID == bookingID);
        }

        public async Task<Booking> GetByID(long bookingID)
        {
            return await db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.BookingID == bookingID);
        }

        public async Task<BookingDetail> GetDetailsByID(long bookingID)
        {
            return await db.BookingDetails.Include(b => b.RestaurantSchedule)
                                          .ThenInclude(b => b.Restaurant)
                                          .FirstOrDefaultAsync(b => b.BookingID == bookingID);
        }

        public async Task<IEnumerable<Booking>> GetByUserID(string userID, string status)
        {
            if (status.Equals("all"))
            {
                return await db.Bookings.Where(b => b.UserID == userID)
                                        .OrderByDescending(b => b.BookingID)
                                        .ToListAsync();
            }
            else
            {
                return await db.Bookings.Where(b => b.UserID == userID && b.BookingStatus.Equals(status))
                                        .OrderByDescending(b => b.BookingID)
                                        .ToListAsync();
            }
        }

        public async Task<Booking> Add(Booking booking)
        {
            Booking newBooking = booking;

            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    await db.Bookings.AddAsync(booking);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }
            return newBooking;
        }

        public async Task CreateBookingDetail(BookingDetail bookingDetail)
        {
            var schedule = await db.RestaurantSchedules.FindAsync(bookingDetail.ScheduleID);
            var oldScheduleValue = db.RestaurantSchedules.First(r => r.ScheduleID == bookingDetail.ScheduleID);

            using (Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // Update Schedule AvailableSeat Value
                    schedule.AvailableSeat -= bookingDetail.Pax;
                    db.Entry(oldScheduleValue).CurrentValues.SetValues(schedule);

                    await db.BookingDetails.AddAsync(bookingDetail);
                    await db.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                }
                finally
                {
                    await transaction.DisposeAsync();
                }
            }
        }

        public async Task Update(Booking booking)
        {
            var oldValue = db.Bookings.First(b => b.BookingID == booking.BookingID);
            db.Entry(oldValue).CurrentValues.SetValues(booking);
            await db.SaveChangesAsync();
        }

        public async Task Delete(Booking booking)
        {
            db.Bookings.Remove(booking);
            await db.SaveChangesAsync();
        }
    }
}
