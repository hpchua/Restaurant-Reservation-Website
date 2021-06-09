using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.IntegrationTests.Configurations.Api.Services
{
    public class PromotionServices : ApiService
    {
        public static class PromotionInputs
        {
            public static readonly DateTime today = DateTime.Today;

            public static readonly long murniRestaurantID = 1;
            public static readonly string murniRestaurantName = "Murni Discovery Kepong";
            public static readonly long murniPromotionID = 2;

            public static readonly long existingPromotionID = 1;

            public static readonly long fakePromotionID = 9_999;
            public static readonly string fakeAuthorName = "Testing123";
        }

        protected async Task<RestaurantCategorySchedulePromtionVM> GetRestaurantByID(long ID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Restaurants.GetById.Replace("{id}", $"{ID}"));
            RestaurantCategorySchedulePromtionVM restaurant = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurant = await JsonSerializer.DeserializeAsync<RestaurantCategorySchedulePromtionVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurant;
        }

        protected async Task<RestaurantPromotionVM> GetByID(long promotionID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Promotions.GetById.Replace("{id}", $"{promotionID}"));
            RestaurantPromotionVM restaurantPromotionVM = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantPromotionVM = await JsonSerializer.DeserializeAsync<RestaurantPromotionVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurantPromotionVM;
        }

        protected async Task<Boolean> Create(Promotion promotion)
        {
            var data = new StringContent(JsonSerializer.Serialize<Promotion>(promotion), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Promotions.Post, data);

            return await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<HttpStatusCode> Update(long id, Promotion promotion)
        {
            var data = new StringContent(JsonSerializer.Serialize<Promotion>(promotion), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PutAsync(ApiRoutes.Promotions.Put.Replace("{id}", id.ToString()), data);

            return response.StatusCode;
        }

        protected async Task<Boolean> Delete(long id)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Promotions.Delete.Replace("{id}", $"{id}"));

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
