using System.ComponentModel.DataAnnotations;

namespace Thesis.Model
{
    public class ListingCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Category")]
        public string CategoryName { get; set; }

        [Display(Name = "Category Description")]
        public string CategoryDescription { get; set; }

        [Display(Name = "Category Icon")]
        public string CategoryIcon { get; set; }
    }
}
