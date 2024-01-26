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

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class OffersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration Configuration;
        private readonly UserManager<User> _userManager;

        public OffersModel(ApplicationDbContext db,
            IConfiguration configuration, 
            UserManager<User> userManager)
        {
            _db = db;
            Configuration = configuration;
            _userManager = userManager;
        }

        public PaginatedList<Offer> OffersForExpert { get; set; }

        public PaginatedList<Offer> OffersByExpert { get; set; }

        [BindProperty]
        public Offer Offer { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        private async Task LoadAsync(User user, int? pageIndexForExpert, int? pageIndexByExpert)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            // get list of user's requests including user, request offers, expert models 
            ICollection<Request> Requests = await _db.Request.Include(x => x.User).Include(x => x.RequestOffers).ThenInclude(x => x.Expert).ThenInclude(x => x.User).Where(x => x.UserId == userId).ToListAsync();

            ICollection<Offer> OffersForExpertCollection = new List<Offer>();

            // for every request
            foreach (var request in Requests)
            {
                // for every offer ordered by date
                foreach (var item in request.RequestOffers.OrderByDescending(x => x.OfferDate))
                {
                    // if OffersForExpert list doesn't contain item
                    if (!OffersForExpertCollection.Contains(item))
                    {
                        // add item to list
                        OffersForExpertCollection.Add(item);
                    }
                }
            }

            // get page size value from the configuration or set it at 10 
            int pageSize = Configuration.GetValue("PageSize", 10);
            // the PaginatedList.CreateAsync method converts the offers for expert query to a single page
            // of offers in a collection type that supports paging.
            OffersForExpert = PaginatedList<Offer>.CreateICollection(
                OffersForExpertCollection, pageIndexForExpert ?? 1, pageSize);

            // if user has role of expert
            if (User.IsInRole("Expert"))
            {
                // get list of user's offers including expert, user, request models ordered by offer date
                IQueryable<Offer> offersByExpertIQ = from s in _db.Offer
                                                     .Include(x => x.Expert)
                                                     .ThenInclude(x => x.User)
                                                     .Include(x => x.Request)
                                                     .Where(x => x.ExpertId == userId)
                                                     .OrderByDescending(x => x.OfferDate) select s;

                // the PaginatedList.CreateAsync method converts the offers by expert query to a single page
                // of offers in a collection type that supports paging.
                OffersByExpert = await PaginatedList<Offer>.CreateAsync(
                    offersByExpertIQ.AsNoTracking(), pageIndexByExpert ?? 1, pageSize);
            }

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);

        }

        public async Task<IActionResult> OnGetAsync(int? pageIndexForExpert, int? pageIndexByExpert)
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserIdAsync(user)}'.");
            }
            // call LoadAsync
            await LoadAsync(user, pageIndexForExpert, pageIndexByExpert);
            return Page();
        }

        public async Task<IActionResult> OnPostEditOffer()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call EditOffer with parameter the Offer model
            await Query.EditOffer(Offer);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteOffer(int id)
        {
            // get offer's model from database based on id
            Offer Offer = await _db.Offer.FindAsync(id);
            // if offer doesn't exist return a message
            if (Offer == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call DeleteOffer with parameter the Offer model
            await Query.RemoveOffer(Offer);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }
    }
}
