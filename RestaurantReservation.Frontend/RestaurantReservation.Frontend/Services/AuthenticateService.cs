using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<AuthenticateService> logger;

        public AuthenticateService(IHttpClientFactory httpClientFactory,
                                   IConfiguration configuration,
                                   ILogger<AuthenticateService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:Authenticate"];
            this.logger = logger;
        }

        #region Login - POST 
        public async Task<AuthenticationResult> Login(LoginVM loginInput)
        {
            var body = JsonSerializer.Serialize<LoginVM>(loginInput);
            var data = new StringContent(body, Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync($"{route}/login", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var authResult = await JsonSerializer.DeserializeAsync<AuthenticationResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {authResult}");
            return authResult;
        }
        #endregion

        #region Refresh Token - POST 
        public async Task<UserToken> RefreshToken(string accessToken, string refreshToken)
        {
            RefreshRequest refreshRequest = new RefreshRequest(accessToken, refreshToken);

            var body = JsonSerializer.Serialize<RefreshRequest>(refreshRequest);
            var data = new StringContent(body, Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync($"{route}/refreshToken", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var result = await JsonSerializer.DeserializeAsync<UserToken>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {result}");
            return result;
        }
        #endregion

        #region Register - POST 
        public async Task<AuthenticationResult> Register(RegisterVM registerInput)
        {
            var data = new StringContent(JsonSerializer.Serialize<RegisterVM>(registerInput), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync($"{route}/register", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");
            response.EnsureSuccessStatusCode();

            var authResult = await JsonSerializer.DeserializeAsync<AuthenticationResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {authResult}");
            return authResult;
        }
        #endregion

        #region ForgotPassword - GET 
        public async Task<Boolean> ForgotPassword(string email)
        {
            var response = await httpClient.GetAsync($"{route}/ForgotPassword/{email}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");
            response.EnsureSuccessStatusCode();

            var result = await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {result}");
            return result;
        }
        #endregion

        #region ResetPassword - POST 
        public async Task<Boolean> ResetPassword(ResetPasswordVM resetPasswordVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<ResetPasswordVM>(resetPasswordVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync($"{route}/ResetPassword", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");
            response.EnsureSuccessStatusCode();

            var result = await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {result}");
            return result;
        }
        #endregion
    }
}
