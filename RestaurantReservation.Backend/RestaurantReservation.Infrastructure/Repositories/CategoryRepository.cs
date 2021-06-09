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
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DatabaseContext db;

        public CategoryRepository(DatabaseContext context)
        {
            db = context;
        }

        public async Task<Int32> GetCount()
        {
            return await db.Categories.CountAsync();
        }

        public async Task<List<Category>> GetAll()
        {
            return await db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        }

        public async Task<Category> GetCategoryByID(long categoryID)
        {
            return await db.Categories.FindAsync(categoryID);
        }

        public async Task<Category> GetCategoryByName(string categoryName)
        {
            return await db.Categories.FirstOrDefaultAsync(c => c.Name.ToLower().Equals(categoryName.Trim().ToLower()));
        }

        public async Task Add(Category category)
        {
            await db.Categories.AddAsync(category);
            await db.SaveChangesAsync();
        }

        public async Task Update(Category category)
        {
            var oldValue = db.Categories.First(c => c.CategoryID == category.CategoryID);
            db.Entry(oldValue).CurrentValues.SetValues(category);
            await db.SaveChangesAsync();
        }

        public async Task Delete(Category category)
        {
            db.Categories.Remove(category);
            await db.SaveChangesAsync();
        }
    }
}
