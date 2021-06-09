using System;

namespace RestaurantReservation.Api.EmailService.Models
{
    public class Promotion
    {
        public long PromotionID { get; set; }
        public long RestaurantID { get; set; }
        public Restaurant Restaurant { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Type { get; set; }
        public bool isAvailable { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public byte[] RowVersion { get; set; }
        public bool isEmailCreatedSent { get; set; }
    }

}
