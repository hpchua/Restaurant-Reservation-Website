using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly IRestaurantRepository restaurantRepository;

        public RestaurantService(IRestaurantRepository restaurantRepository)
        {
            this.restaurantRepository = restaurantRepository;
        }

        public async Task<Int32> GetCount()
        {
            return await restaurantRepository.GetCount();
        }

        public async Task<List<Restaurant>> GetAll()
        {
            return await restaurantRepository.GetAll();
        }

        public async Task<Restaurant> GetRestaurantByID(long restaurantID)
        {
            return await restaurantRepository.GetRestaurantByID(restaurantID);
        }

        public async Task<Restaurant> GetRestaurantByName(string restaurantName)
        {
            return await restaurantRepository.GetRestaurantByName(restaurantName);
        }

        public async Task Add(Restaurant restaurant)
        {
            await restaurantRepository.Add(restaurant);
        }

        public async Task Update(Restaurant restaurant)
        {
            await restaurantRepository.Update(restaurant);
        }

        public async Task Delete(Restaurant restaurant)
        {
            await restaurantRepository.Delete(restaurant);
        }
    }
}
