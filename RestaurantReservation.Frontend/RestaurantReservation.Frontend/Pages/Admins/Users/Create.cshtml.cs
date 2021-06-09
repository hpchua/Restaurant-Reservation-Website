using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Users
{
    public class CreateModel : PageModel
    {
        public CreateModel(IAuthenticateService authenticateService)
        {
            AuthenticateService = authenticateService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }

        [BindProperty]
        public RegisterVM Input { get; set; }

        [TempData]
        public string Message { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            await RefreshToken();

            Input.Role = SD.ROLE_ADMIN;
            var result = await AuthenticateService.Register(Input);
            switch (result.StatusCode)
            {
                case SD.StatusCode.OK:
                    Message = $"New admin user {Input.Name} added successfully.";
                    return RedirectToPage("List");

                case SD.StatusCode.BAD_REQUEST:
                    foreach (var error in result.Message)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    ErrorMessage = "New admin detail unable to be added successfully, please input the info again";
                    break;
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
