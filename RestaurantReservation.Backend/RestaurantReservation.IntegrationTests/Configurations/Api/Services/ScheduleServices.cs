using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.IntegrationTests.Configurations.Api.Services
{
    public class ScheduleServices : ApiService
    {
        public static class ScheduleInputs
        {
            public static readonly DateTime today = DateTime.Today;

            public static readonly long murniRestaurantID = 1;
            public static readonly string murniRestaurantName = "Murni Discovery Kepong";
            public static readonly long murniScheduleID = 2;

            public static readonly long fakeScheduleID = 9_999;
            public static readonly long fakeRestaurantID = 9_999;
            public static readonly string fakeAuthorName = "Testing123";
        }

        protected async Task<Int32> GetCount(long restaurantID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Schedules.GetCount.Replace("{restaurantID}", $"{restaurantID}"));

            return await JsonSerializer.DeserializeAsync<Int32>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<List<RestaurantSchedule>> GetAll(long restaurantID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Schedules.GetAll.Replace("{id}", restaurantID.ToString()));
            List<RestaurantSchedule> restaurantSchedules = new List<RestaurantSchedule>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantSchedules = await JsonSerializer.DeserializeAsync<List<RestaurantSchedule>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurantSchedules;
        }

        protected async Task<RestaurantScheduleVM> GetByID(long scheduleID)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Schedules.GetById.Replace("{id}", $"{scheduleID}"));
            RestaurantScheduleVM restaurantScheduleVM = null;

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantScheduleVM = await JsonSerializer.DeserializeAsync<RestaurantScheduleVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurantScheduleVM;
        }

        protected async Task<RestaurantCategorySchedulePromtionVM> GetDeleteScheduleInfoByRestaurantID(long ID)
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

        protected async Task<Boolean> Create(RestaurantSchedule restaurantSchedule)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantSchedule>(restaurantSchedule), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Schedules.Post, data);

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        protected async Task<HttpStatusCode> Update(long scheduleID, RestaurantSchedule restaurantSchedule)
        {
            var data = new StringContent(JsonSerializer.Serialize<RestaurantSchedule>(restaurantSchedule), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PutAsync(ApiRoutes.Schedules.Put.Replace("{id}", scheduleID.ToString()), data);

            return response.StatusCode;
        }

        protected async Task<Boolean> Delete(long id, int status)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Schedules.Delete.Replace("{userId}/{status}/{id}", $"{"testUserID"}/{status}/{id}"));

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }

        protected async Task<Boolean> DeleteCompletely(long id)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Schedules.DeleteCompletely.Replace("{id}", $"{id}"));

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
