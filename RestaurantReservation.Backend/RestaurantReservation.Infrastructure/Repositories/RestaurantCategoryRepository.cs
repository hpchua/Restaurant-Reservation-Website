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
    public class RestaurantCategoryRepository : IRestaurantCategoryRepository
    {
        private readonly DatabaseContext db;

        public RestaurantCategoryRepository(DatabaseContext context)
        {
            db = context;
        }

        public async Task<List<RestaurantCategory>> GetAll()
        {
            return await db.RestaurantCategories.AsNoTracking()
                                                .Include(rc => rc.Restaurant)
                                                .Include(rc => rc.Category)
                                                .ToListAsync();
        }

        public async Task<List<RestaurantCategory>> GetRestaurantCategory(long restaurantID)
        {
            return await db.RestaurantCategories.Where(rc => rc.RestaurantID == restaurantID)
                                                .Include(rc => rc.Restaurant)
                                                .Include(rc => rc.Category)
                                                .ToListAsync();
        }

        public async Task<List<RestaurantCategory>> GetCategories(long restaurantID)
        {
            return await db.RestaurantCategories.Include(rc => rc.Category)
                                                .Where(rc => rc.RestaurantID == restaurantID)
                                                .ToListAsync();
        }

        public async Task Add(RestaurantCategory restaurantCategory)
        {
            await db.RestaurantCategories.AddAsync(restaurantCategory);
            await db.SaveChangesAsync();
        }

        public async Task Delete(List<RestaurantCategory> restaurantCategories)
        {
            db.RestaurantCategories.RemoveRange(restaurantCategories);
            await db.SaveChangesAsync();
        }
    }
}
