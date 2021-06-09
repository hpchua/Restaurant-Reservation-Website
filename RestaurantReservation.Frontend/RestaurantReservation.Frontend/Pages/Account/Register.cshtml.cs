using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Core.ViewModels.Accounts;
using RestaurantReservation.Frontend.Services.Interfaces;

namespace RestaurantReservation.Frontend.Pages.Account
{
    public class RegisterModel : PageModel
    {
        public RegisterModel(IAuthenticateService authenticateService)
        {
            AuthenticateService = authenticateService;
        }

        [Inject]
        public IAuthenticateService AuthenticateService { get; }

        [BindProperty]
        public RegisterVM Input { get; set; }

        [TempData]
        public string RegisterSuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                Input.Role = SD.ROLE_MEMBER;
                var authResult = await AuthenticateService.Register(Input);
                switch (authResult.StatusCode)
                {
                    case SD.StatusCode.OK:
                        RegisterSuccessMessage = "Registration success. Click the link below to login.";
                        return RedirectToPage("Register");
                    case SD.StatusCode.BAD_REQUEST:
                    default:
                        foreach (var error in authResult.Message)
                        {
                            ModelState.AddModelError(string.Empty, error);
                        }
                        break;
                }
            }
            return Page();
        }
    }
}
