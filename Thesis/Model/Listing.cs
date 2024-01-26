using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class Listing
    {
        [Key]
        public int Id { get; set; }

        public ListingCategory ListingCategory { get; set; }

        [ForeignKey("ListingCategory")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        public string Images { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        [Required]
        [Display(Name = "Hourly Rate")]
        public double HourlyRate { get; set; }

        [Required]
        [Display(Name = "Availability")]
        public string Availability { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        public Expert Expert { get; set; }

        [ForeignKey("Expert")]
        [Required]
        public string ExpertId { get; set; }

        public DateTime Date { get; set; }

        public bool Visibility { get; set; }

        public double? AverageRating { get; set; }

        public int? CountAllRatings { get; set; }

        public ICollection<ReviewListing> ListingReviews { get; set; }

    }

    public class FileListingViewModel
    {
        [Display(Name = "Images (Optional)")]
        public List<IFormFile> Files { get; set; }
    }
}
