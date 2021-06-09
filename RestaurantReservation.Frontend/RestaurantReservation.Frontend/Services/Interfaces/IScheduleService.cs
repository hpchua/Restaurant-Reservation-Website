using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.ViewModels.Admins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface IScheduleService
    {
        public Task<IEnumerable<RestaurantSchedule>> GetAll(string token, long ID);
        public Task<RestaurantScheduleVM> GetByID(string token, long ID);
        public Task<Boolean> Add(string token, RestaurantSchedule restaurantSchedule);
        public Task<int> Update(string token, RestaurantSchedule restaurantScheduleVM);
        public Task<Boolean> Delete(string token, long id, int status, string userID);
    }
}
