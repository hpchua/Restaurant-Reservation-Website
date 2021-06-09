using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Schedules
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(IAuthenticateService authenticateService,
                            IScheduleService scheduleService)
        {
            AuthenticateService = authenticateService;
            ScheduleService = scheduleService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IScheduleService ScheduleService { get; }

        public RestaurantScheduleVM RestaurantScheduleVM { get; set; }
        public async Task<IActionResult> OnGet(long ScheduleID)
        {
            RestaurantScheduleVM = await GetRestaurantScheduleByID(ScheduleID);

            if (RestaurantScheduleVM == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

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
