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

namespace RestaurantReservation.Frontend.Pages.Admins.Promotions
{
    public class EditModel : PageModel
    {
        public EditModel(IAuthenticateService authenticateService,
                         IPromotionService promotionService)
        {
            AuthenticateService = authenticateService;
            PromotionService = promotionService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IPromotionService PromotionService { get; }

        [BindProperty]
        public RestaurantPromotionVM RestaurantPromotionVM { get; set; }

        [TempData]
        public string Message { get; set; }
        [TempData]
        public string ErrorConflictMessage { get; set; }
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(long PromotionID)
        {
            RestaurantPromotionVM = await GetPromotionByID(PromotionID);

            if (RestaurantPromotionVM == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                await RefreshToken();
                RestaurantPromotionVM.Promotion.Type = Enum.GetName(typeof(SD.PromotionType), Convert.ToInt32(RestaurantPromotionVM.SelectedType));
                RestaurantPromotionVM.Promotion.EditedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;
                RestaurantPromotionVM.Promotion.EditedDate = DateTime.Now;

                var result = await PromotionService.Update(Request.Cookies["AccessToken"].ToString(), RestaurantPromotionVM.Promotion);
                switch (result)
                {
                    case SD.StatusCode.OK:
                        Message = $"Promotion {RestaurantPromotionVM.Promotion.Name} existing info updated successfully.";
                        return RedirectToPage("/Restaurants/Details", new { ID = RestaurantPromotionVM.Promotion.RestaurantID });
                    case SD.StatusCode.BAD_REQUEST:
                        ErrorMessage = "Duplicate promotion name is prohibited, try another one";
                        break;
                    case SD.StatusCode.INTERNAL:
                        return RedirectToPage("/Error");
                    default:
                        ErrorConflictMessage = "The record you attempted to edit was modified by another user after you got the original value. The edit operation was cancelled and the latest value have been displayed. If you still want to edit this record, edit again.";
                        return RedirectToPage("/Restaurants/Details", new { ID = RestaurantPromotionVM.Promotion.RestaurantID });
                }
            }
            return Page();
        }

        private async Task<RestaurantPromotionVM> GetPromotionByID(long ID)
        {
            await RefreshToken();

            var result = await PromotionService.GetByID(Request.Cookies["AccessToken"].ToString(), ID);
            result.Promotion.VersionNo = Convert.ToBase64String(result.Promotion.RowVersion);
            result.SelectedType = (int)Enum.Parse(typeof(SD.PromotionType), result.Promotion.Type);

            return result;
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
