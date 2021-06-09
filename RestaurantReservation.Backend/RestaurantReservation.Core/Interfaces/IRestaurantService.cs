using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IRestaurantService
    {
        public Task<Int32> GetCount();
        public Task<List<Restaurant>> GetAll();
        public Task<Restaurant> GetRestaurantByID(long restaurantID);
        public Task<Restaurant> GetRestaurantByName(string restaurantName);
        public Task Add(Restaurant restaurant);
        public Task Update(Restaurant restaurant);
        public Task Delete(Restaurant restaurant);
    }
}
