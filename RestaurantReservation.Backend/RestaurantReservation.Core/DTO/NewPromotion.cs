using RestaurantReservation.Core.Entities;

namespace RestaurantReservation.Core.DTO
{
    public class NewPromotion
    {
        public string Title { get; set; }
        public Promotion Promotion { get; set; }
    }
}
