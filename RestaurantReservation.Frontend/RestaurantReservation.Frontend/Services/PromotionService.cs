using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<PromotionService> logger;

        public PromotionService(IHttpClientFactory httpClientFactory,
                               IConfiguration configuration,
                               ILogger<PromotionService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:Promotion"];
            this.logger = logger;
        }

        #region CheckExistingByID - GET
        public async Task<Promotion> CheckExistingByID(string token, long ID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/Check/{ID}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");
            Promotion promotion = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                promotion = await JsonSerializer.DeserializeAsync<Promotion>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }

            logger.LogTrace($"RestaurantReservationSystem.API Responses : {promotion}");
            return promotion;
        }
        #endregion

        #region GetByID - GET
        public async Task<RestaurantPromotionVM> GetByID(string token, long ID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/{ID}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var json = await response.Content.ReadAsStreamAsync();
            var promotion = await JsonSerializer.DeserializeAsync<RestaurantPromotionVM>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {promotion}");
            return promotion;
        }
        #endregion

        #region Add - POST
        public async Task<Boolean> Add(string token, Promotion promotion)
        {
            var data = new StringContent(JsonSerializer.Serialize<Promotion>(promotion), Encoding.UTF8, SD.CONTENT_JSON);
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
        public async Task<int> Update(string token, Promotion promotion)
        {
            var data = new StringContent(JsonSerializer.Serialize<Promotion>(promotion), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PutAsync($"{route}/{promotion.PromotionID}", data);
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

        #region PushEmail - GET
        public async Task<Boolean> PushEmail(string token, long id)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/PushEmail/{id}");

            if ((int)response.StatusCode == SD.StatusCode.NOT_FOUND)
                return false;

            return true;
        }
        #endregion
    }
}
