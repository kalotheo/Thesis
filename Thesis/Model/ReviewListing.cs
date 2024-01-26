using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class ReviewListing
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime ReviewDate { get; set; }

        [Required]
        [Display(Name = "Rating")]
        public double Rating { get; set; }

        [Required]
        [Display(Name = "Review")]
        public string ReviewMessage { get; set; } 

        public User User { get; set; }

        [Required]
        [ForeignKey("User")]
        public string IdReviewer { get; set; }

        public Listing Listing { get; set; }

        [Required]
        [ForeignKey("Listing")]
        public int ListingId { get; set; }
    }
}
