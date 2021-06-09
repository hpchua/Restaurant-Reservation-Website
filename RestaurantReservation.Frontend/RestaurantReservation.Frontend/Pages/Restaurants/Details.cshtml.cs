using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Core.ViewModels.Members;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Restaurants
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(IAuthenticateService authenticateService,
                            IRestaurantService restaurantService,
                            IScheduleService scheduleService,
                            IPromotionService promotionService,
                            IBookingService bookingService)
        {
            AuthenticateService = authenticateService;
            RestaurantService = restaurantService;
            ScheduleService = scheduleService;
            PromotionService = promotionService;
            BookingService = bookingService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IRestaurantService RestaurantService { get; }
        [Inject]
        public IScheduleService ScheduleService { get; }
        [Inject]
        public IPromotionService PromotionService { get; }
        [Inject]
        public IBookingService BookingService { get; }

        private const int PAGE_SIZE = 5;
        public RestaurantCategorySchedulePromtionVM RestaurantCategorySchedulePromtionVM { get; set; }
        public int TotalScheduleRow { get; set; }
        public int TotalPromotionRow { get; set; }
        public PaginatedList<RestaurantSchedule> ScheduleList { get; set; }
        public PaginatedList<RestaurantSchedule> MemberScheduleList { get; set; }
        public PaginatedList<Promotion> PromotionList { get; set; }

        [TempData]
        public string DeleteScheduleMessage { get; set; }
        [TempData]
        public string DeletePromotionMessage { get; set; }

        [BindProperty]
        public MakeBookingVM MakeBookingVM { get; set; }
        public IEnumerable<SelectListItem> BookingScheduleList { get; set; }
        public string BookSuccessMessage { get; set; }
        public string BookFailedMessage { get; set; }
        private long RestaurantID { get; set; }

        public async Task<IActionResult> OnGet(long ID, string ScheduleStatus, string PromotionStatus, int? schedulePageIndex, int? promotionPageIndex)
        {
            var ScheduleStatusFilter = ScheduleStatus?.Trim() ?? "all";
            var PromotionStatusFilter = PromotionStatus?.Trim() ?? "all";

            RestaurantCategorySchedulePromtionVM = await GetRestaurantDetails(ID, ScheduleStatusFilter, PromotionStatusFilter);

            if (RestaurantCategorySchedulePromtionVM == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

            await FilterRestaurantDetails(ID, schedulePageIndex, promotionPageIndex);

            return Page();
        }

        public async Task<IActionResult> OnPostBooking()
        {
            if (ModelState.IsValid)
            {
                await RefreshToken();
                var result = await ScheduleService.GetByID(Request.Cookies["AccessToken"].ToString(), MakeBookingVM.ScheduleID);
                if (result.Schedule.ScheduleID == MakeBookingVM.ScheduleID)
                {
                    RestaurantID = result.Schedule.RestaurantID;
                    if (MakeBookingVM.Pax > result.Schedule.AvailableSeat)
                    {
                        BookFailedMessage = "Exceed number of available places, currently it left " + result.Schedule.AvailableSeat;
                        ModelState.AddModelError("MakeBookingVM.Pax", "Current available only have: " + result.Schedule.AvailableSeat);
                    }
                    else
                    {
                        MakeBookingVM.Booking.UserID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                        MakeBookingVM.Booking.BookingDate = DateTime.Now;
                        MakeBookingVM.Booking.BookingNo = GenerateBookingNo();
                        MakeBookingVM.Booking.BookingStatus = SD.BookingStatus.PENDING;

                        await RefreshToken();
                        var newBooking = await BookingService.Add(Request.Cookies["AccessToken"].ToString(), MakeBookingVM);
                        if (newBooking != null)
                            BookSuccessMessage = "Your booking has been placed successfully, please check your email ya!";
                        else
                        {
                            result = await ScheduleService.GetByID(Request.Cookies["AccessToken"].ToString(), MakeBookingVM.ScheduleID);
                            BookFailedMessage = "Your booking unable to be placed due to insufficient seat";
                            ModelState.AddModelError("MakeBookingVM.Pax", "Current available only have: " + result.Schedule.AvailableSeat);
                        }
                    }
                }
            }

            RestaurantCategorySchedulePromtionVM = await GetRestaurantDetails(RestaurantID, "all", "all");

            if (RestaurantCategorySchedulePromtionVM == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

            await FilterRestaurantDetails(RestaurantID, 0, 0);

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteSchedule(long id, long ScheduleID)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            await RefreshToken();

            var success = await ScheduleService.Delete(Request.Cookies["AccessToken"].ToString(), ScheduleID, (int)SD.ScheduleStatus.Unavailable, userId);

            if (!success)
                DeleteScheduleMessage = "Unsuccessful due to there are some members are booked already";
            else
                DeleteScheduleMessage = "Successful";

            return RedirectToPage("/Restaurants/Details", new { ID = id });
        }

        public async Task<IActionResult> OnPostUpdateSchedule(long id, long ScheduleID)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            await RefreshToken();

            var success = await ScheduleService.Delete(Request.Cookies["AccessToken"].ToString(), ScheduleID, (int)SD.ScheduleStatus.Available, userId);

            if (!success)
                DeleteScheduleMessage = "Unsuccessful due to there are some members are booked already";
            else
                DeleteScheduleMessage = "Successful";

            return RedirectToPage("/Restaurants/Details", new { ID = id });
        }

        public async Task<IActionResult> OnPostDeletePromotion(long id, long PromotionID)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            await RefreshToken();

            var success = await PromotionService.Delete(Request.Cookies["AccessToken"].ToString(), PromotionID, userId);

            if (!success)
                DeleteScheduleMessage = "Unsuccessful";
            else
                DeleteScheduleMessage = "Successful";

            return RedirectToPage("/Restaurants/Details", new { ID = id });
        }

        private string GenerateBookingNo()
        {
            var bookingNumberBuilder = new StringBuilder();

            Random random = new Random();
            char randomChar = (char)random.Next('a', 'z');  //Generate one character
            int randomNumber = random.Next(1000, 9999);     // Generate Number

            bookingNumberBuilder.Append(randomChar);
            bookingNumberBuilder.Append(randomNumber);

            return bookingNumberBuilder.ToString();
        }

        private async Task GetBookingScheduleList(long ID)
        {
            await RefreshToken();
            var restaurantSchedules = await ScheduleService.GetAll(Request.Cookies["AccessToken"].ToString(), ID);

            BookingScheduleList = restaurantSchedules.Select(item => new SelectListItem
            {
                Text = item.ScheduleDate.ToString("dd/MM/yyyy") + " " + item.StartTime.ToString("HH:mm") + " - " + item.EndTime.ToString("HH:mm"),
                Value = item.ScheduleID.ToString()
            }).ToList();
        }

        private async Task FilterRestaurantDetails(long ID, int? schedulePageIndex, int? promotionPageIndex)
        {
            /*****     Schedule     *****/
            var schedules = RestaurantCategorySchedulePromtionVM.RestaurantSchedules;
            schedules = schedules.OrderBy(s => s.ScheduleDate);
            TotalScheduleRow = schedules.Count();

            var memberSchedules = RestaurantCategorySchedulePromtionVM.MemberRestaurantSchedules;
            memberSchedules = memberSchedules.OrderBy(s => s.ScheduleDate);
            TotalScheduleRow = memberSchedules.Count();

            ScheduleList = PaginatedList<RestaurantSchedule>.Create(schedules.AsQueryable<RestaurantSchedule>(), schedulePageIndex ?? 1, PAGE_SIZE);
            MemberScheduleList = PaginatedList<RestaurantSchedule>.Create(memberSchedules.AsQueryable<RestaurantSchedule>(), schedulePageIndex ?? 1, PAGE_SIZE);

            /*****     Promotion     *****/
            var promotions = RestaurantCategorySchedulePromtionVM.Promotions;
            promotions = promotions.OrderBy(s => s.StartDate);
            TotalPromotionRow = promotions.Count();

            PromotionList = PaginatedList<Promotion>.Create(promotions.AsQueryable<Promotion>(), promotionPageIndex ?? 1, PAGE_SIZE);
            await GetBookingScheduleList(ID);
        }

        private async Task<RestaurantCategorySchedulePromtionVM> GetRestaurantDetails(long ID, string ScheduleStatus, string PromotionStatus)
        {
            await RefreshToken();
            return await RestaurantService.GetByID(Request.Cookies["AccessToken"].ToString(), ID, ScheduleStatus, PromotionStatus);
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
