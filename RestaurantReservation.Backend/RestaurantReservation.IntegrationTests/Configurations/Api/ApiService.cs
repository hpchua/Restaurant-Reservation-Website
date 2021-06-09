using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RestaurantReservation.Api;
using RestaurantReservation.Core.DTO;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Infrastructure.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.IntegrationTests.Configurations.Api
{
    public class ApiService
    {
        protected readonly HttpClient httpClient;

        protected ApiService()
        {
            var appFactory = new WebApplicationFactory<Startup>()
                 .WithWebHostBuilder(builder =>
                 {
                     builder.ConfigureServices(services =>
                     {
                         services.RemoveAll(typeof(DatabaseContext));
                         services.AddDbContext<DatabaseContext>(options =>
                         {
                             options.UseInMemoryDatabase("TestDb");
                         });
                     });
                 });

            httpClient = appFactory.CreateClient();
        }

        #region Get Jwt Token
        //  Admin
        protected async Task AuthenticateAdminAsync()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetAdminJwt());
        }

        private async Task<string> GetAdminJwt()
        {
            var body = JsonSerializer.Serialize<LoginVM>(new LoginVM
            {
                Username = "admin",
                Password = "testing123",
                RememberMe = false,
            });
            var data = new StringContent(body, Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Authenticate.Login, data);

            var authResult = await JsonSerializer.DeserializeAsync<AuthenticationResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return authResult.Token;
        }

        //  Member
        protected async Task AuthenticateMemberAsync()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetMemberJwt());
        }

        private async Task<string> GetMemberJwt()
        {
            var body = JsonSerializer.Serialize<LoginVM>(new LoginVM
            {
                Username = "member1",
                Password = "testing123",
                RememberMe = false,
            });
            var data = new StringContent(body, Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Authenticate.Login, data);

            var authResult = await JsonSerializer.DeserializeAsync<AuthenticationResult>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return authResult.Token;
        }
        #endregion
    }
}
