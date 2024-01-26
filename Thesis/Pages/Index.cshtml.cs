using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        private Query Query;

        public List<string> CulturalActivitiesTagsList = new List<string>();

        public ICollection<CulturalActivityCategory> CulturalActivityCategories { get; private set; }

        public ICollection<CulturalActivity> CulturalActivities { get; private set; }

        private ICollection<CulturalActivity> RecommendedCulturalActivities { get; set; }

        public List<string> ListingsTagsList = new List<string>();

        public Dictionary<int, int> CulturalActivityCategoryItems = new Dictionary<int, int>();

        public ICollection<FavouriteCulturalActivity> ExistingFavourite { get; set; }

        [BindProperty]
        public FavouriteCulturalActivity Favourite { get; set; }

        public IEnumerable<Expert> Experts { get; private set; }

        public Dictionary<int, int> ListingCategoryItems = new Dictionary<int, int>();

        public ICollection<ListingCategory> ListingCategories { get; private set; }

        public int ListingsCount { get; private set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        [BindProperty]
        public Message Message { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public string ContentTitle { get; set; }

        public string ContentSubtitle { get; set; }

        public IndexModel(ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
        }

        public async Task OnGet()
        {

            // get all cultural activities and group them by their tags
            IEnumerable<IGrouping<string, CulturalActivity>> CulturalActivitiesTags = _db.CulturalActivity.OrderByDescending(x => x.AverageRating).ToList().GroupBy(x => x.Tags);
            List<string> culturalActivitiesTags = new List<string>();

            // for every cultural activity
            foreach (var item in CulturalActivitiesTags)
            {
                // get cultural activities tags to a string list splitted by comma
                culturalActivitiesTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in culturalActivitiesTags)
                {
                    // if it doesn't exist in tags list
                    if (!CulturalActivitiesTagsList.Contains(tag))
                    {
                        // add it to tags list
                        CulturalActivitiesTagsList.Add(tag);
                    }
                }
            }

            // get all cultural activity categories
            CulturalActivityCategories = await _db.CulturalActivityCategory.ToListAsync();

            // get all cultural activities including main, subcategory and reviews models
            // ordering them by non nullable average rating
            CulturalActivities = await _db.CulturalActivity
                .Include(x => x.CulturalActivityMainCategory)
                .Include(x => x.CulturalActivitySubcategory)
                .Include(x => x.CulturalActivityReviews)
                .Where(x => x.AverageRating != null)
                .OrderByDescending(x => x.AverageRating).ToListAsync();

            ContentTitle = "Featured";

            ContentSubtitle = "Cultural activities worth paying attention to";

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);

            // get cultural activity categories and their number of cultural activities
            CulturalActivityCategoryItems = Query.CulturalActivityCategoryItems();

            // get all cultural activities and group them by their tags
            IEnumerable<IGrouping<string, Listing>> ListingsTags = _db.Listing.OrderByDescending(x => x.AverageRating).ToList().GroupBy(x => x.Tags);
            List<string> listingsTags = new List<string>();

            // for every listing
            foreach (var item in ListingsTags)
            {
                // get listings tags to a string list splitted by comma
                listingsTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in listingsTags)
                {
                    // if it doesn't exist in tags list
                    if (!ListingsTagsList.Contains(tag))
                    {
                        // add it to tags list
                        ListingsTagsList.Add(tag);
                    }
                }
            }

            // get number of listings from database
            ListingsCount = await _db.Listing.CountAsync();

            // get all experts including user model ordering them by non nullable average rating
            Experts = await _db.Expert
                .Include(x => x.User)
                .Where(x => x.AverageRating != null)
                .OrderByDescending(x => x.AverageRating).ToListAsync();

            // get listing categories and their number of listings
            ListingCategoryItems = Query.ListingCategoryItems();

            // get all listing categories
            ListingCategories = await _db.ListingCategory.ToListAsync();

            // if a user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                // get user model from database based on user id
                User userManager = await _db.User.FindAsync(_userManager.GetUserId(User));

                // if user has favourite categories or tags
                if(userManager.FavouriteCategories != null || userManager.FavouriteTags != null) 
                {
                    // get recommendations list calling the reccomended cultural activities
                    // passing the parameter of user
                    RecommendedCulturalActivities = await Query.RecommendedCulturalActivities(userManager);
                    if (RecommendedCulturalActivities.Count() > 0)
                    {
                        ContentTitle = "Recommendations for you";
                        ContentSubtitle = "Cultural activities just for you";
                        CulturalActivities = RecommendedCulturalActivities;
                    }

                }
                // get all favourite listings of this user
                ExistingFavourite = await Query.ExistingFavouriteCulturalActivities(_userManager.GetUserId(User));
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get cultural activity's model from database based on id
            CulturalActivity CulturalActivity = await _db.CulturalActivity.FindAsync(id);
            // if cultural activity doesn't exist return a message
            if (CulturalActivity == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveCulturalActivity with parameter the CulturalActivity model
            await Query.RemoveCulturalActivity(CulturalActivity);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddFavourite()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddFavouriteCulturalActivity with parameter the FavouriteCulturalActivity model
            await Query.AddFavouriteCulturalActivity(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteFavourite(int id)
        {
            // get favourite cultural activity's model from database based on id
            FavouriteCulturalActivity Favourite = await _db.FavouriteCulturalActivity.FindAsync(id);
            // if favourite cultural activity doesn't exist return a message
            if (Favourite == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveFavouriteCulturalActivity with parameter the FavouriteCulturalActivity model
            await Query.RemoveFavouriteCulturalActivity(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMessage()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddMessage with parameter the Message model
            await Query.AddMessage(Message);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }
    }
}
