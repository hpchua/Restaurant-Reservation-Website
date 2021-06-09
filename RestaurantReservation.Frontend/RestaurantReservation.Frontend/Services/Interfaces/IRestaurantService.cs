using RestaurantReservation.Core.ViewModels.Admins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface IRestaurantService
    {
        public Task<IEnumerable<RestaurantCategoryVM>> GetAll();
        public Task<RestaurantCategorySchedulePromtionVM> GetByID(string token, long ID, string ScheduleStatus, string PromotionStatus);
        public Task<RestaurantCategoryVM> GetEditRestaurantByID(string token, long ID);
        public Task<Boolean> Add(string token, RestaurantCategoryVM restaurantCategoryVM);
        public Task<int> Update(string token, RestaurantCategoryVM restaurantCategoryVM);
        public Task<Boolean> Delete(string token, long id, string userID);
    }
}
