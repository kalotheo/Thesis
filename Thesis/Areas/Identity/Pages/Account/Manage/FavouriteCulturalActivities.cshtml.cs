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
    public class FavouriteCulturalActivitiesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public FavouriteCulturalActivitiesModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSort { get; set; }

        public PaginatedList<FavouriteCulturalActivity> FavouriteCulturalActivities { get; set; }

        private Query Query;

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task LoadAsync(User user, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            CurrentSort = sortOrder;

            // get all favourite cultural activities of user including cultural activity and main and subcategory models
            IQueryable<FavouriteCulturalActivity> favouriteCulturalActivitiesIQ = from s in _db.FavouriteCulturalActivity
                                            .Include(x => x.CulturalActivity).ThenInclude(x => x.CulturalActivityMainCategory)
                                            .Include(x => x.CulturalActivity).ThenInclude(x => x.CulturalActivitySubcategory)
                                            .Where(x => x.IdUser == userId) select s;

            // sort favourite cultural activities based on user's selection
            switch (sortOrder)
            {
                case "rating_desc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderByDescending(x => x.CulturalActivity.AverageRating);
                    break;
                case "rating_asc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderBy(x => x.CulturalActivity.AverageRating);
                    break;
                case "dateStart_desc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderByDescending(x => x.CulturalActivity.DateStart);
                    break;
                case "dateStart_asc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderBy(x => x.CulturalActivity.DateStart);
                    break;
                case "dateEnd_desc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderByDescending(x => x.CulturalActivity.DateEnd);
                    break;
                case "dateEnd_asc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderBy(x => x.CulturalActivity.DateEnd);
                    break;
                case "title_desc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderByDescending(x => x.CulturalActivity.Title);
                    break;
                case "title_asc":
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderBy(x => x.CulturalActivity.Title);
                    break;
                default:
                    favouriteCulturalActivitiesIQ = favouriteCulturalActivitiesIQ.OrderByDescending(x => x.CulturalActivity.AverageRating);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the favourite cultural activities query to a single page
            // of favourite cultural activities in a collection type that supports paging.
            FavouriteCulturalActivities = await PaginatedList<FavouriteCulturalActivity>.CreateAsync(
                favouriteCulturalActivitiesIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

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
    }
}
