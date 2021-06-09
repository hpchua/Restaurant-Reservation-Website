using Microsoft.EntityFrameworkCore;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantReservation.Infrastructure.Repositories
{
    public class RestaurantRepository : IRestaurantRepository
    {
        private readonly DatabaseContext db;

        public RestaurantRepository(DatabaseContext context)
        {
            db = context;
        }

        public async Task<Int32> GetCount()
        {
            return await db.Restaurants.CountAsync();
        }

        public async Task<List<Restaurant>> GetAll()
        {
            return await db.Restaurants.OrderBy(r => r.Name).ToListAsync();
        }

        public async Task<Restaurant> GetRestaurantByID(long restaurantID)
        {
            return await db.Restaurants.FindAsync(restaurantID);
        }

        public async Task<Restaurant> GetRestaurantByName(string restaurantName)
        {
            return await db.Restaurants.FirstOrDefaultAsync(r => r.Name.ToLower().Equals(restaurantName.Trim().ToLower()));
        }

        public async Task Add(Restaurant restaurant)
        {
            await db.Restaurants.AddAsync(restaurant);
            await db.SaveChangesAsync();
        }

        public async Task Update(Restaurant restaurant)
        {
            var oldValue = db.Restaurants.First(r => r.RestaurantID == restaurant.RestaurantID);
            db.Entry(oldValue).CurrentValues.SetValues(restaurant);
            await db.SaveChangesAsync();
        }

        public async Task Delete(Restaurant restaurant)
        {
            db.Restaurants.Remove(restaurant);
            await db.SaveChangesAsync();
        }
    }
}
