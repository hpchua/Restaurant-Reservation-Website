using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Restaurant_Schedule")]
    public class RestaurantSchedule
    {
        [Key]
        public long ScheduleID { get; set; }

        [Required]
        public long RestaurantID { get; set; }

        [ForeignKey("RestaurantID")]
        public Restaurant Restaurant { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        [Required]
        public DateTime EndTime { get; set; }

        [Required]
        public int Capacity { get; set; }

        [MaxLength(20)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(20)]
        public string? EditedBy { get; set; }

        public DateTime? EditedDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [NotMapped]
        public string VersionNo { get; set; }

        public int AvailableSeat { get; set; }
        public int Status { get; set; }
    }
}
