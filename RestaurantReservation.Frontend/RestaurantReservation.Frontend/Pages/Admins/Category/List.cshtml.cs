using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Admins.Category
{
    public class ListModel : PageModel
    {
        public ListModel(IAuthenticateService authenticateService,
                         ICategoryService categoryService)
        {
            AuthenticateService = authenticateService;
            CategoryService = categoryService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }
        [Inject]
        public ICategoryService CategoryService { get; }

        public PaginatedList<RestaurantReservation.Core.Entities.Category> CategoryList { get; set; }

        public string SearchTerm { get; set; }
        public int TotalRow { get; set; }
        public int TotalRowAfterFilter { get; set; }

        private const int PAGE_SIZE = 5;

        [BindProperty(SupportsGet = true)]
        public string DeleteSuccessMessage { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public async Task<IActionResult> OnGet(string searchString, int? pageIndex)
        {
            await RefreshToken();

            SearchTerm = searchString?.Trim() ?? "";

            var categories = await CategoryService.GetAll(Request.Cookies["AccessToken"].ToString());
            TotalRow = categories.Count();   // Return total number of categories

            if (!string.IsNullOrEmpty(SearchTerm))
                categories = categories.Where(c => c.Name.ToLower().Contains(searchString.ToLower()));

            TotalRowAfterFilter = categories.Count();  // Return total number of categories after filtering
            CategoryList = PaginatedList<RestaurantReservation.Core.Entities.Category>.Create(categories.AsQueryable<RestaurantReservation.Core.Entities.Category>(), pageIndex ?? 1, PAGE_SIZE);

            if (CategoryList.TotalPages < pageIndex && pageIndex > 1)
            {
                pageIndex--;

                return RedirectToPage("List", new { searchString, pageIndex });
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDelete(long id, string searchString, int pageIndex)
        {
            var userId = User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value;
            await RefreshToken();
            var success = await CategoryService.Delete(Request.Cookies["AccessToken"].ToString(), id, userId);

            if (!success)
                ErrorMessage = "Category cannot be deleted as there are some restaurants belong to this category.";

            var successMessage = "Category record deleted successfully";
            return RedirectToPage("List", new { searchString, pageIndex, DeleteSuccessMessage = successMessage });

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
