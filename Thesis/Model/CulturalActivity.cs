using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class CulturalActivity
    {
        [Key]
        public int Id { get; set; }

        public CulturalActivityCategory CulturalActivityMainCategory { get; set; }

        [ForeignKey("CulturalActivityMainCategory")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public CulturalActivityCategory CulturalActivitySubcategory { get; set; }

        [ForeignKey("CulturalActivitySubcategory")]
        [Display(Name = "Subcategory")]
        public int? SubcategoryId { get; set; }

        public string Images { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "Date Start")]
        public DateTime? DateStart { get; set; }

        [Display(Name = "Date End")]
        public DateTime? DateEnd { get; set; }

        [Required]
        [Display(Name = "Place")]
        public string Place { get; set; }

        [Display(Name = "Cast")]
        public string Cast { get; set; }

        [Display(Name = "Media")]
        public string Media { get; set; }

        public double? AverageRating { get; set; }

        public int? CountAllRatings { get; set; }

        public ICollection<ReviewCulturalActivity> CulturalActivityReviews { get; set; }
    }

    public class FileCulturalAcitivityViewModel
    {
        [Display(Name = "Images (Optional)")]
        public List<IFormFile> Files { get; set; }
    }
}
