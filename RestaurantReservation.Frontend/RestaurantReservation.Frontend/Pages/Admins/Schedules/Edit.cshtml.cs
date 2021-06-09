using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Schedules
{
    public class EditModel : PageModel
    {
        public EditModel(IAuthenticateService authenticateService,
                         IScheduleService scheduleService)
        {
            AuthenticateService = authenticateService;
            ScheduleService = scheduleService;
        }
        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IScheduleService ScheduleService { get; }

        [BindProperty]
        public RestaurantScheduleVM RestaurantScheduleVM { get; set; }
        public string ErrorMessage { get; set; }
        [TempData]
        public string Message { get; set; }
        [TempData]
        public string ErrorConflictMessage { get; set; }
        public string MinTime { get; set; }
        public string MaxTime { get; set; }

        public async Task<IActionResult> OnGet(long ScheduleID)
        {
            RestaurantScheduleVM = await GetRestaurantScheduleByID(ScheduleID);

            if (RestaurantScheduleVM == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

            MinTime = RestaurantScheduleVM.StartWorkingTime.ToString("HH:mm");
            MaxTime = RestaurantScheduleVM.EndWorkingTime.ToString("HH:mm");
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                RestaurantScheduleVM.Schedule.AvailableSeat = RestaurantScheduleVM.Schedule.Capacity;
                RestaurantScheduleVM.Schedule.EditedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;
                RestaurantScheduleVM.Schedule.EditedDate = DateTime.Now;
                RestaurantScheduleVM.Schedule.EndTime = RestaurantScheduleVM.Schedule.StartTime.AddHours(RestaurantScheduleVM.Duration);

                var result = await ScheduleService.Update(Request.Cookies["AccessToken"].ToString(), RestaurantScheduleVM.Schedule);
                switch (result)
                {
                    case SD.StatusCode.OK:
                        Message = $"Schedule info updated successfully.";
                        return RedirectToPage("/Restaurants/Details", new { ID = RestaurantScheduleVM.Schedule.RestaurantID });
                    case SD.StatusCode.BAD_REQUEST:
                        ErrorMessage = "Duplicate Schedule start time is prohibited, try another one";
                        break;
                    case SD.StatusCode.INTERNAL:
                        return RedirectToPage("/Error");
                    default:
                        ErrorConflictMessage = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was cancelled and the latest value have been displayed. If you still want to edit this record, edit again.";
                        return RedirectToPage("/Restaurants/Details", new { ID = RestaurantScheduleVM.Schedule.RestaurantID });
                }
            }
            return Page();
        }

        private async Task<RestaurantScheduleVM> GetRestaurantScheduleByID(long ID)
        {
            await RefreshToken();

            var result = await ScheduleService.GetByID(Request.Cookies["AccessToken"].ToString(), ID);
            result.Schedule.VersionNo = Convert.ToBase64String(result.Schedule.RowVersion);

            return result;
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
