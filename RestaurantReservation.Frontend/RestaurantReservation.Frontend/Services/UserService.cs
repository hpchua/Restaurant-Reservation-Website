using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
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
    public class UserService : IUserService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<UserService> logger;

        public UserService(IHttpClientFactory httpClientFactory,
                           IConfiguration configuration,
                           ILogger<UserService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:User"];
            this.logger = logger;
        }

        #region GetAll - GET
        public async Task<IEnumerable<ApplicationUser>> GetAll(string role, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/{role}");
            response.EnsureSuccessStatusCode();
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var json = await response.Content.ReadAsStreamAsync();
            IEnumerable<ApplicationUser> users = await JsonSerializer.DeserializeAsync<IEnumerable<ApplicationUser>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {users}");
            return users;
        }
        #endregion

        #region GetByID - GET
        public async Task<ApplicationUser> GetByID(string userID, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/info/{userID}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var user = await JsonSerializer.DeserializeAsync<ApplicationUser>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {user}");
                return user;
            }
            else
            {
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {null}");
                return null;
            }
        }
        #endregion

        #region CheckExistingEmail - GET
        public async Task<ApplicationUser> CheckExistingEmail(string email, string token)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await httpClient.GetAsync($"{route}/check/{email}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var user = await JsonSerializer.DeserializeAsync<ApplicationUser>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {user}");
                return user;
            }
            else
            {
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {null}");
                return null;
            }
        }
        #endregion

        #region ChangePassword - POST
        public async Task<Boolean> ChangePassword(string token, ChangePasswordVM changePasswordVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<ChangePasswordVM>(changePasswordVM), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync($"{route}/ChangePassword", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var success = true;

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                success = false;
            else
            {
                var result = await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                success = result;
            }

            logger.LogTrace($"RestaurantReservationSystem.API Responses : {success}");
            return success;
        }
        #endregion

        #region Update - Put
        public async Task<Boolean> Update(ProfileVM input, string token)
        {
            var data = new StringContent(JsonSerializer.Serialize<ProfileVM>(input), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PutAsync($"{route}", data);
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
    }
}
