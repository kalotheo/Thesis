using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Listings
{
    public class ViewModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ViewModel(ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Listing Listing { get; set; }

        public List<string> tags { get; set; }

        private List<Listing> ListingsTags = new List<Listing>();

        public IEnumerable<Listing> Listings { get; set; }

        private Query Query;

        public ICollection<FavouriteListing> ExistingFavourite { get; set; }

        public ICollection<ReviewListing> ExistingReview { get; set; }

        [BindProperty]
        public ReviewListing Review { get; set; }

        [BindProperty]
        public Message Message { get; set; }

        [BindProperty]
        public FavouriteListing Favourite { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            // get listing's model from database including listing category, expert, user, listing reviews models based on id
            Listing = await _db.Listing.Include(x => x.ListingCategory).Include(x => x.Expert).ThenInclude(x => x.User).Include(x => x.ListingReviews).ThenInclude(x => x.User).SingleOrDefaultAsync(x => x.Id == id);

            // if listing is null return a message
            if (Listing == null)
            {
                return NotFound($"Unable to load listing with id '{id}'.");
            }

            // if a user is logged in
            if (_signInManager.IsSignedIn(User)) {
                
                // get listing where expert's id doesn't equal to logged-in user id and is invinsible
                Listing authorizedListing = await _db.Listing.Where(x => x.ExpertId != _userManager.GetUserId(User) && x.Visibility == false).SingleOrDefaultAsync(x => x.Id == id);
                
                // if it doesn't exist return unauthorized message
                if (authorizedListing != null)
                {
                    return Unauthorized();
                }

                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all favourite listings of this user
                ExistingFavourite = await Query.ExistingFavouriteListings(_userManager.GetUserId(User));
                // get review  of this cultural activity of this user
                ExistingReview = await Query.ExistingReviewListing(_userManager.GetUserId(User), id);
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }

           else
           {
                // check if listing is invisible 
                Listing authorizedListing = await _db.Listing.Where(x => x.Visibility == false).SingleOrDefaultAsync(x => x.Id == id);

                // if it doesn't exist return unauthorized message
                if (authorizedListing != null)
                {
                    return Unauthorized();
                }
            }

            // get a list all visible listings excluding current listing from list,
            // including listing category, expert, user
            IEnumerable<Listing> AllListings = await _db.Listing
                .Include(x => x.ListingCategory)
                .Include(x => x.Expert)
                .ThenInclude(x => x.User)
                .Where(x => x.Id != Listing.Id && x.Visibility == true)
                .ToListAsync();

            // list that has all listings with the same category as the current listing
            IEnumerable<Listing> ListingsCategory = AllListings.Where(x => x.CategoryId == Listing.CategoryId);

            // get listing tags to a string list splitted by comma
            tags = Listing.Tags.Split(',').ToList();
            List<string> allTags = new List<string>();

            // for every listing
            foreach (var listing in AllListings)
            {
                // get listing tags to a string list splitted by comma
                allTags = listing.Tags.Split(',').ToList();
                // if allTags list and tags list have common tags
                if (allTags.Intersect(tags).Any())
                {
                    // if listing doesn't exist in ListingsCategory
                    if (!ListingsCategory.Contains(listing))
                    {
                        // add it to ListingsTags list
                        ListingsTags.Add(listing);
                    }
                }
            }

            // concatenate ListingsCategory list with ListingsTags and take the first 12 items ordered by their average rating
            Listings = ListingsCategory.Concat(ListingsTags).Take(12).OrderByDescending(x => x.AverageRating);

            return Page();
        }

        public async Task<IActionResult> OnPostDelete(int id, int redirectPageId)
        {
            // get listing's model from database based on id
            Listing Listing = await _db.Listing.FindAsync(id);
            // if listing doesn't exist return a message
            if (Listing == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveCulturalActivity with parameter the Listing model
            await Query.RemoveListing(Listing);
            StatusMessage = Query.StatusMessage;
            // if user is deleting this listing
            if (id == redirectPageId)
            {
                // redirect him to index
                return RedirectToPage("Index");
            }
            else
            {
                // redirect him to listing's details
                return RedirectToPage("View", new { id = redirectPageId });
            }
        }

        public async Task<IActionResult> OnPostChangeVisibility(int id, int id2)
        {
            // get listing's model from database based on id
            Listing Listing = await _db.Listing.FindAsync(id);
            // if listing doesn't exist return a message
            if (Listing == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call ChangeVisibilityListing with parameter the Listing model
            await Query.ChangeVisibilityListing(Listing);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = id2 });
        }

        public async Task<IActionResult> OnPostAddFavourite(int id)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddFavouriteListing with parameter the FavouriteListing model
            await Query.AddFavouriteListing(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = id }); 
        }

        public async Task<IActionResult> OnPostDeleteFavourite(int id, int id2)
        {
            // get favourite listing's model from database based on id
            FavouriteListing Favourite = await _db.FavouriteListing.FindAsync(id);
            // if favourite listing doesn't exist return a message
            if (Favourite == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveFavouriteListing with parameter the FavouriteListing model
            await Query.RemoveFavouriteListing(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = id2 });
        }

        public async Task<IActionResult> OnPostMessage(int redirectPageId)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddMessage with parameter the Message model
            await Query.AddMessage(Message);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = redirectPageId });
        }

        public async Task<IActionResult> OnPostReview()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddReviewListing with parameter the ReviewListing model
            await Query.AddReviewListing(Review);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = Review.ListingId });
        }

        public async Task<IActionResult> OnPostEditReview()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call EditReviewListing with parameter the ReviewListing model
            await Query.EditReviewListing(Review);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = Review.ListingId });
        }

        public async Task<IActionResult> OnPostDeleteReview(int id)
        {
            // get review listing's model from database based on id
            ReviewListing ReviewListing = await _db.ReviewListing.FindAsync(id);
            // if review listing doesn't exist return a message
            if (ReviewListing == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveReviewListing with parameter the ReviewListing model
            await Query.RemoveReviewListing(ReviewListing);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = ReviewListing.ListingId });
        }
    }
}
