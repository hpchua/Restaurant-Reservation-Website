using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IRestaurantScheduleRepository
    {
        public Task<Int32> GetCount(long restaurantID);
        public Task<RestaurantSchedule> GetScheduleByID(long scheduleID);
        public Task<List<RestaurantSchedule>> GetAvailableRestaurantSchedules(long restaurantID);
        public Task<List<RestaurantSchedule>> GetRestaurantSchedules(long restaurantID);
        public Task<List<RestaurantSchedule>> GetMemberRestaurantSchedules(long restaurantID);
        public Task<bool> CheckExistingStartTime(long scheduleID, DateTime scheduleDate, DateTime startTime, long restaurantID, string action);
        public Task Add(RestaurantSchedule restaurantSchedule);
        public Task Update(RestaurantSchedule restaurantSchedule);
        public Task Delete(RestaurantSchedule restaurantSchedule);
    }
}
