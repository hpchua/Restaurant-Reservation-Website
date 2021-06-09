using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Promotions
{
    public class DetailsModel : PageModel
    {
        public DetailsModel(IAuthenticateService authenticateService,
                         IPromotionService promotionService)
        {
            AuthenticateService = authenticateService;
            PromotionService = promotionService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IPromotionService PromotionService { get; }
        [TempData]
        public string ErrorConflictMessage { get; set; }

        public RestaurantPromotionVM RestaurantPromotionVM { get; set; }
        public string Message { get; set; }

        public async Task<IActionResult> OnGet(long PromotionID)
        {
            RestaurantPromotionVM = await GetPromotionByID(PromotionID);

            if (RestaurantPromotionVM == null)
                return RedirectToPage("/StatusCode", new { code = 400 });

            return Page();
        }

        public async Task<IActionResult> OnGetPushEmail(long PromotionID, long RestaurantID)
        {
            await RefreshToken();

            var promotion = await PromotionService.CheckExistingByID(Request.Cookies["AccessToken"].ToString(), PromotionID);
            if (promotion == null)
            {
                ErrorConflictMessage = "The promotion you want to send email has been deleted by another admin";
                return RedirectToPage("/Restaurants/Details", new { ID = RestaurantID });
            }

            var result = await PromotionService.PushEmail(Request.Cookies["AccessToken"].ToString(), PromotionID);

            switch (result)
            {
                case true:
                    Message = $"Email have been sent successfully.";
                    break;
            }

            RestaurantPromotionVM = await GetPromotionByID(PromotionID);
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
