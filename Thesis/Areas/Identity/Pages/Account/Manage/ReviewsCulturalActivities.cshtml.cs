using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class ReviewsCulturalActivitiesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public ReviewsCulturalActivitiesModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSort { get; set; }

        public PaginatedList<ReviewCulturalActivity> Reviews { get; set; }

        [BindProperty]
        public ReviewCulturalActivity Review { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        private async Task LoadAsync(User user, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            CurrentSort = sortOrder;
            // get user's reviews of cultural activities including cultural activity model
            IQueryable<ReviewCulturalActivity> reviewsIQ = from s in _db.ReviewCulturalActivity
                                                           .Include(x => x.CulturalActivity)
                                                           .Where(x => x.IdReviewer == userId) select s;
            // sort reviews based on user's selection
            switch (sortOrder)
            {
                case "date_desc":
                    reviewsIQ = reviewsIQ.OrderByDescending(x => x.ReviewDate);
                    break;
                case "date_asc":
                    reviewsIQ = reviewsIQ.OrderBy(x => x.ReviewDate);
                    break;
                case "rating_desc":
                    reviewsIQ = reviewsIQ.OrderByDescending(x => x.Rating);
                    break;
                case "rating_asc":
                    reviewsIQ = reviewsIQ.OrderBy(x => x.Rating);
                    break;
                default:
                    reviewsIQ = reviewsIQ.OrderByDescending(x => x.ReviewDate);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the reviews query to a single page
            // of reviews in a collection type that supports paging.
            Reviews = await PaginatedList<ReviewCulturalActivity>.CreateAsync(
            reviewsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGetAsync(string sortOrder, int? pageIndex)
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // call LoadAsync
            await LoadAsync(user, sortOrder, pageIndex);
            return Page();
        }

        public async Task<IActionResult> OnPostEditReview()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call EditReviewCulturalActivity with parameter the Review Cultural Activity model
            await Query.EditReviewCulturalActivity(Review);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteReview(int id)
        {
            // get review cultural activity's model from database based on id
            ReviewCulturalActivity ReviewCulturalActivity = await _db.ReviewCulturalActivity.FindAsync(id);
            // if ReviewCulturalActivity doesn't exist return a message
            if (ReviewCulturalActivity == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveReviewCulturalActivity with parameter the Review Cultural Activity model
            await Query.RemoveReviewCulturalActivity(ReviewCulturalActivity);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

    }
}
