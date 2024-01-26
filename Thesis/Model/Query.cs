using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thesis.Data;

namespace Thesis.Model
{
    public class Query
    {
        private readonly ApplicationDbContext _db;

        [TempData]
        public string StatusMessage { get; set; }

        public Query(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<int> GetUnreadMessages(string userId)
        {
            // get count of messages where the logged-in user is the receiver and read bool is false
            int UnreadMessages = await _db.Message.Where(x => x.IdReceiver == userId && x.Read == false).CountAsync();
            return UnreadMessages;
        }

        // dictionary that key is cultural activity categories ids
        // and value is their number of cultural activities
        public Dictionary<int, int> CulturalActivityCategoryItems()
        {
            var CulturalActivityCategoryItems = new Dictionary<int, int>();

            var query = from CulturalActivity in _db.CulturalActivity
                        group new { CulturalActivity } by new
                        {
                            CulturalActivity.CategoryId
                            
                        } into g
                        select new
                        {
                            Category = g.Key.CategoryId,
                            CountAllCulturalActivities = g.Count()
                        };

            // for every item in query
            foreach (var item in query)
            {
                if (item.Category != null)
                {
                    // add category id as key and number of all cultural activities as value to dictionary
                    CulturalActivityCategoryItems.Add((int)item.Category, item.CountAllCulturalActivities);
                }
            }

            return CulturalActivityCategoryItems;
        }

        // dictionary that key is listing categories ids
        // and value is their number of listings
        public Dictionary<int, int> ListingCategoryItems()
        {
            var ListingCategoryItems = new Dictionary<int, int>();

            var query = from Listing in _db.Listing
                        group new { Listing } by new
                        {
                            Listing.CategoryId
                        } into g
                        select new
                        {
                            Category = g.Key.CategoryId,
                            CountAllListings = g.Count()
                        };

            // for every item in query
            foreach (var items in query)
            {
                if (items.Category != null)
                {
                    // add category id as key and number of all listings as value to dictionary
                    ListingCategoryItems.Add((int)items.Category, items.CountAllListings);
                }
            }

            return ListingCategoryItems;
        }

        // dictionary that key is average expert rating
        // and value is the number of count ratings
        private Dictionary<double?, int> AverageRatingOfSelectedExpert(string username)
        {
            var averageRatingOfSelectedExpert = new Dictionary<double?, int>();

            // get all reviews listings that specific expert owns
            var query = from Review in _db.ReviewListing
                        where Review.Listing.ExpertId == username
                        group new { Review.Listing, Review } by new
                        {
                            Review.Listing.ExpertId
                        } into g
                        select new
                        {
                            AverageExpertRating = (double?)g.Average(p => p.Review.Rating),
                            CountAllRatings = g.Count()
                        };

            // for every rating
            foreach (var rating in query)
            {
                // add average expert rating rounded to 1 decimal point as key and number of all ratings as value to dictionary
                averageRatingOfSelectedExpert.Add(Math.Round((double)rating.AverageExpertRating, 1), rating.CountAllRatings);
            }

            return averageRatingOfSelectedExpert;
        }

        // dictionary that key is average listing rating
        // and value is the number of count ratings
        private Dictionary<double?, int> AverageListingRatingOfSelectedListing(int id)
        {

            var averageListingRatingOfSelectedListing = new Dictionary<double?, int>();

            // get all reviews of specific listing
            var query = from Review in _db.ReviewListing
                        where Review.Listing.Id == id
                        group new { Review, Review.Listing } by new
                        {
                            Review.ListingId,
                            Review.Listing.ExpertId
                        } into g
                        select new
                        {
                            AverageListingRating = (double?)g.Average(p => p.Review.Rating),
                            CountAllRatings = g.Count()
                        };

            // for every rating
            foreach (var rating in query)
            {
                // add average listing rating rounded to 1 decimal point as key and number of all ratings as value to dictionary
                averageListingRatingOfSelectedListing.Add(Math.Round((double)rating.AverageListingRating, 1), rating.CountAllRatings);
            }

            return averageListingRatingOfSelectedListing;
        }

        // dictionary that key is average cultural activity rating
        // and value is the number of count ratings
        private Dictionary<double?, int> AverageCulturalActivityRatingOfSelectedActivity(int id)
        {

            var averageCulturalActivityRatingOfSelectedActivity = new Dictionary<double?, int>();

            // get all reviews of specific cultural activity
            var query = from Review in _db.ReviewCulturalActivity
                        where Review.CulturalActivity.Id == id
                        group new { Review, Review.CulturalActivity } by new
                        {
                            Review.CulturalActivityId
                        } into g
                        select new
                        {
                            AverageCulturalActivityRating = (double?)g.Average(p => p.Review.Rating),
                            CountAllRatings = g.Count()
                        };

            // for every rating
            foreach (var rating in query)
            {
                // add average cultural activity rating rounded to 1 decimal point as key and number of all ratings as value to dictionary
                averageCulturalActivityRatingOfSelectedActivity.Add(Math.Round((double)rating.AverageCulturalActivityRating, 1), rating.CountAllRatings);
            }

            return averageCulturalActivityRatingOfSelectedActivity;
        }

        public async Task RemoveCulturalActivityCategory(CulturalActivityCategory CulturalActivityCategory)
        {

            ICollection<CulturalActivityCategory> CulturalActivityCategories;
            ICollection<CulturalActivity> CulturalActivities;

            // if category doesn't have a parent
            if (CulturalActivityCategory.CategoryParent == null)
            {
                // get all cultural activities that have this category
                CulturalActivities = await _db.CulturalActivity.Where(x => x.CategoryId == CulturalActivityCategory.Id).ToListAsync();
                // for every matched cultural activity make category null
                foreach (var item in CulturalActivities)
                {
                    item.CategoryId = null;
                }
                // get all categories that have this category as parent
                CulturalActivityCategories = await _db.CulturalActivityCategory.Where(x => x.CategoryParent == CulturalActivityCategory.Id).ToListAsync();
                // for every matched category make category parent null
                foreach (var item in CulturalActivityCategories)
                {
                    item.CategoryParent = null;
                }
            }
            else
            {
                // get all cultural activities that have this category as a subcategory
                CulturalActivities = await _db.CulturalActivity.Where(x => x.SubcategoryId == CulturalActivityCategory.Id).ToListAsync();
                // for every matched cultural activity make subcategory null
                foreach (var item in CulturalActivities)
                {
                    item.SubcategoryId = null;
                }
                // call RemoveCategoryFromUserFavourites with parameter the category id
                // that it's going to be deleted
                await RemoveCategoryFromUserFavourites(CulturalActivityCategory.Id.ToString());
            }
            // remove category from database
            _db.CulturalActivityCategory.Remove(CulturalActivityCategory);
            // save changes in database
            await _db.SaveChangesAsync();
            StatusMessage = "Cultural Activity Category has been succesfully deleted!";
        }

        private async Task RemoveCategoryFromUserFavourites(string subcategoryId)
        {
            // get all users where they have favourite categories from database
            ICollection<User> Users = await _db.User.Where(x => x.FavouriteCategories != null).ToListAsync();
            List<string> favouriteCategories;

            // for every user
            foreach (var user in Users)
            {
                // get favourite categories of user to a string list splitted by comma
                favouriteCategories = user.FavouriteCategories.Split(',').ToList();
                // if it contains the subcategory
                if (favouriteCategories.Contains(subcategoryId))
                {
                    // remove it from the list
                    favouriteCategories.Remove(subcategoryId);
                }
                // join strings from array with comma
                user.FavouriteCategories = string.Join(",", favouriteCategories);
            }
            // save changes to database
            await _db.SaveChangesAsync();

        }

        public async Task<ICollection<FavouriteCulturalActivity>> ExistingFavouriteCulturalActivities(string userId)
        {
            // get favourite cultural activities of logged-in user
            ICollection<FavouriteCulturalActivity> ExistingFavouriteCulturalActivities = await _db.FavouriteCulturalActivity.Where(x => x.IdUser == userId).ToListAsync();
            return ExistingFavouriteCulturalActivities;
        }

        public async Task<ICollection<ReviewCulturalActivity>> ExistingReviewCulturalActivity(string userId, int culturalActivityId)
        {
            // get review of a cultural activity from the logged-in user
            ICollection<ReviewCulturalActivity> ExistingReviewCulturalActivity = await _db.ReviewCulturalActivity.Where(x => x.IdReviewer == userId && x.CulturalActivityId == culturalActivityId).ToListAsync();
            return ExistingReviewCulturalActivity;
        }

        public async Task AddFavouriteCulturalActivity(FavouriteCulturalActivity Favourite)
        {
            // add favourite cultural activity model to database
            await _db.FavouriteCulturalActivity.AddAsync(Favourite);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Added to favourites!";
        }

        public async Task RemoveFavouriteCulturalActivity(FavouriteCulturalActivity Favourite)
        {
            // remove favourite cultural activity model from database
            _db.FavouriteCulturalActivity.Remove(Favourite);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Removed from favourites!";
        }

        public async Task RemoveCulturalActivity(CulturalActivity CulturalActivity)
        {
            // remove cultural activity model from database
            _db.CulturalActivity.Remove(CulturalActivity);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Cultural Activity has been succesfully deleted!";
        }

        public async Task AddReviewCulturalActivity(ReviewCulturalActivity ReviewCulturalActivity)
        {
            // get review cultural activity from a user
            ReviewCulturalActivity ExistingReviewCulturalActivity = await _db.ReviewCulturalActivity.SingleOrDefaultAsync(x => x.IdReviewer == ReviewCulturalActivity.IdReviewer && x.CulturalActivityId == ReviewCulturalActivity.CulturalActivityId);
            // if it exists return a message
            if (ExistingReviewCulturalActivity != null)
            {
                StatusMessage = "Error. You have already submitted a review for this cultural activity!";
            }

            else
            {
                // set review cultural activity to current date
                ReviewCulturalActivity.ReviewDate = DateTime.Now;
                // add review cultural activity model to database
                await _db.ReviewCulturalActivity.AddAsync(ReviewCulturalActivity);
                // save changes to database
                await _db.SaveChangesAsync();
                // call AddUserFavourites with parameter the user id
                await AddUserFavourites(ReviewCulturalActivity.IdReviewer);
                // call EditCulturalActivityRating with parameter the cultural activity id
                await EditCulturalActivityRating(ReviewCulturalActivity.CulturalActivityId);
                StatusMessage = "Review submitted successfully!";
            }
        }

        public async Task EditReviewCulturalActivity(ReviewCulturalActivity ReviewCulturalActivity)
        {
            // get review cultural activity model based on the id
            ReviewCulturalActivity ReviewFromDb = await _db.ReviewCulturalActivity.FindAsync(ReviewCulturalActivity.Id);
            ReviewFromDb.ReviewDate = DateTime.Now;
            ReviewFromDb.Rating = ReviewCulturalActivity.Rating;
            ReviewFromDb.ReviewMessage = ReviewCulturalActivity.ReviewMessage;
            // save changes to database
            await _db.SaveChangesAsync();
            // call AddUserFavourites with parameter the user id
            await AddUserFavourites(ReviewCulturalActivity.IdReviewer);
            // call EditCulturalActivityRating with parameter the cultural activity id
            await EditCulturalActivityRating(ReviewCulturalActivity.CulturalActivityId);
            StatusMessage = "Review edited successfully!";
        }

        public async Task RemoveReviewCulturalActivity(ReviewCulturalActivity ReviewCulturalActivity)
        {
            // remove cultural activity model from database
            _db.ReviewCulturalActivity.Remove(ReviewCulturalActivity);
            // save changes to database
            await _db.SaveChangesAsync();
            // call AddUserFavourites with parameter the user id
            await AddUserFavourites(ReviewCulturalActivity.IdReviewer);
            // call EditCulturalActivityRating with parameter the cultural activity id
            await EditCulturalActivityRating(ReviewCulturalActivity.CulturalActivityId);
            StatusMessage = "Review deleted successfully!";
        }

        private async Task EditCulturalActivityRating(int culturalActivityId)
        {
            // get cultural activity model based on the id
            CulturalActivity CulturalActivity = await _db.CulturalActivity.FindAsync(culturalActivityId);
            // get average rating and the number of all reviews
            Dictionary<double?, int> averageCulturalActivityRating = AverageCulturalActivityRatingOfSelectedActivity(culturalActivityId);
            if (averageCulturalActivityRating.Count() > 0)
            {
                foreach (var rating in averageCulturalActivityRating)
                {
                    CulturalActivity.AverageRating = rating.Key;
                    CulturalActivity.CountAllRatings = rating.Value;
                }
            }
            else
            {
                CulturalActivity.AverageRating = null;
                CulturalActivity.CountAllRatings = null;
            }
            // save changes to database
            await _db.SaveChangesAsync();
        }

        private async Task AddUserFavourites(string userId)
        {
            // get all reviews of cultural activities of user where its rating is greater than or equal to 3
            ICollection<ReviewCulturalActivity> ExistingReviewCulturalActivities = 
                await _db.ReviewCulturalActivity
                .Include(x => x.CulturalActivity)
                .Where(x => x.IdReviewer == userId && x.Rating >=3)
                .ToListAsync();
            // get user model from user id
            User user = await _db.User.FindAsync(userId);
            // set favourite categories and tags to null
            user.FavouriteCategories = null;
            user.FavouriteTags = null;
            List<string> favouriteCategories = new List<string>();
            List<string> favouriteTags = new List<string>();

            // for every review
            foreach (var item in ExistingReviewCulturalActivities)
            {
                // if cultural activity has a subcategory
                if (item.CulturalActivity.SubcategoryId != null)
                {
                    // if favourite categories list doesn't contain subcategory id
                    if (!favouriteCategories.Contains(item.CulturalActivity.SubcategoryId.ToString()))
                    {
                        // add subcategory id to the list
                        favouriteCategories.Add(item.CulturalActivity.SubcategoryId.ToString());
                    }
                }
                // if it doesn't
                else
                {
                    // if it has a category
                    if (item.CulturalActivity.CategoryId != null)
                    {
                        // if favourite categories list doesn't contain category id
                        if (!favouriteCategories.Contains(item.CulturalActivity.CategoryId.ToString()))
                        {
                            // add category id to the list
                            favouriteCategories.Add(item.CulturalActivity.CategoryId.ToString());
                        }
                    }
                }
                // split cultural activity tags by comma
                string[] culturalActivityTags = item.CulturalActivity.Tags.Split(',');
                // for every tag
                foreach (var tag in culturalActivityTags)
                {
                    // if favourite tags contain tag
                    if (!favouriteTags.Contains(tag))
                    {
                        // add tag to the list
                        favouriteTags.Add(tag);
                    }
                }
            }

            if (favouriteCategories.Count() > 0)
            {
                // join strings from array with comma
                user.FavouriteCategories = string.Join(",", favouriteCategories);
            }

            if (favouriteTags.Count() > 0)
            {
                // join strings from array with comma
                user.FavouriteTags = string.Join(",", favouriteTags);
            }

            // save changes to database
            await _db.SaveChangesAsync();
        }

        public async Task<ICollection<CulturalActivity>> RecommendedCulturalActivities(User userManager)
        {
            List<string> favouriteCategories = new List<string>();
            List<string> favouriteTags = new List<string>();
            ICollection<CulturalActivity> CulturalActivities;
            // get all cultural activities including main and subcategory from database
            ICollection<CulturalActivity> RecommendedCulturalActivities = await _db.CulturalActivity
                .Include(x => x.CulturalActivityMainCategory)
                .Include(x => x.CulturalActivitySubcategory).ToListAsync();
            // clear the list
            RecommendedCulturalActivities.Clear();

            if (userManager.FavouriteCategories != null)
            {
                // get favourite categories of user to a string list splitted by comma
                favouriteCategories = userManager.FavouriteCategories.Split(',').ToList();
                // for every category
                foreach (var category in favouriteCategories)
                {
                    // get a list of cultural activities that have the same subcategory or the same category
                    CulturalActivities = await _db.CulturalActivity
                        .Where(x => x.SubcategoryId == int.Parse(category) || x.CategoryId == int.Parse(category))
                        .ToListAsync();

                    if (CulturalActivities != null)
                    {
                        // for every cultural activity in that list
                        foreach (var item in CulturalActivities)
                        {
                            // search if the user has made a review
                            ReviewCulturalActivity ExistingReviewCulturalActivity = await _db.ReviewCulturalActivity
                                .SingleOrDefaultAsync(x => x.IdReviewer == userManager.Id && x.CulturalActivityId == item.Id);

                            // if this cultural activity is not on recommended list and the user hasn't made a review about it
                            if (!RecommendedCulturalActivities.Contains(item) && ExistingReviewCulturalActivity == null)
                            {
                                // add it to the list
                                RecommendedCulturalActivities.Add(item);
                            }
                        }
                    }
                }
            }

            if (userManager.FavouriteTags != null)
            {
                // get favourite tags of user to a string list splitted by comma
                favouriteTags = userManager.FavouriteTags.Split(',').ToList();
                // for every tag
                foreach (var tag in favouriteTags)
                {
                    // get a list of cultural activities that contain the tag
                    CulturalActivities = await _db.CulturalActivity.Where(x => x.Tags.Contains(tag)).ToListAsync();

                    if (CulturalActivities != null)
                    {
                        // for every cultural activity in that list
                        foreach (var item in CulturalActivities)
                        {
                            // search if the user has made a review
                            ReviewCulturalActivity ExistingReviewCulturalActivity =
                                await _db.ReviewCulturalActivity.
                                SingleOrDefaultAsync(x => x.IdReviewer == userManager.Id && x.CulturalActivityId == item.Id);
                            // if this cultural activity is not on recommended list and the user hasn't made a review about it
                            if (!RecommendedCulturalActivities.Contains(item) && ExistingReviewCulturalActivity == null)
                            {
                                // add it to the list
                                RecommendedCulturalActivities.Add(item);
                            }
                        }
                    }
                }
            }

            return RecommendedCulturalActivities;
        }

        public async Task<ICollection<FavouriteListing>> ExistingFavouriteListings(string userId)
        {
            // get favourite listings of logged-in user
            ICollection<FavouriteListing> ExistingFavouriteListings = await _db.FavouriteListing.Where(x => x.IdUser == userId).ToListAsync();
            return ExistingFavouriteListings;
        }

        public async Task<ICollection<ReviewListing>> ExistingReviewListing(string userId, int listingId)
        {
            // get review of a listing from the logged-in user
            ICollection<ReviewListing> ExistingReviewListing = await _db.ReviewListing.Where(x => x.IdReviewer == userId && x.ListingId == listingId).ToListAsync();
            return ExistingReviewListing;
        }

        public async Task AddFavouriteListing(FavouriteListing Favourite)
        {
            // check if listing is owned from the logged-in user who tries to add it to favourites
            Listing Listing = await _db.Listing.SingleOrDefaultAsync(x => x.ExpertId == Favourite.IdUser && x.Id == Favourite.ListingId);
            if (Listing != null)
            {
                StatusMessage = "Error. You can't favourite your own listing!";
            }
            else
            {
                // add favourite listing model to database
                await _db.FavouriteListing.AddAsync(Favourite);
                // save changes to database
                await _db.SaveChangesAsync();
                StatusMessage = "Added to favourites!";
            }
        }

        public async Task RemoveFavouriteListing(FavouriteListing Favourite)
        {
            // remove favourite listing model from database
            _db.FavouriteListing.Remove(Favourite);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Removed from favourites!";
        }

        public async Task RemoveListing(Listing Listing)
        {
            // get list of messages that have a set listing id from database
            ICollection<Message> Messages = await _db.Message.Where(x => x.ListingId == Listing.Id).ToListAsync();
            // for every message
            foreach (var item in Messages)
            {
                // set listing id to null
                item.ListingId = null;
            }
            // remove listing model from database
            _db.Listing.Remove(Listing);
            // save changes to database
            await _db.SaveChangesAsync();
            // call EditExpertRating with parameter the id of the expert that owns the listing
            await EditExpertRating(Listing.ExpertId);
            StatusMessage = "Listing has been succesfully deleted!";
        }

        public async Task ChangeVisibilityListing(Listing Listing)
        {
            // if listing is visible
            if (Listing.Visibility)
            {
                // set it to invisible
                Listing.Visibility = false;
                StatusMessage = "Visibility changed to: hidden";
            }
            else
            {
                // set it to visible
                Listing.Visibility = true;
                StatusMessage = "Visibility changed to: visible";
            }
            // save changes to database
            await _db.SaveChangesAsync();
        }

        public async Task AddMessage(Message Message)
        {
            // if ids of sender and receiver are equal return an error message
            if (Message.IdSender == Message.IdReceiver)
            {
                StatusMessage = "Error. You can't send a message to yourself!";
            }
            else
            {
                // set message date of current date
                Message.MessageDate = DateTime.Now;
                Message.Read = false;
                // add message model to database
                await _db.Message.AddAsync(Message);
                // save changes to database
                await _db.SaveChangesAsync();
                StatusMessage = "Message sent successfully!";
            }
        }

        public async Task AddReviewListing(ReviewListing ReviewListing)
        {
            // check if reviewer has already submitted a review for this listing
            ReviewListing Review = await _db.ReviewListing.SingleOrDefaultAsync(x => x.IdReviewer == ReviewListing.IdReviewer && x.ListingId == ReviewListing.ListingId);
            // check if reviewer owns the listing
            Listing Listing = await _db.Listing.SingleOrDefaultAsync(x => x.ExpertId == ReviewListing.IdReviewer && x.Id == ReviewListing.ListingId);

            if (Review != null)
            {
                StatusMessage = "Error. You have already submitted a review for this listing!";
            }

            else if (Listing != null)
            {
                StatusMessage = "Error. You can't submit a review for your own listing!";
            }

            else
            {
                // set review date of current date
                ReviewListing.ReviewDate = DateTime.Now;
                // add review listing model to database
                await _db.ReviewListing.AddAsync(ReviewListing);
                // save changes to database
                await _db.SaveChangesAsync();
                // call EditListingRating with parameter the listing id
                await EditListingRating(ReviewListing.ListingId);
                StatusMessage = "Review submitted successfully!";
            }
        }

        public async Task EditReviewListing(ReviewListing ReviewListing)
        {
            // get review listing model based on the id
            ReviewListing ReviewFromDb = await _db.ReviewListing.FindAsync(ReviewListing.Id);
            // set review date of current date
            ReviewFromDb.ReviewDate = DateTime.Now;
            ReviewFromDb.Rating = ReviewListing.Rating;
            ReviewFromDb.ReviewMessage = ReviewListing.ReviewMessage;
            // save changes to database
            await _db.SaveChangesAsync();
            // call EditListingRating with parameter the listing id
            await EditListingRating(ReviewFromDb.ListingId);
            StatusMessage = "Review edited successfully!";
        }

        public async Task RemoveReviewListing(ReviewListing ReviewListing)
        {
            // remove review listing model from database
            _db.ReviewListing.Remove(ReviewListing);
            // save changes to database
            await _db.SaveChangesAsync();
            // call EditListingRating with parameter the listing id
            await EditListingRating(ReviewListing.ListingId);
            StatusMessage = "Review deleted successfully!";
        }

        private async Task EditListingRating(int listingId)
        {
            // get listing model based on the id
            Listing Listing = await _db.Listing.FindAsync(listingId);
            // get average rating and the number of all reviews
            Dictionary<double?, int> averageListingRating = AverageListingRatingOfSelectedListing(listingId);
            if (averageListingRating.Count() > 0)
            {
                foreach (var rating in averageListingRating)
                {
                    Listing.AverageRating = rating.Key;
                    Listing.CountAllRatings = rating.Value;
                }
            }
            else
            {
                Listing.AverageRating = null;
                Listing.CountAllRatings = null;
            }
            // save changes to database
            await _db.SaveChangesAsync();
            // call EditExpertRating with parameter the expert's id
            await EditExpertRating(Listing.ExpertId);
        }

        private async Task EditExpertRating(string expertId)
        {
            // get expert model based on the id
            Expert Expert = await _db.Expert.FindAsync(expertId);
            // get average rating and the number of all reviews
            Dictionary<double?, int> averageRating = AverageRatingOfSelectedExpert(expertId);
            if (averageRating.Count() > 0)
            {
                foreach (var rating in averageRating)
                {
                    Expert.AverageRating = rating.Key;
                    Expert.CountAllRatings = rating.Value;
                }
            }
            else
            {
                Expert.AverageRating = null;
                Expert.CountAllRatings = null;
            }
            // save changes to database
            await _db.SaveChangesAsync();
        }

        public async Task RemoveRequest(Request Request)
        {
            // get list of messages that have a set request id from database
            ICollection<Message> Messages = await _db.Message.Where(x => x.RequestId == Request.Id).ToListAsync();
            // for every message
            foreach (var item in Messages)
            {
                // set request id to null
                item.RequestId = null;
            }
            // get list of request's offers from database
            ICollection<Offer> Offers = await _db.Offer.Where(x => x.RequestId == Request.Id).ToListAsync();
            // remove offers from database
            _db.Offer.RemoveRange(Offers);
            // remove request model from database
            _db.Request.Remove(Request);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Request has been succesfully deleted!";
        }

        public async Task<ICollection<Offer>> ExistingOffer(string userId)
        {
            // get offer of request from the logged-in user
            ICollection<Offer> ExistingOffer = await _db.Offer.Where(x => x.ExpertId == userId).ToListAsync();
            return ExistingOffer;
        }

        public async Task AddOffer(Offer Offer)
        {
            // check if the user who makes the offer is an expert
            Expert Expert = await _db.Expert.FindAsync(Offer.ExpertId);
            // check if the user who makes the offer has created the request
            Request Request = await _db.Request.SingleOrDefaultAsync(x => x.UserId == Offer.ExpertId && x.Id == Offer.RequestId);
            // check if the user has already created an offer for this specific request
            Offer ExistingOffer = await _db.Offer.SingleOrDefaultAsync(x => x.ExpertId == Offer.ExpertId && x.RequestId == Offer.RequestId);

            if (Expert == null)
            {
                StatusMessage = "Error. You have to be an expert to make an offer!";
            }

            else if (Request != null)
            {
                StatusMessage = "Error. You can't make an offer to your own request!";
            }

            else if (ExistingOffer != null)
            {
                StatusMessage = "Error. You have already submitted an offer for this request!";
            }

            else
            {
                // set review date of current date
                Offer.OfferDate = DateTime.Now;
                // add offer model to database
                await _db.Offer.AddAsync(Offer);
                // save changes to database
                await _db.SaveChangesAsync();
                // get offer model that's already submitted
                Offer MessageOffer = await _db.Offer.Include(x => x.Request).SingleOrDefaultAsync(x => x.Id == Offer.Id);
                // create a new message model
                Message Message = new Message();
                Message.MessageDate = MessageOffer.OfferDate;
                Message.MessageText = MessageOffer.OfferText;
                Message.Read = false;
                Message.IdSender = MessageOffer.ExpertId;
                Message.IdReceiver = MessageOffer.Request.UserId;
                Message.RequestId = MessageOffer.Request.Id;
                // call AddMessage with parameter the message
                await AddMessage(Message);
                StatusMessage = "Offer submitted successfully!";
            }
        }

        public async Task EditOffer(Offer Offer)
        {
            // get offer model based on the id
            Offer OfferFromDb = await _db.Offer.FindAsync(Offer.Id);
            // set offer date of current date
            OfferFromDb.OfferDate = DateTime.Now;
            OfferFromDb.OfferText = Offer.OfferText;
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Offer edited successfully!";
        }

        public async Task RemoveOffer(Offer Offer)
        {
            // remove offer model from database
            _db.Offer.Remove(Offer);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Offer deleted successfully!";
        }

        public async Task RemoveUser(string userId)
        {

            Expert Expert = await _db.Expert.FindAsync(userId);

            // if user is expert
            if (Expert != null)
            {
                // get list of offers where expert's id equals to user's id
                ICollection<Offer> Offers = await _db.Offer.Where(x => x.ExpertId == userId).ToListAsync();
                // remove all offers from database
                _db.Offer.RemoveRange(Offers);

                // get list of user's listings
                ICollection<Listing> Listings = await _db.Listing.Where(x => x.ExpertId == userId).Include(x => x.ListingReviews).ToListAsync();

                List<ReviewListing> ExpertReviewListings = new List<ReviewListing>();

                // for every listing
                foreach (var listing in Listings)
                {
                    // for every listing's review
                    foreach (var item in listing.ListingReviews)
                    {
                        // if ExpertReviewListings list doesn't contain item
                        if (!ExpertReviewListings.Contains(item))
                        {
                            // add item to list
                            ExpertReviewListings.Add(item);
                        }
                    }
                }

                // remove all reviews from database
                _db.ReviewListing.RemoveRange(ExpertReviewListings);
            }

            // get list of user's requests
            ICollection<Request> Requests = await _db.Request.Where(x => x.UserId == userId).Include(x => x.RequestOffers).ToListAsync();

            List<Offer> OffersForExpert = new List<Offer>();

            // for every request
            foreach (var request in Requests)
            {
                // for every offer
                foreach (var item in request.RequestOffers)
                {
                    // if OffersForExpert list doesn't contain item
                    if (!OffersForExpert.Contains(item))
                    {
                        // add item to list
                        OffersForExpert.Add(item);
                    }
                }
            }

            // remove all offers from database
            _db.Offer.RemoveRange(OffersForExpert);

            // remove all requests from database
            _db.Request.RemoveRange(Requests);

            // get list of messages where user is either the sender or the receiver
            ICollection<Message> Messages = await _db.Message.Where(x => x.IdSender == userId || x.IdReceiver == userId).ToListAsync();
            // remove all messages from database
            _db.Message.RemoveRange(Messages);

            // get list of favourite's cultural activities where favourite's user id equals to user's id
            ICollection<FavouriteCulturalActivity> FavouriteCulturalActivities = await _db.FavouriteCulturalActivity.Where(x => x.IdUser == userId).ToListAsync();
            _db.FavouriteCulturalActivity.RemoveRange(FavouriteCulturalActivities);

            // get list of reviews cultural activities where reviewer's id equals to user's id
            ICollection<ReviewCulturalActivity> ReviewCulturalActivities = await _db.ReviewCulturalActivity.Where(x => x.IdReviewer == userId).ToListAsync();
            // remove the reviews
            _db.ReviewCulturalActivity.RemoveRange(ReviewCulturalActivities);

            // get list of favourite's listings where favourite's user id equals to user's id
            ICollection<FavouriteListing> FavouriteListings = await _db.FavouriteListing.Where(x => x.IdUser == userId).ToListAsync();
            _db.FavouriteListing.RemoveRange(FavouriteListings);

            // get list of reviews listings where reviewer's id equals to user's id
            ICollection<ReviewListing> ReviewListings = await _db.ReviewListing.Where(x => x.IdReviewer == userId).ToListAsync();
            // remove the reviews
            _db.ReviewListing.RemoveRange(ReviewListings);

            // save changes to database
            await _db.SaveChangesAsync();

            // for every review call EditCulturalActivityRating with parameter the cultural activity id
            foreach (var reviewCulturalActivity in ReviewCulturalActivities)
            {
                await EditCulturalActivityRating(reviewCulturalActivity.CulturalActivityId);
            }

            // for every review call EditListingRating with parameter the listing id
            foreach (var reviewListing in ReviewListings)
            {
                await EditListingRating(reviewListing.ListingId);
            }

        }

    }
}
