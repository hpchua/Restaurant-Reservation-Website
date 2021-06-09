using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.IntegrationTests.Configurations.Api.Services
{
    public class AuthenticateService : ApiService
    {
        public static class AuthenticateInputs
        {
            public static readonly string newMockMemberUsername = "member99";
            public static readonly string fakeMemberEmail = "testinggg@gmail.com";

            public static readonly string existingMemberUsername = "member1";
            public static readonly string existingMemberEmail = "member1@gmail.com";
        }

        protected async Task<AuthenticationResult> Login(LoginVM loginInput)
        {
            var data = new StringContent(JsonSerializer.Serialize<LoginVM>(loginInput), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Authenticate.Login, data);

            return await JsonSerializer.DeserializeAsync<AuthenticationResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<AuthenticationResult> RegisterMember(RegisterVM registerVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<RegisterVM>(registerVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Authenticate.Register, data);

            return await JsonSerializer.DeserializeAsync<AuthenticationResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllMemberUsers()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Users.GetAll.Replace("{role}", SD.ROLE_MEMBER));
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

        public async Task<Boolean> DeleteMemberUser(string userID)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Users.DeleteUser.Replace("{userId}", userID));
            
            if (response.StatusCode != HttpStatusCode.NoContent)
                return false;

            return true;
        }

        protected async Task<Boolean> ForgotPassword(string email)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Authenticate.ForgotPassword.Replace("{email}", email.ToString()));
            return await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
