using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<CategoryService> logger;

        public CategoryService(IHttpClientFactory httpClientFactory,
                               IConfiguration configuration,
                               ILogger<CategoryService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:Category"];
            this.logger = logger;
        }

        #region GetAll - GET
        public async Task<IEnumerable<Category>> GetAll(string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token); 
            
            var response = await httpClient.GetAsync(route);
            response.EnsureSuccessStatusCode();
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var json = await response.Content.ReadAsStreamAsync();
            IEnumerable<Category> categories = await JsonSerializer.DeserializeAsync<IEnumerable<Category>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {categories}");
            return categories;
        }
        #endregion

        #region GetByID - GET
        public async Task<Category> GetByID(string token, long ID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/{ID}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var json = await response.Content.ReadAsStreamAsync();
            var category = await JsonSerializer.DeserializeAsync<Category>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {category}");
            return category;
        }
        #endregion

        #region Add - POST
        public async Task<Boolean> Add(string token, Category category)
        {
            var data = new StringContent(JsonSerializer.Serialize<Category>(category), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync(route, data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var success = false;

            if (response.IsSuccessStatusCode)
            {
                success = await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {success}");
            return success;
        }
        #endregion

        #region Update - PUT
        public async Task<int> Update(string token, Category category)
        {
            var data = new StringContent(JsonSerializer.Serialize<Category>(category), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PutAsync($"{route}/{category.CategoryID}", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var statusCode = Convert.ToInt32(response.StatusCode);

            logger.LogTrace($"RestaurantReservationSystem.API Responses : {statusCode}");
            return statusCode;
        }
        #endregion

        #region Delete - DELETE
        public async Task<Boolean> Delete(string token, long id, string userID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.DeleteAsync($"{route}/{userID}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return false;

            return true;
        }
        #endregion
    }
}
