using System;

namespace RestaurantReservation.Api.EmailService.Models
{
    public class Restaurant
    {
        public long RestaurantID { get; set; }
        public string Name { get; set; }
        public string OperatingHour { get; set; }
        public string WorkingDay { get; set; }
        public int SelectedWorkingDay { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime EndWorkingTime { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public bool IsAvailable { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string EditedBy { get; set; }
        public DateTime? EditedDate { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
