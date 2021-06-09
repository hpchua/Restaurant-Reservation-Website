using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.ViewModels.Members;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Members.History
{
    public class ListModel : PageModel
    {
        public ListModel(IAuthenticateService authenticateService,
                         IBookingService bookingService)
        {
            AuthenticateService = authenticateService;
            BookingService = bookingService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IBookingService BookingService { get; }

        public BookingHistoryVM BookingHistoryList { get; set; }
        public int TotalRow { get; set; }

        public async Task<IActionResult> OnGet(string status)
        {
            var StatusFilter = status?.Trim() ?? "all";

            await RefreshToken();
            string userID = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var bookingHistory = await BookingService.GetAllBookingsByUserID(Request.Cookies["AccessToken"].ToString(), userID, StatusFilter);
            TotalRow = bookingHistory.Bookings.Count();   // Return total number of users

            BookingHistoryList = bookingHistory;

            return Page();
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
