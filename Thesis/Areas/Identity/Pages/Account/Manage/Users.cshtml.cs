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
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public UsersModel(ApplicationDbContext db,
            IConfiguration configuration,
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public string CurrentSearch { get; set; }

        public string CurrentSort { get; set; }

        public PaginatedList<User> Users { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task LoadAsync(User user, string searchStr, string sortOrder, int? pageIndex)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            CurrentSearch = searchStr;

            CurrentSort = sortOrder;

            // get all users
            IQueryable<User> usersIQ = from s in _db.User select s;

            if (!string.IsNullOrEmpty(searchStr))
            {
                // users where listing first name or last name or username or email contain search string 
                usersIQ = usersIQ.Where(x => x.FirstName.Contains(searchStr) 
                || x.LastName.Contains(searchStr) || x.UserName.Contains(searchStr) 
                || x.Email.Contains(searchStr));
            }

            // sort users based on user's selection
            switch (sortOrder)
            {
                case "username_desc":
                    usersIQ = usersIQ.OrderByDescending(x => x.UserName);
                    break;
                case "username_asc":
                    usersIQ = usersIQ.OrderBy(x => x.UserName);
                    break;
                case "firstName_desc":
                    usersIQ = usersIQ.OrderByDescending(x => x.FirstName);
                    break;
                case "firstName_asc":
                    usersIQ = usersIQ.OrderBy(x => x.FirstName);
                    break;
                case "lastName_desc":
                    usersIQ = usersIQ.OrderByDescending(x => x.LastName);
                    break;
                case "lastName_asc":
                    usersIQ = usersIQ.OrderBy(x => x.LastName);
                    break;
                default:
                    usersIQ = usersIQ.OrderByDescending(x => x.UserName);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the users query to a single page
            // of users in a collection type that supports paging.
            Users = await PaginatedList<User>.CreateAsync(
                usersIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGet(string searchStr, string sortOrder, int? pageIndex)
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
    }
}
