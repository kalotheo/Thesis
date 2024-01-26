using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Expert")]
    public class MyListingsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public MyListingsModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSearch { get; set; }

        public string CurrentSort { get; set; }

        public PaginatedList<Listing> Listings { get; set; }

        private Query Query;

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        private async Task LoadAsync(User user, string searchStr, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            CurrentSearch = searchStr;

            CurrentSort = sortOrder;

            // get all listings of user including expert, user, listing category models
            IQueryable<Listing> listingsIQ = from s in _db.Listing
                                             .Include(x => x.Expert)
                                             .ThenInclude(x => x.User)
                                             .Include(x => x.ListingCategory)
                                             .Where(x => x.ExpertId == userId) select s;

            if (!string.IsNullOrEmpty(searchStr))
            {
                // listings where listing title or description or tags contain search string 
                listingsIQ = listingsIQ.Where(x => x.Title.Contains(searchStr) ||
                x.Description.Contains(searchStr) || x.Tags.Contains(searchStr));
            }

            // sort listings based on user's selection
            switch (sortOrder)
            {
                case "rating_desc":
                    listingsIQ = listingsIQ.OrderByDescending(x => x.AverageRating);
                    break;
                case "rating_asc":
                    listingsIQ = listingsIQ.OrderBy(x => x.AverageRating);
                    break;
                case "date_desc":
                    listingsIQ = listingsIQ.OrderByDescending(x => x.Date);
                    break;
                case "date_asc":
                    listingsIQ = listingsIQ.OrderBy(x => x.Date);
                    break;
                case "title_desc":
                    listingsIQ = listingsIQ.OrderByDescending(x => x.Title);
                    break;
                case "title_asc":
                    listingsIQ = listingsIQ.OrderBy(x => x.Title);
                    break;
                case "rate_desc":
                    listingsIQ = listingsIQ.OrderByDescending(x => x.HourlyRate);
                    break;
                case "rate_asc":
                    listingsIQ = listingsIQ.OrderBy(x => x.HourlyRate);
                    break;
                default:
                    listingsIQ = listingsIQ.OrderByDescending(x => x.AverageRating);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the listings query to a single page
            // of listings in a collection type that supports paging.
            Listings = await PaginatedList<Listing>.CreateAsync(
                listingsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGetAsync(string searchStr, string sortOrder, int? pageIndex)
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // find expert based on user id
            Expert Expert = await _db.Expert.FindAsync(userId);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }
            // if expert doesn't exist return a message
            if (Expert == null)
            {
                return NotFound($"Unable to load expert with ID '{userId}'.");
            }
            // call LoadAsync
            await LoadAsync(user, searchStr, sortOrder, pageIndex);
            return Page();
        }

        public async Task<IActionResult> OnPostChangeVisibility(int id)
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
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
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
            // call RemoveListing with parameter the Listing model
            await Query.RemoveListing(Listing);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }
    }
}
