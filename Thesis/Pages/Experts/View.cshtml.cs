using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Experts
{
    public class ViewModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ViewModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string CurrentSearch { get; set; }
        public string CurrentSort { get; set; }

        public Expert Expert { get; set; }

        public PaginatedList<Listing> ExpertsListings { get; set; }

        public IEnumerable<Expert> Experts { get; private set; }

        public List<string> tags { get; set; }

        private List<Expert> ExpertsTags = new List<Expert>();

        private Query Query;

        public ICollection<FavouriteListing> ExistingFavourite { get; set; }

        [BindProperty]
        public FavouriteListing Favourite { get; set; }

        [BindProperty]
        public Message Message { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet(string id, string searchStr, 
            string sortOrder, int? pageIndex)
        {
            // get expert's model from database including user model based on id
            Expert = await _db.Expert.Include(x => x.User).SingleOrDefaultAsync(x => x.User.UserName == id);

            // if expert is null return a message
            if (Expert == null)
            {
                return NotFound($"Unable to load expert with username '{id}'.");
            }

            CurrentSearch = searchStr;

            CurrentSort = sortOrder;

            // get all expert's listings including listing category, expert and user models
            IQueryable<Listing> listingsIQ = from s in _db.Listing
                         .Include(x => x.ListingCategory)
                         .Include(x => x.Expert)
                         .ThenInclude(x => x.User)
                         .Where(x => x.Expert.User.UserName == id) select s;

            if (!string.IsNullOrEmpty(searchStr))
            {
                // listings where listing's title or description or tags contain search string 
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

            // get a list of all experts excluding expert from list including user 
            IEnumerable<Expert> AllExperts = await _db.Expert
                .Include(x => x.User)
                .Where(x => x.User.UserName != id).ToListAsync();

            // get expert's tags to a string list splitted by comma
            tags = Expert.Tags.Split(',').ToList();
            List<string> allTags = new List<string>();

            // for every expert
            foreach (var expert in AllExperts)
            {
                // get expert tags to a string list splitted by comma
                allTags = expert.Tags.Split(',').ToList();
                // if allTags list and tags list have common tags
                if (allTags.Intersect(tags).Any())
                {
                    // add it to ExpertsTags list
                    ExpertsTags.Add(expert);
                }
            }

            // Equalize Experts list with ExpertsTags list and take the first 12 items ordered by their average rating
            Experts = ExpertsTags.Take(12).OrderByDescending(x => x.AverageRating);

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the expert's listings query to a single page
            // of listings in a collection type that supports paging.
            ExpertsListings = await PaginatedList<Listing>.CreateAsync(
                listingsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            // if a user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all favourite listings of this user
                ExistingFavourite = await Query.ExistingFavouriteListings(_userManager.GetUserId(User));
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddFavourite(string id)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddFavouriteListing with parameter the FavouriteListing model
            await Query.AddFavouriteListing(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = id });
        }

        public async Task<IActionResult> OnPostDeleteFavourite(int id, string id2)
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

        public async Task<IActionResult> OnPostMessage(string redirectPageId)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddMessage with parameter the Message model
            await Query.AddMessage(Message);
            StatusMessage = Query.StatusMessage;            
            return RedirectToPage("View", new { id = redirectPageId });
        }

        public async Task<IActionResult> OnPostChangeVisibility(int id)
        {
            // get listing's model from database including expert, user models based on id
            Listing Listing = await _db.Listing.Include(x => x.Expert).ThenInclude(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
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
            return RedirectToPage("View", new { id = Listing.Expert.User.UserName });
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get listing's model from database including expert, user models based on id
            Listing Listing = await _db.Listing.Include(x => x.Expert).ThenInclude(x => x.User).SingleOrDefaultAsync(x => x.Id == id);
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
            return RedirectToPage("View", new { id = Listing.Expert.User.UserName });
        }
    }
}
