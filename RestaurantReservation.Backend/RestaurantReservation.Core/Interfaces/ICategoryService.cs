using RestaurantReservation.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Interfaces
{
    public interface ICategoryService
    {
        public Task<Int32> GetCount();
        public Task<List<Category>> GetAll();
        public Task<Category> GetCategoryByID(long categoryID);
        public Task<Category> GetCategoryByName(string categoryName);
        public Task Add(Category category);
        public Task Update(Category category);
        public Task Delete(Category category);
    }
}
