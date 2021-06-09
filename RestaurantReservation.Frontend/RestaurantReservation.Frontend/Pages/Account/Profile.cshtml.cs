using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Account
{
    public class ProfileModel : PageModel
    {
        public ProfileModel(IAuthenticateService authenticateService,
                           IUserService userService)
        {
            AuthenticateService = authenticateService;
            UserService = userService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IUserService UserService { get; }

        [BindProperty]
        public ProfileVM Input { get; set; }
        [TempData]
        public string SuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet()
        {
            Input = await GetUserProfile();
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var user = await UserService.CheckExistingEmail(Input.Email, Request.Cookies["AccessToken"].ToString());
                if (user != null)
                {
                    if (!user.Id.Trim().ToLower().Equals(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value))
                    {
                        await DisplayErrorMessage("The email has been used by another member, please try another email");
                        return Page();
                    }
                }

                var result = await UserService.Update(Input, Request.Cookies["AccessToken"].ToString());
                if (result)
                {
                    SuccessMessage = "Profile updated successfully!";
                    return RedirectToPage("Profile");
                }
                else
                {
                    await DisplayErrorMessage("Wrong current password, please enter correct password");
                    return Page();
                }
            }
            return Page();
        }

        private async Task DisplayErrorMessage(string message)
        {
            ErrorMessage = message;
            Input = await GetUserProfile();
        }

        private async Task<ProfileVM> GetUserProfile()
        {
            await RefreshToken();
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            var user = await UserService.GetByID(userId, Request.Cookies["AccessToken"].ToString());

            return new ProfileVM
            {
                UserID = user.Id,
                Username = user.UserName,
                Name = user.Name,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                Subscribe = user.isSubscriber,
            };
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
