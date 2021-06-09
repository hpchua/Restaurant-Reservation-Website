using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository promotionRepository;

        public PromotionService(IPromotionRepository promotionRepository)
        {
            this.promotionRepository = promotionRepository;
        }

        public async Task<Promotion> GetPromotionByID(long promotionID)
        {
            return await promotionRepository.GetPromotionByID(promotionID);
        }

        public async Task<Promotion> GetByIDForEmail(long promotionID)
        {
            return await promotionRepository.GetByIDForEmail(promotionID);
        }

        public async Task<List<Promotion>> GetAllPendingPromotionEmail()
        {
            return await promotionRepository.GetAllPendingPromotionEmail();
        }

        public async Task<List<Promotion>> GetRestaurantPromotion(long restaurantID)
        {
            return await promotionRepository.GetRestaurantPromotion(restaurantID);
        }

        public async Task<Promotion> GetPromotionByName(string PromotionName, long restaurantID)
        {
            return await promotionRepository.GetPromotionByName(PromotionName, restaurantID);
        }

        public async Task<bool> CheckExistingName(Promotion promotion, long promotionID)
        {
            return await promotionRepository.CheckExistingName(promotion, promotionID);
        }

        public async Task<Promotion> Add(Promotion promotion)
        {
            return await promotionRepository.Add(promotion);
        }

        public async Task Update(Promotion promotion)
        {
            await promotionRepository.Update(promotion);
        }

        public async Task Delete(Promotion promotion)
        {
            await promotionRepository.Delete(promotion);
        }
    }
}
