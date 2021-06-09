using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantReservation.Core.Entities
{
    [Table("Restaurant_Category")]
    public class RestaurantCategory
    {
        [Key]
        public long RestaurantCategoryID { get; set; }

        [Required]
        public long RestaurantID { get; set; }

        [Required]
        public long CategoryID { get; set; }

        [ForeignKey("RestaurantID")]
        public Restaurant Restaurant { get; set; }

        [ForeignKey("CategoryID")]
        public Category Category { get; set; }
    }
}
