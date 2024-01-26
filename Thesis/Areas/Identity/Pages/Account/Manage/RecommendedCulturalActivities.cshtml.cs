using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class RecommendedCulturalActivitiesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public RecommendedCulturalActivitiesModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSort { get; set; }

        public User UserManager { get; set; }

        private Query Query;

        public PaginatedList<CulturalActivity> RecomendedCulturalActivities { get; set; }

        public ICollection<FavouriteCulturalActivity> ExistingFavourite { get; set; }

        [BindProperty]
        public FavouriteCulturalActivity Favourite { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task LoadAsync(User user, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // get user model based on id
            UserManager = await _db.User.FindAsync(userId);

            CurrentSort = sortOrder;

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);

            // get user's reccomended cultural activities collection 
            ICollection<CulturalActivity> RecomendedCulturalActivitiesCollection = await Query.RecommendedCulturalActivities(UserManager);

            // get user's favourite cultural activities collection 
            ExistingFavourite = await Query.ExistingFavouriteCulturalActivities(userId);

            // sort cultural activities based on user's selection
            switch (sortOrder)
            {
                case "rating_desc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderByDescending(x => x.AverageRating).ToList();
                    break;
                case "rating_asc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderBy(x => x.AverageRating).ToList();
                    break;
                case "dateStart_desc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderByDescending(x => x.DateStart).ToList();
                    break;
                case "dateStart_asc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderBy(x => x.DateStart).ToList();
                    break;
                case "dateEnd_desc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderByDescending(x => x.DateEnd).ToList();
                    break;
                case "dateEnd_asc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderBy(x => x.DateEnd).ToList();
                    break;
                case "title_desc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderByDescending(x => x.Title).ToList();
                    break;
                case "title_asc":
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderBy(x => x.Title).ToList();
                    break;
                default:
                    RecomendedCulturalActivitiesCollection = RecomendedCulturalActivitiesCollection.OrderByDescending(x => x.AverageRating).ToList();
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateICollection method converts the recomended cultural activities query
            // to a single page of recomended cultural activities in a collection type that supports paging.
            RecomendedCulturalActivities = PaginatedList<CulturalActivity>.CreateICollection(
                RecomendedCulturalActivitiesCollection, pageIndex ?? 1, pageSize);

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
            // call RemoveCulturalActivity with parameter the Cultural Activity model
            await Query.RemoveCulturalActivity(CulturalActivity);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAddFavourite()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddFavouriteCulturalActivity with parameter the Favourite model
            await Query.AddFavouriteCulturalActivity(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteFavourite(int id)
        {
            // get favourite cultural activity's model from database based on id
            // if favourite cultural activity doesn't exist return a message
            FavouriteCulturalActivity Favourite = await _db.FavouriteCulturalActivity.FindAsync(id);
            if (Favourite == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveFavouriteCulturalActivity with parameter the Favourite model
            await Query.RemoveFavouriteCulturalActivity(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }
    }
}
