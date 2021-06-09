using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.IntegrationTests.Configurations.Api.Services
{
    public class UserServices : ApiService
    {
        public static class UserInputs
        {
            public static readonly string existingAdminUsername = "admin";
            public static readonly string existingAdminEmail = "admin@gmail.com";

            public static readonly string fakeUserID = "TESTING123";
            public static readonly string fakeEmail = "adminTesting123@gmail.com";
        }

        public async Task<IEnumerable<ApplicationUser>> GetAll()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Users.GetAll.Replace("{role}", "all"));
            var users = Enumerable.Empty<ApplicationUser>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                users = await JsonSerializer.DeserializeAsync<IEnumerable<ApplicationUser>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return users;
        }

        protected async Task<HttpResponseMessage> GetByID(string userID)
        {
            return await httpClient.GetAsync(ApiRoutes.Users.GetById.Replace("{userID}", userID));
        }

        protected async Task<ApplicationUser> CheckExistingUserEmail(string email)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Users.CheckExistingEmail.Replace("{email}", email));
            ApplicationUser userToFound = null;

            if (response.IsSuccessStatusCode)
            {
                userToFound = await JsonSerializer.DeserializeAsync<ApplicationUser>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return userToFound;
        }

        protected async Task<Boolean> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<ChangePasswordVM>(changePasswordVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Users.ChangePassword, data);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
                return false;
        }

        protected async Task<Boolean> UpdateProfile(ProfileVM profileVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<ProfileVM>(profileVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PutAsync(ApiRoutes.Users.UpdateProfile, data);

            if (response.IsSuccessStatusCode)
            {
                return await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
                return false;
        }
    }
}
