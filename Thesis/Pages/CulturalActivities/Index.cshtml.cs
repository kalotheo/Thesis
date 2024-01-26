using System;
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

namespace Thesis.Pages.CulturalActivities
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
        public string CurrentSubcategory { get; set; }
        public DateTime? CurrentDateStart { get; set; }
        public DateTime? CurrentDateEnd { get; set; }
        public string CurrentTags { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<CulturalActivity> CulturalActivities { get; set; }

        public List<SelectListItem> CulturalActivityCategory { get; private set; }

        public List<SelectListItem> CulturalActivitySubcategory { get; private set; }

        public CulturalActivityCategory SelectedCulturalActivityCategory { get; set; }

        private CulturalActivityCategory SelectedCulturalActivitySubcategory { get; set; }

        public List<string> TagsList = new List<string>();

        public ICollection<FavouriteCulturalActivity> ExistingFavourite { get; set; }

        [BindProperty]
        public FavouriteCulturalActivity Favourite { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task OnGet(string searchStr, string category, string subcategory,
            string tags, DateTime? dateStart, DateTime? dateEnd,
            string sortOrder, int? pageIndex)
        {

            CurrentSearch = searchStr;

            CurrentCategory = category;

            CurrentSubcategory = subcategory;

            CurrentTags = tags;

            CurrentDateStart = dateStart;

            CurrentDateEnd = dateEnd;

            CurrentSort = sortOrder;

            // get all cultural activities including main, subcategory and reviews models
            IQueryable<CulturalActivity> culturalActivitiesIQ = from s in _db.CulturalActivity
                                   .Include(x => x.CulturalActivityMainCategory)
                                   .Include(x => x.CulturalActivitySubcategory)
                                   .Include(x => x.CulturalActivityReviews) select s;

            if (!string.IsNullOrEmpty(searchStr))
            {
                // cultural activities where cultural activity's title or description
                // or tags contain search string 
                culturalActivitiesIQ = culturalActivitiesIQ.Where(x => x.Title.Contains(searchStr) ||
                x.Description.Contains(searchStr) || x.Tags.Contains(searchStr));
            }

            // create a new select list with all the categories, that don't have a parent,
            // with text and value the name and value of the category
            CulturalActivityCategory = (from CulturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == null)
                                        select new SelectListItem()
                                        {
                                            Text = CulturalActivityCategory.CategoryName,
                                            Value = CulturalActivityCategory.CategoryName
                                        }).Distinct().ToList();

            if (!string.IsNullOrEmpty(category))
            {
                // get selected category's model from filter
                SelectedCulturalActivityCategory = await _db.CulturalActivityCategory.SingleOrDefaultAsync(x => x.CategoryName == category);

                // cultural activities where category id equals to selected cultural activity category id
                culturalActivitiesIQ = culturalActivitiesIQ.Where(x => x.CategoryId == SelectedCulturalActivityCategory.Id);

                // create a new select list with all the categories, with parent the selected category
                // and with text and value the name and value of the category
                CulturalActivitySubcategory = (from CulturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == SelectedCulturalActivityCategory.Id)
                                               select new SelectListItem()
                                               {
                                                   Text = CulturalActivityCategory.CategoryName,
                                                   Value = CulturalActivityCategory.CategoryName
                                               }).Distinct().ToList();
            }

            if (!string.IsNullOrEmpty(subcategory))
            {
                // get selected subcategory's model from filter
                SelectedCulturalActivitySubcategory = await _db.CulturalActivityCategory.SingleOrDefaultAsync(x => x.CategoryName == subcategory);
                // cultural activities where category id equal to selected cultural activity subcategory id
                culturalActivitiesIQ = culturalActivitiesIQ.Where(x => x.SubcategoryId == SelectedCulturalActivitySubcategory.Id);
            }

            if (!string.IsNullOrEmpty(tags))
            {
                // cultural activities where their tags contain filtered tags
                culturalActivitiesIQ = culturalActivitiesIQ.Where(x => x.Tags.Contains(tags));
            }

            if (dateStart != null)
            {
                // cultural activities where their dateStart is less than or equal to filtered dateStart
                culturalActivitiesIQ = culturalActivitiesIQ.Where(x => x.DateStart <= dateStart);
            }

            if (dateEnd != null)
            {
                // cultural activities where their dateEnd is greater than or equal to filtered dateStart
                culturalActivitiesIQ = culturalActivitiesIQ.Where(x => x.DateEnd >= dateEnd);
            }

            // get all cultural activities and group them by their tags
            IEnumerable<IGrouping<string, CulturalActivity>> CulturalActivitiesTags = culturalActivitiesIQ.ToList().GroupBy(x => x.Tags);
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
                    if (!TagsList.Contains(tag))
                    {
                        // add it to tags list
                        TagsList.Add(tag);
                    }
                }
            }

            // sort cultural activities based on user's selection
            switch (sortOrder)
            {
                case "rating_desc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderByDescending(x => x.AverageRating);
                    break;
                case "rating_asc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderBy(x => x.AverageRating);
                    break;
                case "dateStart_desc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderByDescending(x => x.DateStart);
                    break;
                case "dateStart_asc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderBy(x => x.DateStart);
                    break;
                case "dateEnd_desc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderByDescending(x => x.DateEnd);
                    break;
                case "dateEnd_asc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderBy(x => x.DateEnd);
                    break;
                case "title_desc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderByDescending(x => x.Title);
                    break;
                case "title_asc":
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderBy(x => x.Title);
                    break;
                default:
                    culturalActivitiesIQ = culturalActivitiesIQ.OrderByDescending(x => x.AverageRating);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the cultural activities query to a single page
            // of cultural activities in a collection type that supports paging.
            CulturalActivities = await PaginatedList<CulturalActivity>.CreateAsync(
                culturalActivitiesIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            // if a user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all favourite cultural activities of this user
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

    }
}
