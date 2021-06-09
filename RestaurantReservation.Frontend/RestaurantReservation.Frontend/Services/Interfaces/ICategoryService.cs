using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services.Interfaces
{
    public interface ICategoryService
    {
        public Task<IEnumerable<Category>> GetAll(string token);
        public Task<Category> GetByID(string token, long id);
        public Task<Boolean> Add(string token, Category category);
        //public Task<Boolean> Update(string token, Category category);
        public Task<int> Update(string token, Category category);
        public Task<Boolean> Delete(string token, long id, string userID);
    }
}
