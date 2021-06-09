using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Core.ViewModels.Members;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Services
{
    public class BookingService : IBookingService
    {
        private readonly HttpClient httpClient;
        private readonly string route;
        private readonly ILogger<BookingService> logger;

        public BookingService(IHttpClientFactory httpClientFactory,
                               IConfiguration configuration,
                               ILogger<BookingService> logger)
        {
            httpClient = httpClientFactory.CreateClient("api");
            route = configuration["APIRoutes:Booking"];
            this.logger = logger;
        }

        #region AdminGetPendingCount - GET
        public async Task<Int32> AdminGetPendingCount()
        {
            var response = await httpClient.GetAsync($"{route}/Admin/ListCount");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            int count = 0;

            if (response.IsSuccessStatusCode)
            {
                count = await JsonSerializer.DeserializeAsync<Int32>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {count}");
            return count;
        }
        #endregion

        #region GetAllBookingsOfStatus - GET
        public async Task<IEnumerable<Booking>> GetAll(string token, string status)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"{route}/Admin/{status}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");
            IEnumerable<Booking> bookings = Enumerable.Empty<Booking>();

            bookings = await JsonSerializer.DeserializeAsync<IEnumerable<Booking>>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            logger.LogTrace($"RestaurantReservationSystem.API Responses : {bookings}");
            return bookings;
        }
        #endregion

        #region GetBookingDetailsByNumber - GET
        public async Task<BookingDetailVM> GetBookingDetailsByNumber(string token, string bookingNo)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"{route}/Details/BookingNo/{bookingNo}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var orderDetailsVM = await JsonSerializer.DeserializeAsync<BookingDetailVM>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {orderDetailsVM}");
                return orderDetailsVM;
            }
            else
            {
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {null}");
                return null;
            }
        }
        #endregion

        #region GetAllBookingsByUserID - GET
        public async Task<BookingHistoryVM> GetAllBookingsByUserID(string token, string userID, string status)
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.GetAsync($"{route}/Member/{userID}/{status}");
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            if (response.IsSuccessStatusCode)
            {
                var bookingHistory = await JsonSerializer.DeserializeAsync<BookingHistoryVM>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {bookingHistory}");
                return bookingHistory;
            }
            else
            {
                logger.LogTrace($"RestaurantReservationSystem.API Responses : {null}");
                return null;
            }
        }
        #endregion

        #region Add - POST
        public async Task<Booking> Add(string token, MakeBookingVM makeBookingVM)
        {
            var data = new StringContent(JsonSerializer.Serialize<MakeBookingVM>(makeBookingVM), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PostAsync(route, data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            Booking booking = null;

            if (response.IsSuccessStatusCode)
            {
                booking = await JsonSerializer.DeserializeAsync<Booking>(await response.Content.ReadAsStreamAsync(), new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {booking}");
            return booking;
        }
        #endregion

        #region Update - PUT
        public async Task<Boolean> Update(string token, Booking booking)
        {
            var data = new StringContent(JsonSerializer.Serialize<Booking>(booking), Encoding.UTF8, SD.CONTENT_JSON);
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await httpClient.PutAsync($"{route}/{booking.BookingID}", data);
            logger.LogTrace("Calling RestaurantReservationSystem.API");

            var success = false;

            if (response.IsSuccessStatusCode)
            {
                success = true;
            }
            logger.LogTrace($"RestaurantReservationSystem.API Responses : {success}");
            return success;
        }
        #endregion
    }
}
