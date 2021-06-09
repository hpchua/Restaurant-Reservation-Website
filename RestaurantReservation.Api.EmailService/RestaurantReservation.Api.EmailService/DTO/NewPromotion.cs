using RestaurantReservation.Api.EmailService.Models;

namespace RestaurantReservation.Api.EmailService.DTO
{
    public class NewPromotion
    {
        public string  Title { get; set; }
        public Promotion Promotion { get; set; }
    }
}
