using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Core.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            this.categoryRepository = categoryRepository;
        }

        public async Task<Int32> GetCount()
        {
            return await categoryRepository.GetCount();
        }

        public async Task<List<Category>> GetAll()
        {
            return await categoryRepository.GetAll();
        }

        public async Task<Category> GetCategoryByID(long categoryID)
        {
            return await categoryRepository.GetCategoryByID(categoryID);
        }

        public async Task<Category> GetCategoryByName(string categoryName)
        {
            return await categoryRepository.GetCategoryByName(categoryName);
        }

        public async Task Add(Category category)
        {
            await categoryRepository.Add(category);
        }

        public async Task Update(Category category)
        {
            await categoryRepository.Update(category);
        }

        public async Task Delete(Category category)
        {
            await categoryRepository.Delete(category);
        }
    }
}
