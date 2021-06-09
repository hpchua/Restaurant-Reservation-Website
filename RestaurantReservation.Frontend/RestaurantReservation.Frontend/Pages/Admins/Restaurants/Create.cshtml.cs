using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Restaurants
{
    public class CreateModel : PageModel
    {
        public CreateModel(IAuthenticateService authenticateService,
                           IRestaurantService restaurantService,
                           ICategoryService categoryService,
                           IWebHostEnvironment hostEnvironment)
        {
            AuthenticateService = authenticateService;
            RestaurantService = restaurantService;
            CategoryService = categoryService;
            HostEnvironment = hostEnvironment;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IRestaurantService RestaurantService { get; }
        [Inject]
        public ICategoryService CategoryService { get; }
        public IWebHostEnvironment HostEnvironment { get; }
        [BindProperty]
        public RestaurantCategoryVM RestaurantCategoryVM { get; set; }

        public IEnumerable<SelectListItem> CategoryList { get; set; }

        [BindProperty]
        public IEnumerable<string> SelectedIds { get; set; }

        [TempData]
        public string Message { get; set; }

        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await GetCategoryList();

            RestaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = new Restaurant(),
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (RestaurantCategoryVM.Restaurant.EndWorkingTime < RestaurantCategoryVM.Restaurant.StartWorkingTime)
            {
                ModelState.AddModelError("RestaurantCategoryVM.Restaurant.OperatingHour", "Invalid working hour, please select again");
                return Page();
            }

            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string filename = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);
                    string webRootPath = HostEnvironment.WebRootPath;
                    string uploadPathPrefix = Path.Combine(webRootPath, "images", "restaurants");

                    using (var fileStream = new FileStream(Path.Combine(uploadPathPrefix, filename + extension), FileMode.Create))
                    {
                        await files[0].CopyToAsync(fileStream);
                    }
                    RestaurantCategoryVM.Restaurant.ImageUrl = @"~/images/restaurants/" + filename + extension;
                }

                switch (Convert.ToInt32(RestaurantCategoryVM.Restaurant.WorkingDay))
                {
                    case 1:
                        RestaurantCategoryVM.Restaurant.WorkingDay = "Daily ";
                        break;
                    case 2:
                        RestaurantCategoryVM.Restaurant.WorkingDay = "Weekend ";
                        break;
                }

                RestaurantCategoryVM.Restaurant.IsAvailable = true;
                RestaurantCategoryVM.Restaurant.CreatedDate = DateTime.Now;
                RestaurantCategoryVM.Restaurant.CreatedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;

                await RefreshToken();
                var result = await RestaurantService.Add(Request.Cookies["AccessToken"].ToString(), RestaurantCategoryVM);
                if (result)
                {
                    Message = $"New restaurant {RestaurantCategoryVM.Restaurant.Name} added successfully.";
                    return RedirectToPage("List");
                }
                else
                {
                    ErrorMessage = $"{RestaurantCategoryVM.Restaurant.Name} Restaurant cannot be added due to duplicate name, try another one.";
                }
            }

            RestaurantCategoryVM = new RestaurantCategoryVM
            {
                Restaurant = new Restaurant(),
            };
            await GetCategoryList();
            return Page();
        }

        private async Task GetCategoryList()
        {
            await RefreshToken();
            var categories = await CategoryService.GetAll(Request.Cookies["AccessToken"].ToString());
            CategoryList = categories.Select(item => new SelectListItem
            {
                Text = item.Name,
                Value = item.CategoryID.ToString()
            }).ToList();
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
