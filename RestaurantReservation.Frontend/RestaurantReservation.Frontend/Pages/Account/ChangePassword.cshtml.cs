using System;
using System.Collections.Generic;
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
    public class ChangePasswordModel : PageModel
    {
        public ChangePasswordModel(IAuthenticateService authenticateService,
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
        public ChangePasswordVM Input { get; set; }
        [TempData]
        public string ChangePasswordSuccessMessage { get; set; }
        public string ErrorMessage { get; set; }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                if (Input.NewPassword.ToLower().Trim().Equals(Input.CurrentPassword.ToLower().Trim()))
                {
                    ErrorMessage = "Your new password is same as the old password!";
                    return Page();
                }
                else
                {
                    await RefreshToken();
                    Input.UserID = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
                    var result = await UserService.ChangePassword(Request.Cookies["AccessToken"].ToString(), Input);

                    if (result)
                    {
                        ChangePasswordSuccessMessage = "Account Password changed successfully!";
                        return RedirectToPage("Profile");
                    }
                    else
                    {
                        ErrorMessage = "Wrong current password, please enter correct password";
                        return Page();
                    }
                }
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
