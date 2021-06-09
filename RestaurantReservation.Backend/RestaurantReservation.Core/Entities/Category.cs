using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Category")]
    public class Category
    {
        [Key]
        public long CategoryID { get; set; }

        [Required]
        [Display(Name = "Category Name")]
        [MaxLength(20)]
        public string Name { get; set; }

        [MaxLength(20)]
        public string CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(20)]
        public string? EditedBy { get; set; }

        public DateTime? EditedDate { get; set; }

        [NotMapped]
        public string VersionNo { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; }
    }
}
