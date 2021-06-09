using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class RestaurantScheduleService : IRestaurantScheduleService
    {
        private readonly IRestaurantScheduleRepository restaurantScheduleRepository;

        public RestaurantScheduleService(IRestaurantScheduleRepository restaurantScheduleRepository)
        {
            this.restaurantScheduleRepository = restaurantScheduleRepository;
        }

        public async Task<Int32> GetCount(long restaurantID)
        {
            return await restaurantScheduleRepository.GetCount(restaurantID);
        }

        public async Task<RestaurantSchedule> GetScheduleByID(long scheduleID)
        {
            return await restaurantScheduleRepository.GetScheduleByID(scheduleID);
        }

        public async Task<List<RestaurantSchedule>> GetAvailableRestaurantSchedules(long restaurantID)
        {
            return await restaurantScheduleRepository.GetAvailableRestaurantSchedules(restaurantID);
        }

        public async Task<List<RestaurantSchedule>> GetRestaurantSchedules(long restaurantID)
        {
            return await restaurantScheduleRepository.GetRestaurantSchedules(restaurantID);
        }

        public async Task<List<RestaurantSchedule>> GetMemberRestaurantSchedules(long restaurantID)
        {
            return await restaurantScheduleRepository.GetMemberRestaurantSchedules(restaurantID);
        }

        public async Task<bool> CheckExistingStartTime(long scheduleID, DateTime scheduleDate, DateTime startTime, long restaurantID, string action)
        {
            return await restaurantScheduleRepository.CheckExistingStartTime(scheduleID, scheduleDate, startTime, restaurantID, action);
        }

        public async Task Add(RestaurantSchedule restaurantSchedule)
        {
            await restaurantScheduleRepository.Add(restaurantSchedule);
        }

        public async Task Update(RestaurantSchedule restaurantSchedule)
        {
            await restaurantScheduleRepository.Update(restaurantSchedule);
        }

        public async Task Delete(RestaurantSchedule restaurantSchedule)
        {
            await restaurantScheduleRepository.Delete(restaurantSchedule);
        }
    }
}
