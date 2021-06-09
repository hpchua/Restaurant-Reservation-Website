using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Promotion")]
    public class Promotion
    {
        [Key]
        public long PromotionID { get; set; }

        [Required]
        public long RestaurantID { get; set; }

        [ForeignKey("RestaurantID")]
        public Restaurant Restaurant { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(200)]
        public string Description { get; set; }

        [Required]
        [MaxLength(300)]
        public string Content { get; set; }

        [MaxLength(10)]
        public string Type { get; set; }

        [Required]
        public bool isAvailable { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(20)]
        public string? EditedBy { get; set; }

        public DateTime? EditedDate { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }

        [NotMapped]
        public string VersionNo { get; set; }

        public bool isEmailCreatedSent { get; set; }
    }
}
