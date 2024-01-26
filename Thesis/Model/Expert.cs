using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class Expert
    {
        public User User { get; set; }

        [ForeignKey("User")]
        [Key]
        public string Id { get; set; }   

        [Required]
        [Display(Name = "Hourly Rate")]
        public double HourlyRate { get; set; }

        [Required]
        [Display(Name = "Availability")]
        public string Availability { get; set; }

        [Required]
        [Display(Name = "Experience")]
        public string Experience { get; set; }

        [Required]
        [Display(Name = "Profile Info")]
        public string ProfileInfo { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        public double? AverageRating { get; set; }

        public int? CountAllRatings { get; set; }

        public ICollection<Listing> ExpertsListings { get; set; }

        public ICollection<Offer> ExpertsOffers { get; set; }

    }
    
}
