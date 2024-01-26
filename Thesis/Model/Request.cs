using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class Request
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Title")]
        public string Title { get; set; }

        public string Images { get; set; }

        [Required]
        [Display(Name = "Salary per month")]
        public double Salary { get; set; }

        [Required]
        [Display(Name = "Date Start")]
        public DateTime DateStart { get; set; }

        [Required]
        [Display(Name = "Date End")]
        public DateTime DateEnd { get; set; }

        [Required]
        [Display(Name = "Time Range")]
        public string TimeRange { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Tags")]
        public string Tags { get; set; }

        public DateTime DateAdded { get; set; }

        public User User { get; set; }

        [ForeignKey("User")]
        [Required]
        public string UserId { get; set; }

        public ICollection<Offer> RequestOffers { get; set; }
    }

    public class FileRequestViewModel
    {
        [Display(Name = "Images (Optional)")]
        public List<IFormFile> Files { get; set; }
    }
}
