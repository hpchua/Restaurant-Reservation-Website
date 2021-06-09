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
    public class RestaurantScheduleRepository : IRestaurantScheduleRepository
    {
        private readonly DatabaseContext db;

        public RestaurantScheduleRepository(DatabaseContext context)
        {
            db = context;
        }

        public async Task<Int32> GetCount(long restaurantID)
        {
            return await db.RestaurantSchedules.CountAsync(rs => rs.RestaurantID == restaurantID && rs.Status == (int)SD.ScheduleStatus.Available);
        }

        public async Task<RestaurantSchedule> GetScheduleByID(long scheduleID)
        {
            return await db.RestaurantSchedules.FindAsync(scheduleID);
        }

        public async Task<List<RestaurantSchedule>> GetAvailableRestaurantSchedules(long restaurantID)
        {
            return await db.RestaurantSchedules.Where(rs => rs.RestaurantID == restaurantID && rs.Status == (int)SD.ScheduleStatus.Available)
                                               .ToListAsync();
        }

        public async Task<List<RestaurantSchedule>> GetRestaurantSchedules(long restaurantID)
        {
            return await db.RestaurantSchedules.Where(rs => rs.RestaurantID == restaurantID)
                                               .ToListAsync();
        }

        public async Task<List<RestaurantSchedule>> GetMemberRestaurantSchedules(long restaurantID)
        {
            return await db.RestaurantSchedules.Where(rs => rs.RestaurantID == restaurantID && (rs.Status == (int)SD.ScheduleStatus.Available || rs.Status == (int)SD.ScheduleStatus.Full))
                                                .ToListAsync();
        }

        public async Task<bool> CheckExistingStartTime(long scheduleID, DateTime scheduleDate, DateTime startTime, long restaurantID, string action)
        {
            var schedules = await db.RestaurantSchedules.Where(rs => rs.RestaurantID == restaurantID).ToListAsync();

            foreach (var schedule in schedules)
            {
                if (action.Equals("update"))
                {
                    if (scheduleID != schedule.ScheduleID &&
                    schedule.ScheduleDate.ToString("dd/MM/yyyy") == scheduleDate.ToString("dd/MM/yyyy") &&
                    schedule.StartTime.ToString("hh:mm tt") == startTime.ToString("hh:mm tt"))
                        return true;
                }
                else
                {
                    if (schedule.ScheduleDate.ToString("dd/MM/yyyy") == scheduleDate.ToString("dd/MM/yyyy") &&
                        schedule.StartTime.ToString("hh:mm tt") == startTime.ToString("hh:mm tt"))
                        return true;
                }
            }
            return false;
        }

        public async Task Add(RestaurantSchedule restaurantSchedule)
        {
            await db.RestaurantSchedules.AddAsync(restaurantSchedule);
            await db.SaveChangesAsync();
        }

        public async Task Update(RestaurantSchedule restaurantSchedule)
        {
            var oldValue = db.RestaurantSchedules.First(r => r.ScheduleID == restaurantSchedule.ScheduleID);
            db.Entry(oldValue).CurrentValues.SetValues(restaurantSchedule);
            await db.SaveChangesAsync();
        }

        public async Task Delete(RestaurantSchedule restaurantSchedule)
        {
            db.RestaurantSchedules.Remove(restaurantSchedule);
            await db.SaveChangesAsync();
        }
    }
}
