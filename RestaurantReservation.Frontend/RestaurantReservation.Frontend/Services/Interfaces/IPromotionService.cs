using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.ViewModels.Admins;
using System;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface IPromotionService
    {
        public Task<Promotion> CheckExistingByID(string token, long ID);
        public Task<RestaurantPromotionVM> GetByID(string token, long ID);
        public Task<Boolean> Add(string token, Promotion promotion);
        public Task<int> Update(string token, Promotion promotion);
        public Task<Boolean> Delete(string token, long id, string userID);
        public Task<Boolean> PushEmail(string token, long id);
    }
}
