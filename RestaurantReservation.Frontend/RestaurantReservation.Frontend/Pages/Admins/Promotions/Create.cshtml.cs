using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Promotions
{
    public class CreateModel : PageModel
    {
        public CreateModel(IAuthenticateService authenticateService,
                           IRestaurantService restaurantService,
                           IPromotionService promotionService)
        {
            AuthenticateService = authenticateService;
            RestaurantService = restaurantService;
            PromotionService = promotionService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IRestaurantService RestaurantService { get; }
        [Inject]
        public IPromotionService PromotionService { get; }

        [BindProperty]
        public RestaurantPromotionVM RestaurantPromotionVM { get; set; }

        [TempData]
        public string Message { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(long RestaurantID)
        {
            await RefreshToken();

            var restaurant = await GetRestaurantDetails(RestaurantID);

            RestaurantPromotionVM = new RestaurantPromotionVM
            {
                RestaurantID = restaurant.RestaurantID,
                RestaurantName = restaurant.Name,
                RestaurantWorkingDay = restaurant.WorkingDay,
                StartWorkingTime = restaurant.StartWorkingTime,
                EndWorkingTime = restaurant.EndWorkingTime,
                Promotion = new Promotion(),
            };
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                RestaurantPromotionVM.Promotion.CreatedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;
                RestaurantPromotionVM.Promotion.CreatedDate = DateTime.Now;
                RestaurantPromotionVM.Promotion.isAvailable = true;
                RestaurantPromotionVM.Promotion.Type = Enum.GetName(typeof(SD.PromotionType), Convert.ToInt32(RestaurantPromotionVM.Promotion.Type));
                RestaurantPromotionVM.Promotion.RestaurantID = RestaurantPromotionVM.RestaurantID;
                RestaurantPromotionVM.Promotion.isEmailCreatedSent = false;

                await RefreshToken();
                var result = await PromotionService.Add(Request.Cookies["AccessToken"].ToString(), RestaurantPromotionVM.Promotion);

                if (result)
                {
                    Message = $"New promotion added successfully.";
                    return RedirectToPage("/Restaurants/Details", new { ID = RestaurantPromotionVM.RestaurantID });
                }
                else
                {
                    ModelState.AddModelError("RestaurantPromotionVM.Promotion.Name", "Duplicate Promotion Name Detected");
                    ErrorMessage = "Failed to add promotion, please try another info";
                }
            }
            return Page();
        }

        private async Task<Restaurant> GetRestaurantDetails(long ID)
        {
            await RefreshToken();
            var restaurantDetails = await RestaurantService.GetByID(Request.Cookies["AccessToken"].ToString(), ID, "All", "All");
            return restaurantDetails.Restaurant;
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
