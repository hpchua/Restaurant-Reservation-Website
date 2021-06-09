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

namespace RestaurantReservation.Frontend.Pages.Admins.Reservation
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(IAuthenticateService authenticateService,
                            IBookingService bookingService)
        {
            AuthenticateService = authenticateService;
            BookingService = bookingService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IBookingService BookingService { get; }

        [BindProperty]
        public BookingDetailVM BookingDetailVM { get; set; }
        public string SearchTerm { get; set; }
        public string NotFoundMessage { get; set; }
        public string Message { get; set; }

        [BindProperty(SupportsGet = true)]
        public string BookingNo { get; set; }

        public async Task<IActionResult> OnGetAsync(string searchString)
        {
            string bookingNo = "";
            if (!string.IsNullOrEmpty(BookingNo))
            {
                SearchTerm = BookingNo?.Trim() ?? "";
                bookingNo = BookingNo;
            }
            else
            {
                SearchTerm = searchString?.Trim() ?? "";
                bookingNo = searchString;
            }

            await RefreshToken();
            BookingDetailVM = await BookingService.GetBookingDetailsByNumber(Request.Cookies["AccessToken"].ToString(), bookingNo);

            if (!string.IsNullOrEmpty(searchString) || !string.IsNullOrEmpty(BookingNo))
            {
                if (BookingDetailVM == null)
                    NotFoundMessage = "Invalid Booking Number, please enter another one";
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            BookingDetailVM.Booking.BookingStatus = SD.BookingStatus.COMPLETE;
            BookingDetailVM.Booking.CheckIn = DateTime.Now;
            BookingDetailVM.Booking.CheckOut = DateTime.Now;
            BookingDetailVM.Booking.EditedDate = DateTime.Now;
            BookingDetailVM.Booking.EditedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;

            await RefreshToken();
            await BookingService.Update(Request.Cookies["AccessToken"].ToString(), BookingDetailVM.Booking);
            BookingDetailVM = await BookingService.GetBookingDetailsByNumber(Request.Cookies["AccessToken"].ToString(), BookingDetailVM.Booking.BookingNo);

            Message = "Updated Successfully!";
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
