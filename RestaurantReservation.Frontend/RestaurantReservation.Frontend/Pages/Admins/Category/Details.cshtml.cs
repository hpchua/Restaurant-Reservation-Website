using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Category
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(IAuthenticateService authenticateService, ICategoryService categoryService)
        {
            AuthenticateService = authenticateService;
            CategoryService = categoryService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public ICategoryService CategoryService { get; }

        public Core.Entities.Category Category { get; set; }

        public async Task<IActionResult> OnGet(long ID)
        {
            Category = await GetCategoryByID(ID);

            if (Category.CategoryID == 0)
                return RedirectToPage("/StatusCode", new { code = 400 });

            return Page();
        }

        private async Task<Core.Entities.Category> GetCategoryByID(long ID)
        {
            await RefreshToken();

            var category = await CategoryService.GetByID(Request.Cookies["AccessToken"].ToString(), ID);
            category.VersionNo = Convert.ToBase64String(category.RowVersion);

            return category;
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
