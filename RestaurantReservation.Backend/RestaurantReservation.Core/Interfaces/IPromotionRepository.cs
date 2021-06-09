using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface IPromotionRepository
    {
        public Task<Promotion> GetPromotionByID(long promotionID);
        public Task<Promotion> GetByIDForEmail(long promotionID);
        public Task<List<Promotion>> GetAllPendingPromotionEmail();
        public Task<List<Promotion>> GetRestaurantPromotion(long restaurantID);
        public Task<Promotion> GetPromotionByName(string PromotionName, long restaurantID);
        public Task<bool> CheckExistingName(Promotion promotion, long promotionID);
        public Task<Promotion> Add(Promotion promotion);
        public Task Update(Promotion promotion);
        public Task Delete(Promotion promotion);
    }
}
