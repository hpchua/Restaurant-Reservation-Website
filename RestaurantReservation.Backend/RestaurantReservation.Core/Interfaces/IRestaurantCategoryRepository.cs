using RestaurantReservation.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IRestaurantCategoryRepository
    {
        public Task<List<RestaurantCategory>> GetAll();
        public Task<List<RestaurantCategory>> GetRestaurantCategory(long restaurantID);
        public Task<List<RestaurantCategory>> GetCategories(long restaurantID);
        public Task Add(RestaurantCategory restaurantCategory);
        public Task Delete(List<RestaurantCategory> restaurantCategories);
    }
}
