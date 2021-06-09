using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Reservation
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

        private const int PAGE_SIZE = 5;
        public PaginatedList<Booking> BookingList { get; set; }

        public string SearchTerm { get; set; }
        public int TotalRow { get; set; }
        public int TotalRowAfterFilter { get; set; }
        public string StatusFilter { get; set; }

        public async Task<IActionResult> OnGet(string searchString, string status, int? pageIndex)
        {
            await RefreshToken();

            StatusFilter = status?.Trim() ?? "All";
            SearchTerm = searchString?.Trim() ?? "";

            var bookings = await BookingService.GetAll(Request.Cookies["AccessToken"].ToString(), StatusFilter);
            TotalRow = bookings.Count();   // Return total number of categories

            if (!string.IsNullOrEmpty(SearchTerm))
                bookings = bookings.Where(b => b.FullName.ToLower().Contains(searchString.ToLower()) ||
                                               b.BookingNo.ToLower().Contains(searchString.Trim().ToLower()));

            TotalRowAfterFilter = bookings.Count();  // Return total number of categories after filtering
            BookingList = PaginatedList<Booking>.Create(bookings.AsQueryable<Booking>(), pageIndex ?? 1, PAGE_SIZE);

            if (BookingList.TotalPages < pageIndex && pageIndex > 1)
            {
                pageIndex--;
                return RedirectToPage("List", new { searchString, status, pageIndex });
            }
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
