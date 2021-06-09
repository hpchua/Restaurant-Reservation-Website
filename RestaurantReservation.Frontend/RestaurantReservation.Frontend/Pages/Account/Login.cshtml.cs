using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Account

{
    public class LoginModel : PageModel
    {
        public LoginModel(IAuthenticateService authenticateService)
        {
            AuthenticateService = authenticateService;
        }

        [BindProperty]
        public LoginVM Input { get; set; }

        [TempData]
        public string Message { get; set; }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var authResult = await AuthenticateService.Login(Input);

                if (authResult.StatusCode == SD.StatusCode.OK)
                {
                    #region Building Up Identity For Authenticated User
                    var UserClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.NameIdentifier, authResult.ApplicationUser.Id),
                            new Claim(JwtClaimTypes.GivenName, authResult.ApplicationUser.UserName),
                            new Claim(ClaimTypes.Email, authResult.ApplicationUser.Email),
                            new Claim(ClaimTypes.Role, authResult.ApplicationUser.Role),
                            new Claim(ClaimTypes.MobilePhone, authResult.ApplicationUser.PhoneNumber),
                        };

                    var UserClaimsIdentity = new ClaimsIdentity(UserClaims, "Password");
                    #endregion

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                                                  new ClaimsPrincipal(UserClaimsIdentity),
                                                  new AuthenticationProperties
                                                  {
                                                      IsPersistent = Input.RememberMe,
                                                      RedirectUri = returnUrl
                                                  });

                    HttpContext.Response.Cookies.Append("AccessToken",
                                                        authResult.Token,
                                                        new CookieOptions
                                                        {
                                                            HttpOnly = true,
                                                            SameSite = SameSiteMode.Strict
                                                        });
                    HttpContext.Response.Cookies.Append("RefreshToken",
                                                        authResult.RefreshToken,
                                                        new CookieOptions
                                                        {
                                                            HttpOnly = true,
                                                            SameSite = SameSiteMode.Strict
                                                        });

                    if (returnUrl != null)
                        return LocalRedirect(returnUrl);
                    else
                    {
                        switch (authResult.ApplicationUser.Role)
                        {
                            case SD.ROLE_ADMIN:
                                return RedirectToPage("/Admins/Home");
                            case SD.ROLE_MEMBER:
                                return RedirectToPage("/Index");
                        }
                    }
                }
                else if (authResult.StatusCode == SD.StatusCode.NOT_FOUND || authResult.StatusCode == SD.StatusCode.UNAUTHORIZED)
                    Message = authResult.Message[0];
            }
            return RedirectToPage();
        }
    }
}
