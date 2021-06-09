using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.Pages
{
    public class IndexModel : PageModel
    {
        public IndexModel(IAuthenticateService authenticateService,
                          IRestaurantService restaurantService)
        {
            AuthenticateService = authenticateService;
            RestaurantService = restaurantService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IRestaurantService RestaurantService { get; }

        public IEnumerable<RestaurantCategoryVM> RestaurantList { get; set; }

        public async Task<IActionResult> OnGet()
        {
            RestaurantList = await GetAllRestaurants();
            return Page();
        }

        private async Task<IEnumerable<RestaurantCategoryVM>> GetAllRestaurants()
        {
            return await RestaurantService.GetAll();
        }
    }
}
