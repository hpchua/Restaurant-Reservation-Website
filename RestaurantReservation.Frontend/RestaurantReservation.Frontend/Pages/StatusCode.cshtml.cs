using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RestaurantReservation.Frontend.Pages
{
    public class StatusCodeModel : PageModel
    {
        private readonly ILogger<StatusCodeModel> logger;

        public StatusCodeModel(ILogger<StatusCodeModel> logger)
        {
            this.logger = logger;
        }

        public int ErrorStatusCode { get; set; }

        public IActionResult OnGet(int code)
        {
            ErrorStatusCode = code;

            return Page();
        }
    }
}
