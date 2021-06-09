using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantReservation.Frontend.ViewComponents
{
    public class ReservationCountSideViewComponent : ViewComponent
    {
        private readonly IBookingService bookingService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public ReservationCountSideViewComponent(IBookingService bookingService,
                                             IHttpContextAccessor httpContextAccessor)
        {
            this.bookingService = bookingService;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View(await bookingService.AdminGetPendingCount());
        }
    }
}
