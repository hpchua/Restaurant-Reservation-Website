using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Users
{
    public class ListModel : PageModel
    {

        public ListModel(IAuthenticateService authenticateService,
                         IUserService userService)
        {
            AuthenticateService = authenticateService;
            UserService = userService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public IUserService UserService { get; }

        public PaginatedList<ApplicationUser> UserList { get; set; }
        private const int PAGE_SIZE = 5;
        public string SearchTerm { get; set; }
        public string SearchCriterion { get; set; }
        public string RoleFilter { get; set; }
        public int TotalRow { get; set; }
        public int TotalRowAfterFilter { get; set; }
        public List<SelectListItem> SearchCriteria { get; set; }

        public async Task<IActionResult> OnGet(string searchString, string role, int? pageIndex)
        {
            RoleFilter = role?.Trim() ?? "all";
            SearchTerm = searchString?.Trim() ?? "";

            await RefreshToken();
            var UsersFromDb = await UserService.GetAll(RoleFilter, Request.Cookies["AccessToken"].ToString());
            TotalRow = UsersFromDb.Count();   // Return total number of users

            //Filter
            if (!string.IsNullOrEmpty(searchString))
            {
                string searchValue = searchString.ToLower();
                DateTime dateTime;

                if (DateTime.TryParse(searchValue, out dateTime))
                {
                    UsersFromDb = UsersFromDb.Where(u => u.Name.ToLower().Contains(searchValue) ||
                                                         u.Email.ToLower().Contains(searchValue) ||
                                                         u.PhoneNumber.ToLower().Contains(searchValue) ||
                                                         u.JoinedDate == Convert.ToDateTime(searchValue));
                }
                else
                {
                    UsersFromDb = UsersFromDb.Where(u => u.Name.ToLower().Contains(searchValue) ||
                                                         u.Email.ToLower().Contains(searchValue) ||
                                                         u.PhoneNumber.ToLower().Contains(searchValue));
                }
            }

            TotalRowAfterFilter = UsersFromDb.Count();  // Return total number of users after filtering

            UserList = PaginatedList<ApplicationUser>.Create(UsersFromDb.AsQueryable<ApplicationUser>(), pageIndex ?? 1, PAGE_SIZE);

            if (UserList.TotalPages < pageIndex && pageIndex > 1)
            {
                pageIndex--;

                return RedirectToPage("List", new { searchString, role, pageIndex });
            }

            return Page();
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
