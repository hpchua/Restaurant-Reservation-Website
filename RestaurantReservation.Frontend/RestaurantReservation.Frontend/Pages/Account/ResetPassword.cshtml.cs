using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        public ResetPasswordModel(IAuthenticateService authenticateService)
        {
            AuthenticateService = authenticateService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }

        [BindProperty]
        public ResetPasswordVM Input { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        [TempData]
        public string FailedMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            string email = Request.Query["Email"];
            string token = Request.Query["Token"];

            if (email == null || token == null)
            {
                FailedMessage = "Invalid access to reset password!";
            }
            else
            {
                Input.Email = email;
                Input.Token = token;

                var isSuccess = await AuthenticateService.ResetPassword(Input);

                if (!isSuccess)
                {
                    FailedMessage = "Unable to reset password, please contact admin";
                }
                else
                {
                    SuccessMessage = "Your account password has been reset successfully";
                }
            }
            return RedirectToPage();
        }
    }
}
