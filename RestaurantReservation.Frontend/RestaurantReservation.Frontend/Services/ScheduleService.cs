using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
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
    public class ScheduleService : IScheduleService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<ScheduleService> logger;

        public ScheduleService(IHttpClientFactory httpClientFactory,
                               IConfiguration configuration,
                               ILogger<ScheduleService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:Schedule"];
            this.logger = logger;
        }

        #region GetAll - GET
        public async Task<IEnumerable<RestaurantSchedule>> GetAll(string token, long ID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/All/{ID}");
            response.EnsureSuccessStatusCode();
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var json = await response.Content.ReadAsStreamAsync();
            IEnumerable<RestaurantSchedule> restaurantSchedules = await JsonSerializer.DeserializeAsync<IEnumerable<RestaurantSchedule>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {restaurantSchedules}");
            return restaurantSchedules;
        }
        #endregion

        #region GetByID - GET
        public async Task<RestaurantScheduleVM> GetByID(string token, long ID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/{ID}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var json = await response.Content.ReadAsStreamAsync();
            var schedule = await JsonSerializer.DeserializeAsync<RestaurantScheduleVM>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {schedule}");
            return schedule;
        }

        #endregion

        #region Add - POST
        public async Task<Boolean> Add(string token, RestaurantSchedule restaurantSchedule)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantSchedule>(restaurantSchedule), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync(route, data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var success = true;

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                success = false;
            
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {success}");
            return success;
        }
        #endregion

        #region Edit - PUT
        public async Task<int> Update(string token, RestaurantSchedule restaurantSchedule)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantSchedule>(restaurantSchedule), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PutAsync($"{route}/{restaurantSchedule.ScheduleID}", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var statusCode = Convert.ToInt32(response.StatusCode);

            logger.LogTrace($"RestaurantReservationSystem.API Responses : {statusCode}");
            return statusCode;
        }
        #endregion

        #region Delete - DELETE
        public async Task<Boolean> Delete(string token, long id, int status, string userID)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.DeleteAsync($"{route}/{userID}/{status}/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                return false;

            return true;
        }
        #endregion
    }
}
