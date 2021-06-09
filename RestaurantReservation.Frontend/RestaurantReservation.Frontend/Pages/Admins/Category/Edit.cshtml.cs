using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Category
{
    public class EditModel : PageModel
    {
        public EditModel(IAuthenticateService authenticateService,
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
        [TempData]
        public string ConflictErrorMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(long ID)
        {
            Category = await GetCategoryByID(ID);

            if (Category.CategoryID == 0)
                return RedirectToPage("/StatusCode", new { code = 400 });

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                await RefreshToken();

                Category.EditedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;
                Category.EditedDate = DateTime.Now;

                var result = await CategoryService.Update(Request.Cookies["AccessToken"].ToString(), Category);
                switch (result)
                {
                    case SD.StatusCode.OK:
                        Message = $"Category {Category.Name} existing info updated successfully.";
                        return RedirectToPage("List");
                    case SD.StatusCode.BAD_REQUEST:
                        ErrorMessage = "Duplicate category name is prohibited, try another one";
                        break;
                    case SD.StatusCode.INTERNAL:
                        return RedirectToPage("/Error");
                    default:
                        ConflictErrorMessage = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was cancelled. If you still want to edit this record, edit again.";
                        return RedirectToPage("List");
                }
            }
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
