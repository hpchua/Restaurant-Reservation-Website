using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Users
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(IAuthenticateService authenticateService, 
                            IUserService userService)
        {
            AuthenticateService = authenticateService;
            UserService = userService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IUserService UserService { get; }

        public ApplicationUser UserDetail { get; set; }

        public async Task<IActionResult> OnGet(string ID)
        {
            UserDetail = await GetUserByID(ID);

            if (UserDetail == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

            return Page();
        }

        private async Task<ApplicationUser> GetUserByID(string ID)
        {
            await RefreshToken();

            return await UserService.GetByID(ID, Request.Cookies["AccessToken"].ToString());
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
