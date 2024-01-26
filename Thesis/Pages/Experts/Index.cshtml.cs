using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Experts
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
        public int? CurrentHourlyRateMin { get; set; }
        public int? CurrentHourlyRateMax { get; set; }
        public string CurrentAvailability { get; set; }
        public string CurrentExperience { get; set; }
        public string CurrentTags { get; set; }
        public string CurrentSort { get; set; }

        public PaginatedList<Expert> Experts { get; set; }

        public List<SelectListItem> ExpertAvailability { get; private set; }

        public List<SelectListItem> ExpertExperience{ get; private set; }

        public double minimumHourlyRate, maximumHourlyRate;

        public List<string> TagsList = new List<string>();

        private Query Query;

        [BindProperty]
        public Message Message { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task OnGet(string searchStr, string tags, int? hourlyRateMin, int? hourlyRateMax, 
            string availability, string experience, string sortOrder, int? pageIndex)
        {

            CurrentSearch = searchStr;

            CurrentTags = tags;

            CurrentHourlyRateMin = hourlyRateMin;

            CurrentHourlyRateMax = hourlyRateMax;

            CurrentAvailability = availability;

            CurrentExperience = experience;
            
            CurrentSort = sortOrder;

            // get all experts including user model
            IQueryable<Expert> expertsIQ = from s in _db.Expert.Include(x => x.User) select s;

            if (expertsIQ.Count() > 0)
            {
                // get minimum hourly rate of all experts
                minimumHourlyRate = expertsIQ.OrderBy(x => x.HourlyRate).First().HourlyRate;
                // get maximum hourly rate of all experts
                maximumHourlyRate = expertsIQ.OrderByDescending(x => x.HourlyRate).First().HourlyRate;
            }

            if (!string.IsNullOrEmpty(searchStr))
            {
                // experts where expert's first name or last name contain search string 
                expertsIQ = expertsIQ.Where(x => x.User.LastName.Contains(searchStr) 
                || x.User.FirstName.Contains(searchStr) || x.Tags.Contains(searchStr));
            }

            if (!string.IsNullOrEmpty(tags))
            {
                // listings where listings' tags contain filter tags
                expertsIQ = expertsIQ.Where(x => x.Tags.Contains(tags));
            }

            if (hourlyRateMin != null)
            {
                // experts where expert's minimum hourly rate is greater than
                // or equal to filtered minimum hourly rate
                expertsIQ = expertsIQ.Where(x => x.HourlyRate >= hourlyRateMin);
            }

            if (hourlyRateMax != null)
            {
                // experts where expert's maximum hourly rate is less than
                // or equal to filtered maximum hourly rate
                expertsIQ = expertsIQ.Where(x => x.HourlyRate <= hourlyRateMax);
            }

            // create new select list with experience options for experts
            ExpertAvailability = (from expert in _db.Expert
                                   select new SelectListItem()
                                   {
                                       Text = expert.Availability,
                                       Value = expert.Availability
                                   }).Distinct().ToList();

            ExpertAvailability.Insert(0, new SelectListItem()
            {
                Text = "-",
                Value = string.Empty
            });

            if (!string.IsNullOrEmpty(availability))
            {
                // experts where expert's availability contain filter's availability
                expertsIQ = expertsIQ.Where(x => x.Availability.Contains(availability));
            }

            // create new select list with experience options for experts
            ExpertExperience = (from expert in _db.Expert
                                select new SelectListItem()
                                  {
                                      Text = expert.Experience,
                                      Value = expert.Experience
                                  }).Distinct().ToList();

            ExpertExperience.Insert(0, new SelectListItem()
            {
                Text = "-",
                Value = string.Empty
            });

            if (!string.IsNullOrEmpty(experience))
            {
                // experts where expert's experience contain filter's availability
                expertsIQ = expertsIQ.Where(x => x.Experience.Contains(experience));
            }

            // get all listings and group them by their tags
            IEnumerable<IGrouping<string, Expert>> ExpertsTags = expertsIQ.ToList().GroupBy(x => x.Tags);
            List<string> expertsTags = new List<string>();

            // for every listing
            foreach (var item in ExpertsTags)
            {
                // get listings tags to a string list splitted by comma
                expertsTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in expertsTags)
                {
                    // if it doesn't exist in tags list
                    if (!TagsList.Contains(tag))
                    {
                        // add it to tags list
                        TagsList.Add(tag);
                    }
                }
            }

            // sort experts based on user's selection
            switch (sortOrder)
            {
                case "rating_desc":
                    expertsIQ = expertsIQ.OrderByDescending(x => x.AverageRating);
                    break;
                case "rating_asc":
                    expertsIQ = expertsIQ.OrderBy(x => x.AverageRating);
                    break;
                case "date_desc":
                    expertsIQ = expertsIQ.OrderByDescending(x => x.User.RegistrationDate);
                    break;
                case "date_asc":
                    expertsIQ = expertsIQ.OrderBy(x => x.User.RegistrationDate);
                    break;
                case "name_desc":
                    expertsIQ = expertsIQ.OrderByDescending(x => x.User.FirstName);
                    break;
                case "name_asc":
                    expertsIQ = expertsIQ.OrderBy(x => x.User.FirstName);
                    break;
                case "rate_desc":
                    expertsIQ = expertsIQ.OrderByDescending(x => x.HourlyRate);
                    break;
                case "rate_asc":
                    expertsIQ = expertsIQ.OrderBy(x => x.HourlyRate);
                    break;
                default:
                    expertsIQ = expertsIQ.OrderByDescending(x => x.AverageRating);
                    break;
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the experts query to a single page
            // of experts in a collection type that supports paging.
            Experts = await PaginatedList<Expert>.CreateAsync(
                expertsIQ.AsNoTracking(), pageIndex ?? 1, pageSize);

            // if a user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }

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
