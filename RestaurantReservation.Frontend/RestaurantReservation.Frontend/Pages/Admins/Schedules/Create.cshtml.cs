using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Schedules
{
    public class CreateModel : PageModel
    {
        public CreateModel(IAuthenticateService authenticateService,
                           IRestaurantService restaurantService,
                           IScheduleService scheduleService)
        {
            AuthenticateService = authenticateService;
            RestaurantService = restaurantService;
            ScheduleService = scheduleService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IRestaurantService RestaurantService { get; }
        [Inject]
        public IScheduleService ScheduleService { get; }

        [BindProperty]
        public RestaurantScheduleVM RestaurantScheduleVM { get; set; }

        [TempData]
        public string Message { get; set; }

        public string ErrorMessage { get; set; }
        public string MinTime { get; set; }
        public string MaxTime { get; set; }

        public async Task<IActionResult> OnGet(long RestaurantID)
        {
            await RefreshToken();

            var restaurant = await GetRestaurantDetails(RestaurantID);

            RestaurantScheduleVM = new RestaurantScheduleVM
            {
                RestaurantID = restaurant.RestaurantID,
                RestaurantName = restaurant.Name,
                RestaurantWorkingDay = restaurant.WorkingDay,
                StartWorkingTime = restaurant.StartWorkingTime,
                EndWorkingTime = restaurant.EndWorkingTime,
                Schedule = new RestaurantSchedule(),
                Duration = 1,
            };
            MinTime = RestaurantScheduleVM.StartWorkingTime.ToString("HH:mm");
            MaxTime = RestaurantScheduleVM.EndWorkingTime.ToString("HH:mm");
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                DateTime currentDate = DateTime.Now.Date;
                DateTime scheduleDate = RestaurantScheduleVM.Schedule.ScheduleDate.Date;
                int startTime = TimeSpan.Compare(RestaurantScheduleVM.Schedule.StartTime.TimeOfDay, DateTime.Now.TimeOfDay);

                if (currentDate == scheduleDate && startTime < 0)
                {
                    ErrorMessage = "Invalid Start Time, please try enter valid time";

                    var restaurant = await GetRestaurantDetails(RestaurantScheduleVM.RestaurantID);

                    RestaurantScheduleVM = new RestaurantScheduleVM
                    {
                        RestaurantID = restaurant.RestaurantID,
                        RestaurantName = restaurant.Name,
                        RestaurantWorkingDay = restaurant.WorkingDay,
                        StartWorkingTime = restaurant.StartWorkingTime,
                        EndWorkingTime = restaurant.EndWorkingTime,
                        Schedule = new RestaurantSchedule(),
                        Duration = 1,
                    };
                    MinTime = RestaurantScheduleVM.StartWorkingTime.ToString("HH:mm");
                    MaxTime = RestaurantScheduleVM.EndWorkingTime.ToString("HH:mm");
                }
                else
                {
                    RestaurantScheduleVM.Schedule.AvailableSeat = RestaurantScheduleVM.Schedule.Capacity;
                    RestaurantScheduleVM.Schedule.CreatedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;
                    RestaurantScheduleVM.Schedule.CreatedDate = DateTime.Now;
                    RestaurantScheduleVM.Schedule.StartTime = RestaurantScheduleVM.Schedule.ScheduleDate.Add(RestaurantScheduleVM.Schedule.StartTime.TimeOfDay);
                    RestaurantScheduleVM.Schedule.EndTime = RestaurantScheduleVM.Schedule.StartTime.AddHours(RestaurantScheduleVM.Duration);
                    RestaurantScheduleVM.Schedule.Status = (int)SD.ScheduleStatus.Available;
                    RestaurantScheduleVM.Schedule.RestaurantID = RestaurantScheduleVM.RestaurantID;

                    await RefreshToken();
                    var result = await ScheduleService.Add(Request.Cookies["AccessToken"].ToString(), RestaurantScheduleVM.Schedule);

                    if (result)
                    {
                        Message = $"New schedule added successfully.";
                        return RedirectToPage("/Restaurants/Details", new { ID = RestaurantScheduleVM.RestaurantID });
                    }
                    else
                        ErrorMessage = "Failed to add schedule into database, please try another info";
                }
            }
            return Page();
        }

        private async Task<Restaurant> GetRestaurantDetails(long ID)
        {
            await RefreshToken();
            var restaurantDetails = await RestaurantService.GetByID(Request.Cookies["AccessToken"].ToString(), ID, "All", "All");
            return restaurantDetails.Restaurant;
        }

        private async Task RefreshToken()
        {
            var userToken = await AuthenticateService.RefreshToken(Request.Cookies["AccessToken"].ToString(), Request.Cookies["RefreshToken"].ToString());
            HttpContext.Response.Cookies.Delete("AccessToken");
            HttpContext.Response.Cookies.Append("AccessToken",
                                                userToken.Token,
                                                new CookieOptions
                                                {
                                                    HttpOnly = true,
                                                    SameSite = SameSiteMode.Strict
                                                });
        }
    }
}
