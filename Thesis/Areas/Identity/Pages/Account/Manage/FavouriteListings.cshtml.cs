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
    public class FavouriteListingsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public FavouriteListingsModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSort { get; set; }

        public PaginatedList<FavouriteListing> FavouriteListings { get; set; }

        private Query Query;

        [BindProperty]
        public Message Message { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task LoadAsync(User user, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            CurrentSort = sortOrder;

            // get all favourite visible listings of user including listing, category, expert and user models
            IQueryable<FavouriteListing> favouriteListingsIQ = from s in _db.FavouriteListing
                                  .Include(x => x.Listing).ThenInclude(x => x.ListingCategory)
                                    .Include(x => x.Listing).ThenInclude(x => x.Expert).ThenInclude(x => x.User)
                                .Where(x => x.IdUser == userId && x.Listing.Visibility == true) select s;

            // sort favourite listings based on user's selection
            switch (sortOrder)
            {
                case "rating_desc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderByDescending(x => x.Listing.AverageRating);
                    break;
                case "rating_asc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderBy(x => x.Listing.AverageRating);
                    break;
                case "date_desc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderByDescending(x => x.Listing.Date);
                    break;
                case "date_asc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderBy(x => x.Listing.Date);
                    break;
                case "name_desc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderByDescending(x => x.Listing.Expert.User.FirstName);
                    break;
                case "name_asc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderBy(x => x.Listing.Expert.User.FirstName);
                    break;
                case "title_desc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderByDescending(x => x.Listing.Title);
                    break;
                case "title_asc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderBy(x => x.Listing.Title);
                    break;
                case "rate_desc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderByDescending(x => x.Listing.HourlyRate);
                    break;
                case "rate_asc":
                    favouriteListingsIQ = favouriteListingsIQ.OrderBy(x => x.Listing.HourlyRate);
                    break;
                default:
                    favouriteListingsIQ = favouriteListingsIQ.OrderByDescending(x => x.Listing.AverageRating);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the favourite listing query to a single page
            // of favourite listings in a collection type that supports paging.
            FavouriteListings = await PaginatedList<FavouriteListing>.CreateAsync(
                favouriteListingsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

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

        public async Task<IActionResult> OnPostDeleteFavourite(int id)
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
