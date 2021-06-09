using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Restaurant")]
    public class Restaurant
    {
        [Key]
        public long RestaurantID { get; set; }

        [Required]
        [Display(Name = "Restaurant Name")]
        [MaxLength(50)]
        public string Name { get; set; }

        [NotMapped]
        [Display(Name = "Operating Hour")]
        public string OperatingHour { get; set; }

        [MaxLength(20)]
        public string WorkingDay { get; set; }

        [NotMapped]
        public int SelectedWorkingDay { get; set; }
        public DateTime StartWorkingTime { get; set; }
        public DateTime EndWorkingTime { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [Required]
        [MaxLength(100)]
        public string Address { get; set; }

        [Required]
        public bool IsAvailable { get; set; }

        [StringLength(80)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [StringLength(80)]
        public string EditedBy { get; set; }

        public DateTime? EditedDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [NotMapped]
        public string VersionNo { get; set; }
    }
}
