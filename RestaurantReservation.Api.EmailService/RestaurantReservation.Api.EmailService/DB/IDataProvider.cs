using RestaurantReservation.Api.EmailService.Models;
using System.Collections.Generic;

namespace RestaurantReservation.Api.EmailService.DB
{
    public interface IDataProvider
    {
        public List<string> GetEmails();
        public Promotion GetPromotionByID(long ID);
        public Restaurant GetRestaurantByID(long ID);
        public IEnumerable<Promotion> GetPendingPromotionEmails();
        public void UpdatePromotionEmailStatus(long promotionID);
    }
}
