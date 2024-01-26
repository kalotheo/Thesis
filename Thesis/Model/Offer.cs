using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class Offer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime OfferDate { get; set; }

        [Required]
        [Display(Name = "Offer")]
        public string OfferText { get; set; }

        public Expert Expert { get; set; }

        [Required]
        [ForeignKey("Expert")]
        public string ExpertId { get; set; }

        public Request Request { get; set; }

        [Required]
        [ForeignKey("Request")]
        public int RequestId { get; set; }
    }
}
