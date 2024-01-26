using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.CulturalActivities
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

        public CulturalActivity CulturalActivity { get; set; }

        public List<string> tags { get; set; }

        private List<CulturalActivity> CulturalActivitiesTags = new List<CulturalActivity>();

        public IEnumerable<CulturalActivity> CulturalActivities { get; set; }

        public ICollection<FavouriteCulturalActivity> ExistingFavourite { get; set; }

        public ICollection<ReviewCulturalActivity> ExistingReview { get; set; }

        [BindProperty]
        public ReviewCulturalActivity Review { get; set; }

        [BindProperty]
        public FavouriteCulturalActivity Favourite { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            // get cultural activity's model from database
            // including main, sub category, reviews and user models based on id
            CulturalActivity = await _db.CulturalActivity
                .Include(x => x.CulturalActivityMainCategory)
                .Include(x => x.CulturalActivitySubcategory)
                .Include(x => x.CulturalActivityReviews)
                .ThenInclude(x => x.User)
                .SingleOrDefaultAsync(x => x.Id == id);

            // if cultural activity is null return a message
            if (CulturalActivity == null)
            {
                return NotFound($"Unable to load cultural activity with id '{id}'.");
            }

            // get a list of all cultural activities, excluding current cultural activity from list,
            // including main and sub category
            IEnumerable<CulturalActivity> AllCulturalActivities = await _db.CulturalActivity
                .Include(x => x.CulturalActivityMainCategory)
                .Include(x => x.CulturalActivitySubcategory)
                .Where(x => x.Id != CulturalActivity.Id).ToListAsync();

            // list that has all cultural activities with the same category as the current cultural activity
            IEnumerable<CulturalActivity> CulturalActivityCategory = AllCulturalActivities.Where(x => x.SubcategoryId == CulturalActivity.SubcategoryId);

            // get cultural activity tags to a string list splitted by comma
            tags = CulturalActivity.Tags.Split(',').ToList();
            List<string> allTags = new List<string>();

            // for every cultural activity
            foreach (var culturalActivity in AllCulturalActivities)
            {
                // get cultural activity tags to a string list splitted by comma
                allTags = culturalActivity.Tags.Split(',').ToList();
                // if allTags list and tags list have common tags
                if (allTags.Intersect(tags).Any())
                {
                    // if cultural activity doesn't exist in CulturalActivityCategory
                    if (!CulturalActivityCategory.Contains(culturalActivity))
                    {
                        // add it to CulturalActivitiesTags list
                        CulturalActivitiesTags.Add(culturalActivity);
                    }
                }
            }

            // concatenate CulturalActivityCategory list with CulturalActivitiesTags and take the first 12 items ordered by their average rating
            CulturalActivities = CulturalActivityCategory.Concat(CulturalActivitiesTags).Take(12).OrderByDescending(x => x.AverageRating);

            // if a user is logged in
            if (_signInManager.IsSignedIn(User)){
                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all favourite cultural activities of this user
                ExistingFavourite = await Query.ExistingFavouriteCulturalActivities(_userManager.GetUserId(User));
                // get review  of this cultural activity of this user
                ExistingReview = await Query.ExistingReviewCulturalActivity(_userManager.GetUserId(User), id);
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDelete(int id, int redirectPageId)
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
            // if user is deleting this cultural activity 
            if (id == redirectPageId)
            {
                // redirect him to index
                return RedirectToPage("Index");
            }
            else
            {
                // redirect him to cultural activity's details
                return RedirectToPage("View", new { id = redirectPageId });
            }
        }

        public async Task<IActionResult> OnPostAddFavourite(int id)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddFavouriteCulturalActivity with parameter the FavouriteCulturalActivity model
            await Query.AddFavouriteCulturalActivity(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteFavourite(int id, int id2)
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
            return RedirectToPage("View", new { id = id2 });
        }

        public async Task<IActionResult> OnPostReview()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddReviewCulturalActivity with parameter the ReviewCulturalActivity model
            await Query.AddReviewCulturalActivity(Review);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = Review.CulturalActivityId });
        }

        public async Task<IActionResult> OnPostEditReview()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call EditReviewCulturalActivity with parameter the ReviewCulturalActivity model
            await Query.EditReviewCulturalActivity(Review);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = Review.CulturalActivityId });
        }

        public async Task<IActionResult> OnPostDeleteReview(int id)
        {
            // get review cultural activity's model from database based on id
            ReviewCulturalActivity ReviewCulturalActivity = await _db.ReviewCulturalActivity.FindAsync(id);
            // if review cultural activity doesn't exist return a message
            if (ReviewCulturalActivity == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveReviewCulturalActivity with parameter the ReviewCulturalActivity model
            await Query.RemoveReviewCulturalActivity(ReviewCulturalActivity);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = ReviewCulturalActivity.CulturalActivityId });
        }
    }
}
