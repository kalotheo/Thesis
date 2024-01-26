using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class ReviewCulturalActivity
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

        public CulturalActivity CulturalActivity { get; set; }

        [Required]
        [ForeignKey("CulturalActivity")]
        public int CulturalActivityId { get; set; }
    }
}
