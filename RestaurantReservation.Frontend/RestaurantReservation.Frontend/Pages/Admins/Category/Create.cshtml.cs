using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Category
{
    public class CreateModel : PageModel
    {
        public CreateModel(IAuthenticateService authenticateService,
                           ICategoryService categoryService)
        {
            AuthenticateService = authenticateService;
            CategoryService = categoryService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public ICategoryService CategoryService { get; }

        [BindProperty]
        public Core.Entities.Category Category { get; set; }

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

            Category.CreatedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;
            Category.CreatedDate = DateTime.Now;
            var success = await CategoryService.Add(Request.Cookies["AccessToken"].ToString(), Category);
            if (success)
            {
                Message = $"New category {Category.Name} added successfully.";
                return RedirectToPage("List");
            }
            else
            {
                ErrorMessage = $"{Category.Name} Category cannot be added due to duplicate name, try another one.";
                return Page();
            }
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
