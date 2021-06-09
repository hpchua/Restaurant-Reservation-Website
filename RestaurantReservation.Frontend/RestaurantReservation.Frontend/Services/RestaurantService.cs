using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<RestaurantService> logger;

        public RestaurantService(IHttpClientFactory httpClientFactory,
                                   IConfiguration configuration,
                                   ILogger<RestaurantService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:Restaurant"];
            this.logger = logger;
        }

        #region GetAll - GET
        public async Task<IEnumerable<RestaurantCategoryVM>> GetAll()
        {
            var response = await httpClient.GetAsync(route);
            var restaurants = Enumerable.Empty<RestaurantCategoryVM>();
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurants = await JsonSerializer.DeserializeAsync<IEnumerable<RestaurantCategoryVM>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {restaurants}");
            return restaurants;
        }
        #endregion

        #region GetByID - GET
        public async Task<RestaurantCategorySchedulePromtionVM> GetByID(string token, long ID, string ScheduleStatus, string PromotionStatus)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/{ID}/{ScheduleStatus}/{PromotionStatus}");
            RestaurantCategorySchedulePromtionVM restaurantCategoryScheduleVM = null;
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantCategoryScheduleVM = await JsonSerializer.DeserializeAsync<RestaurantCategorySchedulePromtionVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {restaurantCategoryScheduleVM}");
            return restaurantCategoryScheduleVM;
        }
        #endregion

        #region GetEditRestaurantByID - GET
        public async Task<RestaurantCategoryVM> GetEditRestaurantByID(string token, long ID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/EditInfo/{ID}");
            RestaurantCategoryVM restaurantCategoryVM = null;
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantCategoryVM = await JsonSerializer.DeserializeAsync<RestaurantCategoryVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {restaurantCategoryVM}");
            return restaurantCategoryVM;
        }
        #endregion

        #region Add - POST
        public async Task<Boolean> Add(string token, RestaurantCategoryVM restaurantCategoryVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantCategoryVM>(restaurantCategoryVM), Encoding.UTF8, SD.CONTENT_JSON);
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
        public async Task<int> Update(string token, RestaurantCategoryVM restaurantCategoryVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantCategoryVM>(restaurantCategoryVM), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PutAsync($"{route}/{restaurantCategoryVM.Restaurant.RestaurantID}", data);
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
