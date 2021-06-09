using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Restaurants
{
    public class ListModel : PageModel
    {
        public ListModel(IAuthenticateService authenticateService,
                         IRestaurantService restaurantService,
                         ICategoryService categoryService)
        {
            AuthenticateService = authenticateService;
            RestaurantService = restaurantService;
            CategoryService = categoryService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IRestaurantService RestaurantService { get; }
        [Inject]
        public ICategoryService CategoryService { get; }
        public List<SelectListItem> CategoryList { get; set; }

        private const int PAGE_SIZE = 5;
        public string SearchTerm { get; set; }
        public string CategoryFilter { get; set; }
        public int TotalRow { get; set; }
        public int TotalRowAfterFilter { get; set; }

        public PaginatedList<RestaurantCategoryVM> RestaurantList { get; set; }

        [TempData]
        public string Message { get; set; }

        public async Task OnGet(string searchString, string category, int? pageIndex)
        {
            await RefreshToken();
            var restaurants = await RestaurantService.GetAll();
            TotalRow = restaurants.Count();   // Return total number of categories

            CategoryFilter = category?.Trim() ?? "All";
            SearchTerm = searchString?.Trim() ?? "";

            CategoryList = new List<SelectListItem> {
                new SelectListItem
                {
                    Text = "All",
                    Value = "All",
                    Selected = (CategoryFilter == "All" ? true : false)
                }
            };
            await RefreshToken();
            CategoryList.AddRange((await CategoryService.GetAll(Request.Cookies["AccessToken"].ToString())).Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Name,
                Selected = (x.Name == category ? true : false)
            }).ToList());

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                restaurants = restaurants.Where(r => r.Restaurant.Name.ToLower().Contains(SearchTerm.ToLower()));
                TotalRowAfterFilter = restaurants.Count();  // Return total number of categories after filtering
            }

            if (!string.IsNullOrEmpty(category) && CategoryFilter != "All")
            {
                restaurants = restaurants.Where(r => r.Categories.Contains(category, StringComparer.OrdinalIgnoreCase));
                TotalRowAfterFilter = restaurants.Count();  // Return total number of categories after filtering
            }

            RestaurantList = PaginatedList<RestaurantCategoryVM>.Create(restaurants.AsQueryable<RestaurantCategoryVM>(), pageIndex ?? 1, PAGE_SIZE);
        }

        public async Task<IActionResult> OnPostDelete(long id, string searchString, string category, int pageIndex)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            await RefreshToken();

            var success = await RestaurantService.Delete(Request.Cookies["AccessToken"].ToString(), id, userId);

            if (!success)
                Message = "Unsuccessful";
            else
                Message = "Successful";

            return RedirectToPage("List", new { searchString, category, pageIndex });
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
