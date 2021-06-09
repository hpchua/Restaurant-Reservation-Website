using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class RestaurantCategoryService : IRestaurantCategoryService
    {
        private readonly IRestaurantCategoryRepository restaurantCategoryRepository;

        public RestaurantCategoryService(IRestaurantCategoryRepository restaurantCategoryRepository)
        {
            this.restaurantCategoryRepository = restaurantCategoryRepository;
        }

        public async Task<List<RestaurantCategory>> GetAll()
        {
            return await restaurantCategoryRepository.GetAll();
        }

        public async Task<List<RestaurantCategory>> GetRestaurantCategory(long restaurantID)
        {
            return await restaurantCategoryRepository.GetRestaurantCategory(restaurantID);
        }

        public async Task<List<RestaurantCategory>> GetCategories(long restaurantID)
        {
            return await restaurantCategoryRepository.GetCategories(restaurantID);
        }

        public async Task Add(RestaurantCategory restaurantCategory)
        {
            await restaurantCategoryRepository.Add(restaurantCategory);
        }

        public async Task Delete(List<RestaurantCategory> restaurantCategories)
        {
            await restaurantCategoryRepository.Delete(restaurantCategories);
        }
    }
}
