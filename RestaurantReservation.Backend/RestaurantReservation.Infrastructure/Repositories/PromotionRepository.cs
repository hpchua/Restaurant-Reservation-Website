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
    public class PromotionRepository : IPromotionRepository
    {
        private readonly DatabaseContext db;

        public PromotionRepository(DatabaseContext context)
        {
            db = context;
        }

        public async Task<Promotion> GetPromotionByID(long promotionID)
        {
            return await db.Promotions.FindAsync(promotionID);
        }

        public async Task<Promotion> GetByIDForEmail(long promotionID)
        {
            return await db.Promotions.Include(p => p.Restaurant)
                                      .FirstOrDefaultAsync(p => p.PromotionID == promotionID);
        }

        public async Task<List<Promotion>> GetAllPendingPromotionEmail()
        {
            return await db.Promotions.Where(p => p.isEmailCreatedSent == false)
                                      .Include(p => p.Restaurant)
                                      .ToListAsync();
        }

        public async Task<List<Promotion>> GetRestaurantPromotion(long restaurantID)
        {
            return await db.Promotions.Where(p => p.RestaurantID == restaurantID)
                                      .Include(p => p.Restaurant)
                                      .ToListAsync();
        }

        public async Task<Promotion> GetPromotionByName(string PromotionName, long restaurantID)
        {
            return await db.Promotions.FirstOrDefaultAsync(c => c.Name.ToLower().Equals(PromotionName.Trim().ToLower()) &&
                                                                c.RestaurantID == restaurantID);
        }

        public async Task<bool> CheckExistingName(Promotion promotion, long promotionID)
        {
            var promotions = await db.Promotions.Where(rs => rs.RestaurantID == promotion.RestaurantID).ToListAsync();

            foreach (var existingPromotion in promotions)
            {
                if (promotionID != existingPromotion.PromotionID &&
                    existingPromotion.Name.ToLower().Trim().Equals(promotion.Name.ToLower().Trim()))
                    return true;
            }
            return false;
        }

        public async Task<Promotion> Add(Promotion promotion)
        {
            var newPromotion = await db.Promotions.AddAsync(promotion);
            await db.SaveChangesAsync();
            return newPromotion.Entity;
        }

        public async Task Update(Promotion promotion)
        {
            var oldValue = db.Promotions.First(c => c.PromotionID == promotion.PromotionID);
            db.Entry(oldValue).CurrentValues.SetValues(promotion);
            await db.SaveChangesAsync();
        }

        public async Task Delete(Promotion promotion)
        {
            db.Promotions.Remove(promotion);
            await db.SaveChangesAsync();
        }


    }
}
