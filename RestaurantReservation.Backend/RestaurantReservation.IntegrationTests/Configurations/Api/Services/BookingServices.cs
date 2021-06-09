using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Core.ViewModels.Members;
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
    public class BookingServices : ApiService
    {
        public static class BookingInputs
        {
            public static readonly string existingBookingNo = "l3711";
            public static readonly string existingMemberUsername = "member1";

            public static readonly string fakeBookingNo = "Testing123";
        }

        protected async Task<Int32> GetCount()
        {
            var response = await httpClient.GetAsync(ApiRoutes.Bookings.GetCount);

            return await JsonSerializer.DeserializeAsync<Int32>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<List<Booking>> GetAll(string status)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Bookings.GetAll.Replace("{status}", status));
            List<Booking> restaurantSchedules = new List<Booking>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                restaurantSchedules = await JsonSerializer.DeserializeAsync<List<Booking>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return restaurantSchedules;
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUser()
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

        protected async Task<BookingDetailVM> GetByBookingNo(string bookingNo)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Bookings.GetByBookingNo.Replace("{bookingNo}", bookingNo));
            BookingDetailVM bookingDetailVM = new BookingDetailVM();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                bookingDetailVM = await JsonSerializer.DeserializeAsync<BookingDetailVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return bookingDetailVM;
        }

        protected async Task<BookingHistoryVM> GetAllBookingsByUserID(string userID, string status)
        {
            var response = await httpClient.GetAsync(ApiRoutes.Bookings.GetAllByUserID.Replace("{userID}/{status}", $"{userID}/{status}"));
            BookingHistoryVM bookingHistoryVM = new BookingHistoryVM();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStreamAsync();
                bookingHistoryVM = await JsonSerializer.DeserializeAsync<BookingHistoryVM>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            return bookingHistoryVM;
        }

        protected async Task<Booking> Create(MakeBookingVM makeBookingVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<MakeBookingVM>(makeBookingVM), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PostAsync(ApiRoutes.Bookings.Post, data);
            var json = await response.Content.ReadAsStreamAsync();

            return await JsonSerializer.DeserializeAsync<Booking>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        protected async Task<HttpStatusCode> Update(long bookingID, Booking booking)
        {
            var data = new StringContent(JsonSerializer.Serialize<Booking>(booking), Encoding.UTF8, SD.CONTENT_JSON);
            var response = await httpClient.PutAsync(ApiRoutes.Bookings.Put.Replace("{bookingID}", booking.ToString()), data);

            return response.StatusCode;
        }

        protected async Task<Boolean> Delete(long id)
        {
            var response = await httpClient.DeleteAsync(ApiRoutes.Schedules.Delete.Replace("{id}", $"{id}"));

            if (response.IsSuccessStatusCode)
                return true;
            else
                return false;
        }
    }
}
