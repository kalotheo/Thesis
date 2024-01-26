using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Listings
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public IndexModel(ApplicationDbContext db, 
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
        public string CurrentCategory { get; set; }
        public string CurrentTags { get; set; }
        public int? CurrentHourlyRateMin { get; set; }
        public int? CurrentHourlyRateMax { get; set; }
        public string CurrentAvailability { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<Listing> Listings { get; set; }

        public ListingCategory SelectedListingCategory { get; set; }

        public List<SelectListItem> ListingCategory { get; private set; }

        public List<SelectListItem> ListingAvailability { get; private set; }

        public double minimumHourlyRate, maximumHourlyRate;

        public List<string> TagsList = new List<string>();

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

        public async Task OnGet(string searchStr, string category, string tags, 
            int? hourlyRateMin, int? hourlyRateMax, string availability,
            string sortOrder, int? pageIndex)
        {

            CurrentSearch = searchStr;

            CurrentCategory = category;

            CurrentTags = tags;

            CurrentHourlyRateMin = hourlyRateMin;

            CurrentHourlyRateMax = hourlyRateMax;

            CurrentAvailability = availability;

            CurrentSort = sortOrder;

            // get all listings including listing category, expert and user models
            IQueryable<Listing> listingsIQ = from s in _db.Listing.Include(x => x.ListingCategory).Include(x => x.Expert).ThenInclude(x => x.User) select s;

            // if user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                // if logged-in user has created any listing
                if (listingsIQ.Any(x => x.ExpertId.Equals(_userManager.GetUserId(User))))
                {
                    // display all his listings (visible or invisible)
                    listingsIQ = listingsIQ.Where(x => x.ExpertId.Equals(_userManager.GetUserId(User)) && x.Visibility == false || x.Visibility == true);
                }
                
                // else display only visible listings
                else
                {
                    listingsIQ = listingsIQ.Where(x => x.Visibility == true);
                }

                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all favourite listings of this user
                ExistingFavourite = await Query.ExistingFavouriteListings(_userManager.GetUserId(User));
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }

            // if user is not logged in, display only visible listings
            else
            {
                listingsIQ = listingsIQ.Where(x => x.Visibility == true);
            }

            if (listingsIQ.Count() > 0)
            {
                // get minimum hourly rate of all listings
                minimumHourlyRate = listingsIQ.OrderBy(x => x.HourlyRate).First().HourlyRate;
                // get maximum hourly rate of all listings
                maximumHourlyRate = listingsIQ.OrderByDescending(x => x.HourlyRate).First().HourlyRate;
            }

            if (!string.IsNullOrEmpty(searchStr))
            {
                // listings where listing's title or description or tags contain search string 
                listingsIQ = listingsIQ.Where(x => x.Title.Contains(searchStr) || 
                x.Description.Contains(searchStr) || x.Tags.Contains(searchStr));
            }

            // create a new select list with all the categories
            // with text and value the name and value of the category
            ListingCategory = (from listingCategory in _db.ListingCategory
                               select new SelectListItem()
                               {
                                   Text = listingCategory.CategoryName,
                                   Value = listingCategory.CategoryName
                               }).Distinct().ToList();

            if (!string.IsNullOrEmpty(category))
            {
                // get selected category's model from filter
                SelectedListingCategory = await _db.ListingCategory.SingleOrDefaultAsync(x => x.CategoryName == category);
                // listings where category id equals to selected listing category id
                listingsIQ = listingsIQ.Where(x => x.CategoryId == SelectedListingCategory.Id);
            }

            if (!string.IsNullOrEmpty(tags))
            {
                // listings where listings' tags contain filter tags
                listingsIQ = listingsIQ.Where(x => x.Tags.Contains(tags));
            }

            if (hourlyRateMin != null)
            {
                // listings where listings' minimum hourly rate is greater than
                // or equal to filtered minimum hourly rate
                listingsIQ = listingsIQ.Where(x => x.HourlyRate >= hourlyRateMin);
            }

            if (hourlyRateMax != null)
            {
                // listings where listings' maximum's hourly rate is less than
                // or equal to filtered maximum hourly rate
                listingsIQ = listingsIQ.Where(x => x.HourlyRate <= hourlyRateMax);
            }

            // create new select list with availability options for listings
            ListingAvailability = (from listing in _db.Listing
                                   select new SelectListItem()
                                   {
                                       Text = listing.Availability,
                                       Value = listing.Availability
                                   }).Distinct().ToList();


            ListingAvailability.Insert(0, new SelectListItem()
            {
                Text = "-",
                Value = string.Empty
            });

            if (!string.IsNullOrEmpty(availability))
            {
                // listings where listings' availability contain filtered availability
                listingsIQ = listingsIQ.Where(x => x.Availability.Contains(availability));
            }

            // get all listings and group them by their tags
            IEnumerable<IGrouping<string, Listing>> ListingsTags = listingsIQ.ToList().GroupBy(x => x.Tags);
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
                    if (!TagsList.Contains(tag))
                    {
                        // add it to tags list
                        TagsList.Add(tag);
                    }
                }
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
                case "name_desc":
                    listingsIQ = listingsIQ.OrderByDescending(x => x.Expert.User.FirstName);
                    break;
                case "name_asc":
                    listingsIQ = listingsIQ.OrderBy(x => x.Expert.User.FirstName);
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

        public async Task<IActionResult> OnPostAddFavourite()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddFavouriteListing with parameter the FavouriteListing model
            await Query.AddFavouriteListing(Favourite);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
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
