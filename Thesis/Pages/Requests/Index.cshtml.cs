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

namespace Thesis.Pages.Requests
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
        public int? CurrentSalaryMin { get; set; }
        public int? CurrentSalaryMax { get; set; }
        public DateTime? CurrentDateStart { get; set; }
        public DateTime? CurrentDateEnd { get; set; }
        public string CurrentTimeRange { get; set; }
        public string CurrentTags { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<Request> Requests { get; set; }

        public List<SelectListItem> RequestTimeRange { get; private set; }

        public double minimumSalary, maximumSalary;

        public List<string> TagsList = new List<string>();

        public ICollection<Offer> ExistingOffer { get; set; }

        [BindProperty]
        public Offer Offer { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task OnGet(string searchStr, int? salaryMin, int? salaryMax, 
            DateTime? dateStart, DateTime? dateEnd, string timeRange, 
            string tags, string sortOrder, int? pageIndex)
        {

            CurrentSearch = searchStr;

            CurrentSalaryMin = salaryMin;

            CurrentSalaryMax = salaryMax;

            CurrentDateStart = dateStart;

            CurrentDateEnd = dateEnd;

            CurrentTimeRange = timeRange;

            CurrentTags = tags;

            CurrentSort = sortOrder;

            // get all requests including user model
            IQueryable<Request> requestsIQ = from s in _db.Request.Include(x => x.User) select s;

            if (requestsIQ.Count() > 0)
            {
                // get minimum salary of all requests
                minimumSalary = requestsIQ.OrderBy(x => x.Salary).First().Salary;
                // get maximum salary of all requests
                maximumSalary = requestsIQ.OrderByDescending(x => x.Salary).First().Salary;
            }

            if (!string.IsNullOrEmpty(searchStr))
            {
                // requests where request's title or description or tags contain search string 
                requestsIQ = requestsIQ.Where(x => x.Title.Contains(searchStr) ||
                x.Description.Contains(searchStr) || x.Tags.Contains(searchStr));
            }

            if (salaryMin != null)
            {
                // requests where requests' minimum salary is greater than or equal to filtered minimum salary
                requestsIQ = requestsIQ.Where(x => x.Salary >= salaryMin);
            }

            if (salaryMax != null)
            {
                // requests where requests' maximum's salary is less than or equal to filtered maximum salary
                requestsIQ = requestsIQ.Where(x => x.Salary <= salaryMax);
            }

            if (dateStart != null)
            {
                // requests where their dateStart is less than or equal to filtered dateStart
                requestsIQ = requestsIQ.Where(x => x.DateStart <=  dateStart);
            }

            if (dateEnd != null)
            {
                // requests where their dateEnd is greater than or equal to filtered dateEnd
                requestsIQ = requestsIQ.Where(x => x.DateEnd >= dateEnd);
            }

            // create new select list with time range list
            // with text the name and value of the time range
            RequestTimeRange = (from request in _db.Request
                                select new SelectListItem()
                                {
                                    Text = request.TimeRange,
                                    Value = request.TimeRange
                                }).Distinct().ToList();

            RequestTimeRange.Insert(0, new SelectListItem()
            {
                Text = "-",
                Value = string.Empty
            });

            if (!string.IsNullOrEmpty(timeRange))
            {
                // requests where requests' time range contain filtered time range
                requestsIQ = requestsIQ.Where(x => x.TimeRange.Contains(timeRange));
            }

            if (!string.IsNullOrEmpty(tags))
            {
                // requests where requests' tags contain filter tags
                requestsIQ = requestsIQ.Where(x => x.Tags.Contains(tags));
            }

            // get all requests and group them by their tags
            IEnumerable<IGrouping<string, Request>> RequestsTags = requestsIQ.ToList().GroupBy(x => x.Tags);
            List<string> requestsTags = new List<string>();

            // for every request
            foreach (var item in RequestsTags)
            {
                // get listings tags to a string list splitted by comma
                requestsTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in requestsTags)
                {
                    // if it doesn't exist in tags list
                    if (!TagsList.Contains(tag))
                    {
                        // add it to tags list
                        TagsList.Add(tag);
                    }
                }
            }

            // sort requests based on user's selection
            switch (sortOrder)
            {
                case "date_desc":
                    requestsIQ = requestsIQ.OrderByDescending(x => x.DateStart);
                    break;
                case "date_asc":
                    requestsIQ = requestsIQ.OrderBy(x => x.DateStart);
                    break;
                case "title_desc":
                    requestsIQ = requestsIQ.OrderByDescending(x => x.Title);
                    break;
                case "title_asc":
                    requestsIQ = requestsIQ.OrderBy(x => x.Title);
                    break;
                case "salary_desc":
                    requestsIQ = requestsIQ.OrderByDescending(x => x.Salary);
                    break;
                case "salary_asc":
                    requestsIQ = requestsIQ.OrderBy(x => x.Salary);
                    break;
                default:
                    requestsIQ = requestsIQ.OrderByDescending(x => x.DateStart);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the requests query to a single page
            // of requests in a collection type that supports paging.
            Requests = await PaginatedList<Request>.CreateAsync(
                requestsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            if (_signInManager.IsSignedIn(User))
            {
                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all offers of this user
                ExistingOffer = await Query.ExistingOffer(_userManager.GetUserId(User));
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get request's model from database based on id
            Request Request = await _db.Request.FindAsync(id);
            // if request doesn't exist return a message
            if (Request == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveRequest with parameter the Request model
            await Query.RemoveRequest(Request);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostOffer(int? id)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            if (id == null)
            {
                // call AddOffer with parameter the Offer model
                await Query.AddOffer(Offer);
            }
            else
            {
                // call EditOffer with parameter the Offer model
                await Query.EditOffer(Offer);
            }
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }
    }
}
