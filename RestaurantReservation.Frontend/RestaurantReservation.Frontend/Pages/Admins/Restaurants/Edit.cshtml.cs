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
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Admins;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Restaurants
{
    public class EditModel : PageModel
    {
        public EditModel(IAuthenticateService authenticateService,
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

        [TempData]
        public string Message { get; set; }
        [TempData]
        public string ErrorConflictMessage { get; set; }

        public string ErrorMessage { get; set; }


        public async Task<IActionResult> OnGet(long ID)
        {
            await GetCategoryList();
            RestaurantCategoryVM = await RestaurantService.GetEditRestaurantByID(Request.Cookies["AccessToken"].ToString(), ID);
            RestaurantCategoryVM.Restaurant.SelectedWorkingDay = GetWorkingDayList(RestaurantCategoryVM.Restaurant.WorkingDay);
            RestaurantCategoryVM.Restaurant.VersionNo = Convert.ToBase64String(RestaurantCategoryVM.Restaurant.RowVersion);
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    string filename = Guid.NewGuid().ToString();
                    string extension = Path.GetExtension(files[0].FileName);
                    string webRootPath = HostEnvironment.WebRootPath;
                    string uploadPathPrefix = Path.Combine(webRootPath, "images", "restaurants");

                    if (RestaurantCategoryVM.Restaurant.ImageUrl != null) // There is an existing image for this product
                    {
                        string imageurl = RestaurantCategoryVM.Restaurant.ImageUrl.TrimStart('~', '/').Replace("/", "\\");

                        string existingImagePath = Path.Combine(webRootPath, imageurl);
                        if (System.IO.File.Exists(existingImagePath))
                            System.IO.File.Delete(existingImagePath);
                    }

                    using (var fileStream = new FileStream(Path.Combine(uploadPathPrefix, filename + extension), FileMode.Create))
                    {
                        await files[0].CopyToAsync(fileStream);
                    }
                    RestaurantCategoryVM.Restaurant.ImageUrl = @"~/images/restaurants/" + filename + extension;
                }

                switch (Convert.ToInt32(RestaurantCategoryVM.Restaurant.SelectedWorkingDay))
                {
                    case 1:
                        RestaurantCategoryVM.Restaurant.WorkingDay = "Daily ";
                        break;
                    case 2:
                        RestaurantCategoryVM.Restaurant.WorkingDay = "Weekend ";
                        break;
                }

                RestaurantCategoryVM.Restaurant.EditedDate = DateTime.Now;
                RestaurantCategoryVM.Restaurant.EditedBy = User.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName).Value;

                var result = await RestaurantService.Update(Request.Cookies["AccessToken"].ToString(), RestaurantCategoryVM);
                switch (result)
                {
                    case SD.StatusCode.OK:
                        Message = $"Restaurant {RestaurantCategoryVM.Restaurant.Name} existing info updated successfully.";
                        return RedirectToPage("List");
                    case SD.StatusCode.BAD_REQUEST:
                        ErrorMessage = "Duplicate Restaurant name is prohibited, try another one";
                        break;
                    case SD.StatusCode.INTERNAL:
                        return RedirectToPage("/Error");
                    default:
                        ErrorConflictMessage = "The record you attempted to edit was modified by another user after you got the original value. The record has been refreshed. If you still want to edit this record, edit again.";
                        return RedirectToPage("List");
                }
            }
            return Page();
        }

        private int GetWorkingDayList(string workingDay)
        {
            if (workingDay.Contains(SD.WorkingDays.Daily.ToString()))
                return (int)SD.WorkingDays.Daily;
            else
                return (int)SD.WorkingDays.Weekend;
        }

        private async Task GetCategoryList()
        {
            await RefreshToken();
            var categories = await CategoryService.GetAll(Request.Cookies["AccessToken"].ToString());
            CategoryList = categories.Select(item => new SelectListItem
            {
                Text = item.Name,
                Value = item.CategoryID.ToString(),
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
