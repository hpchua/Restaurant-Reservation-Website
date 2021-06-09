using System;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace RestaurantReservation.Frontend.Pages
{
    public class AccessDeniedModel : PageModel
    {
        private readonly ILogger<AccessDeniedModel> logger;

        public AccessDeniedModel(ILogger<AccessDeniedModel> logger)
        {
            this.logger = logger;
        }

        public void OnGet()
        {
            logger.LogWarning($"Somebody unauthorized access at [{DateTime.Now}]");
        }
    }
}
