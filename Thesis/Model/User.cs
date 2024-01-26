using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Thesis.Model
{
    public class User : IdentityUser
    {
        public string ProfilePicture { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string FavouriteCategories { get; set; }

        public string FavouriteTags { get; set; }

        public DateTime RegistrationDate { get; set; }

        public ICollection<ReviewListing> UserReviewsListings { get; set; }

        public ICollection<ReviewCulturalActivity> UserReviewsCulturalActivities { get; set; }

        public ICollection<FavouriteCulturalActivity> UserFavouriteCulturalActivities { get; set; }

        public ICollection<FavouriteListing> UserFavouriteListings { get; set; }

        public ICollection<Request> UserRequests { get; set; }

    }

    public class FileUserViewModel
    {
        [Display(Name = "Profile Image (Optional)")]
        public IFormFile Files { get; set; }
    }
}
