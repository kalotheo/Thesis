using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class ListingCategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public ListingCategoriesModel(ApplicationDbContext db,
            UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public ListingCategory ListingCategory { get; set; }

        public ICollection<ListingCategory> ListingCategories { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task LoadAsync(User user, int? id)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // if user is editing a specific category, get category's model from database based on id
            ListingCategory = await _db.ListingCategory.FindAsync(id);
            // get all categories list from database
            ListingCategories = await _db.ListingCategory.ToListAsync();
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // if user is editing a category
            if (id != null)
            {
                // get category's model from database based on id
                ListingCategory = await _db.ListingCategory.FindAsync(id);
                // if category doesn't exist return a message
                if (ListingCategory == null)
                {
                    return NotFound($"Unable to load Listing Category with ID '{id}'.");
                }
            }
            // call LoadAsync
            await LoadAsync(user, id);
            return Page();
        }

        public async Task<IActionResult> OnPost(int? id)
        {
            // if user is creating a new category
            if (id == null)
            {
                // check if modelstate is valid
                if (!ModelState.IsValid)
                {
                    StatusMessage = "Error. Something went wrong!";
                    return RedirectToPage();
                }

                // check if category's name exist in database
                ListingCategory ExistingListingCategory = await _db.ListingCategory.SingleOrDefaultAsync(x => x.CategoryName == ListingCategory.CategoryName);
                if (ExistingListingCategory != null)
                {
                    StatusMessage = "Error. This category's name already exists! Please select a different name.";
                    return RedirectToPage();
                }

                // add new listing category to database
                await _db.ListingCategory.AddAsync(ListingCategory);
                StatusMessage = "Category saved successfully!";
            }
            // if user is editing an existing category
            else
            {
                // get category's model from database based on id
                ListingCategory ListingCategoryFromDb = await _db.ListingCategory.FindAsync(id);
                // check if modelstate is valid
                if (!ModelState.IsValid)
                {
                    StatusMessage = "Error. Something went wrong!";
                    return RedirectToPage();
                }
                // assign new fields to old ones from form
                ListingCategoryFromDb.CategoryName = ListingCategory.CategoryName;
                ListingCategoryFromDb.CategoryDescription = ListingCategory.CategoryDescription;
                ListingCategoryFromDb.CategoryIcon = ListingCategory.CategoryIcon;
                StatusMessage = "Category edited successfully!";
            }
            // save changes in database
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get category's model from database based on id
            ListingCategory ListingCategory = await _db.ListingCategory.FindAsync(id);
            // if category doesn't exist return a message
            if (ListingCategory == null)
            {
                return NotFound($"Unable to load Listing Category with ID '{id}'.");
            }
            // get all listings that have this category
            ICollection<Listing> Listings = await _db.Listing.Where(x => x.CategoryId == id).ToListAsync();
            // for every matched listing make category null
            foreach (var item in Listings)
            {
                item.CategoryId = null;
            }
            // remove listing category from database
            _db.ListingCategory.Remove(ListingCategory);
            // save changes in database
            await _db.SaveChangesAsync();
            StatusMessage = "Listing Category was succesfully deleted!";
            return RedirectToPage();
        }
    }
}
