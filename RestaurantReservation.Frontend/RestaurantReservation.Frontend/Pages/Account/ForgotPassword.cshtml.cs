using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        public ForgotPasswordModel(IAuthenticateService authenticateService)
        {
            AuthenticateService = authenticateService;
        }

        [BindProperty]
        public string Email { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string FailedMessage { get; set; }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }

        public async Task<IActionResult> OnPostAsync()
        {
            var isSuccess = await AuthenticateService.ForgotPassword(Email);

            if (!isSuccess)
            {
                FailedMessage = "No account has been registered for this Email";
            }
            else
            {
                SuccessMessage = "Check for your Email ya";
            }

            return RedirectToPage();
        }
    }
}
