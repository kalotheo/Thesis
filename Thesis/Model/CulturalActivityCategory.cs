using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class CulturalActivityCategory
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

        public CulturalActivityCategory CulturalActivityParent { get; set; }

        [ForeignKey("CulturalActivityParent")]
        [Display(Name = "Category Parent")]
        public int? CategoryParent { get; set; }
    }
}
