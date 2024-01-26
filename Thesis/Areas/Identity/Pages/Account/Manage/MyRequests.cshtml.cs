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
    public class MyRequestsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public MyRequestsModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSearch { get; set; }

        public string CurrentSort { get; set; }

        public PaginatedList<Request> Requests { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        private async Task LoadAsync(User user, string searchStr, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            CurrentSearch = searchStr;

            CurrentSort = sortOrder;

            // get all requests of user including user model
            IQueryable<Request> requestsIQ = from s in _db.Request
                                             .Include(x => x.User)
                                             .Where(x => x.UserId == userId) select s;

            if (!string.IsNullOrEmpty(searchStr))
            {
                // requests where request title or description contains search string 
                requestsIQ = requestsIQ.Where(x => x.Title.Contains(searchStr) ||
                x.Description.Contains(searchStr));
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

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGetAsync(string searchStr, string sortOrder, int? pageIndex)
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // call LoadAsync
            await LoadAsync(user, searchStr, sortOrder, pageIndex);
            return Page();
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
    }
}
