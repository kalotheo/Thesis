using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Requests
{
    public class ViewModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public ViewModel(ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Request request { get; set; }

        public List<string> tags { get; set; }

        private List<Request> RequestsTags = new List<Request>();

        public IEnumerable<Request> Requests { get; set; }

        public ICollection<Offer> ExistingOffer { get; set; }

        [BindProperty]
        public Offer Offer { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            // get request's model from database including user, request offers, expert, user models based on id
            request = await _db.Request.Include(x => x.User).Include(x => x.RequestOffers).ThenInclude(x => x.Expert).ThenInclude(x => x.User).SingleOrDefaultAsync(x => x.Id == id);

            // if request is null return a message
            if (request == null)
            {
                return NotFound($"Unable to load request with id '{id}'.");
            }

            // get a list of all requests excluding current request from list, including user
            IEnumerable<Request> AllRequests = await _db.Request
                .Include(x => x.User)
                .Where(x => x.Id != request.Id)
                .ToListAsync();

            // get requests tags to a string list splitted by comma
            tags = request.Tags.Split(',').ToList();
            List<string> allTags = new List<string>();

            // for every request
            foreach (var request in AllRequests)
            {
                // get request tags to a string list splitted by comma
                allTags = request.Tags.Split(',').ToList();
                // if allTags list and tags list have common tags
                if (allTags.Intersect(tags).Any())
                {
                    // add it to RequestsTags list
                    RequestsTags.Add(request);
                }
            }

            //  take the first 12 items of RequestsTags list
            Requests = RequestsTags.Take(12);

            // if a user is logged in
            if (_signInManager.IsSignedIn(User))
            {
                // initialize Query class passing ApplicationDbContext to constructor
                Query = new Query(_db);
                // get offer of this request of this user
                ExistingOffer = await Query.ExistingOffer(_userManager.GetUserId(User));
                // get all user's unread messages
                UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));
            }

            return Page();
        }

        public async Task<IActionResult> OnPostDelete(int id, int redirectPageId)
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
            // if user is deleting this request
            if (id == redirectPageId)
            {
                // redirect him to index
                return RedirectToPage("Index");
            }
            else
            {
                // redirect him to request's details
                return RedirectToPage("View", new { id = redirectPageId });
            }
        }

        public async Task<IActionResult> OnPostOffer(int? id, int? id2)
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
            if (id2 != null)
            {
                return RedirectToPage("View", new { id = id2 });
            }
            return RedirectToPage("View", new { id = Offer.RequestId });
            
        }

        public async Task<IActionResult> OnPostEditOffer()
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call EditOffer with parameter the Offer model
            await Query.EditOffer(Offer);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("View", new { id = Offer.RequestId });
        }

        public async Task<IActionResult> OnPostDeleteOffer(int id)
        {
            // initialize Query class passing ApplicationDbContext to constructor
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
            return RedirectToPage("View", new { id = Offer.RequestId });
        }
    }
}
