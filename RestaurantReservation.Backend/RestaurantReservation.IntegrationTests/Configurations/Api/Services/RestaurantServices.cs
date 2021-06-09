using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
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
    public class RestaurantServices : ApiService
    {
        public static class RestaurantInputs
        {
            public static readonly DateTime currentTime = DateTime.Now;
            public static readonly string newRestaurantName = "Happy Restaurant";
            public static readonly string editRestaurantName = "Happy Happy Restaurant";

            public static readonly long panMeeRestaurantID = 2;
            public static readonly string panMeeRestaurantName = "Super Kitchen Chili Pan Mee Kepong";

            public static readonly long fakeRestaurantID = 9_999;

            public static readonly List<long> CategoryIDsForCreateRestaurant = new List<long>(new long[] { 1, 2, 3 });
            public static readonly List<long> CategoryIDsForUpdateRestaurant = new List<long>(new long[] { 4, 5, 7 });
        }

        protected async Task<Int32> GetCount()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Restaurants.GetCount);

            return await JsonSerializer.DeserializeAsync<Int32>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<IEnumerable<RestaurantCategoryVM>> GetAll()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Restaurants.GetAll);
            var restaurantCategoryVM = Enumerable.Empty<RestaurantCategoryVM>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantCategoryVM = await JsonSerializer.DeserializeAsync<IEnumerable<RestaurantCategoryVM>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurantCategoryVM;
        }

        protected async Task<RestaurantCategorySchedulePromtionVM> GetByID(long ID, string ScheduleStatus, string PromotionStatus)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Restaurants.GetById.Replace("{id}/{ScheduleStatus}/{PromotionStatus}", $"{ID}/{ScheduleStatus}/{PromotionStatus}"));
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

        protected async Task<RestaurantCategoryVM> GetEditInfoById(long ID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Restaurants.GetEditInfoById.Replace("{id}", $"{ID}"));
            RestaurantCategoryVM restaurant = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurant = await JsonSerializer.DeserializeAsync<RestaurantCategoryVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurant;
        }

        protected async Task<Boolean> Create(RestaurantCategoryVM restaurantCategoryVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantCategoryVM>(restaurantCategoryVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Restaurants.Post, data);

            return await JsonSerializer.DeserializeAsync<Boolean>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<HttpStatusCode> Update(long id, RestaurantCategoryVM restaurantCategoryVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantCategoryVM>(restaurantCategoryVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PutAsync(ApiRoutes.Restaurants.Put.Replace("{id}", id.ToString()), data);

            return response.StatusCode;
        }

        protected async Task<HttpStatusCode> Delete(long id)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Restaurants.Delete.Replace("{userId}/{id}", $"{"testUserID"}/{id.ToString()}"));

            return response.StatusCode;
        }

        protected async Task<HttpStatusCode> DeleteCompletely(long id)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Restaurants.DeleteCompletely.Replace("{id}", $"{id.ToString()}"));

            return response.StatusCode;
        }
    }
}
