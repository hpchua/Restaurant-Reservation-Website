using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.IntegrationTests.Configurations.Api.Services
{
    public class CategoryServices : ApiService
    {
        public static class CategoryInputs
        {
            public static readonly int fakeID = 999;
            public static readonly string newCategoryName = "Korean";
            public static readonly string editCategoryName = "Koreann";

            public static readonly long chickenID = 4;
            public static readonly string chickenCategory = "Chicken";
        }

        protected async Task<Int32> GetCount()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Categories.GetCount);

            return await JsonSerializer.DeserializeAsync<Int32>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<List<Category>> GetAll()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Categories.GetAll);

            return await JsonSerializer.DeserializeAsync<List<Category>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<Category> GetByID(long ID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Categories.GetById.Replace("{id}", ID.ToString()));
            Category categoryToFound = null;

            categoryToFound = await JsonSerializer.DeserializeAsync<Category>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (categoryToFound.CategoryID < 1)
                return null;

            return categoryToFound;
        }

        protected async Task<Boolean> Create(Category category)
        {
            var data = new StringContent(JsonSerializer.Serialize<Category>(category), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Categories.Post, data);

            return await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<HttpStatusCode> Update(long id, Category category)
        {
            var data = new StringContent(JsonSerializer.Serialize<Category>(category), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PutAsync(ApiRoutes.Categories.Put.Replace("{id}", id.ToString()), data);

            return response.StatusCode;
        }

        protected async Task<Boolean> Delete(long id)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Categories.Delete.Replace("{id}", id.ToString()));

            if (response.StatusCode != HttpStatusCode.NoContent)
                return false;

            return true;
        }
    }
}
