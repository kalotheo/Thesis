using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    [Authorize(Roles = "Admin")]
    public class CulturalActivitiesCategoriesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public CulturalActivitiesCategoriesModel(ApplicationDbContext db,
            UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        private Query Query;

        [BindProperty]
        public CulturalActivityCategory CulturalActivityCategory { get; set; }

        public List<SelectListItem> CulturalActivityParentCategories { get; set; }

        public ICollection<CulturalActivityCategory> CulturalActivityCategories { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task LoadAsync(User user, int? id)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);

            // if user is editing a specific category, get category's model from database based on id
            CulturalActivityCategory = await _db.CulturalActivityCategory.FindAsync(id);

            // get all categories list including their parents from database
            CulturalActivityCategories = await _db.CulturalActivityCategory.Include(x => x.CulturalActivityParent).ToListAsync();

            // create a new select list with all the categories, that don't have a parent,
            // with text the name and value the id of the category
            CulturalActivityParentCategories = (from culturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == null)
                                                select new SelectListItem()
                                                {
                                                    Text = culturalActivityCategory.CategoryName,
                                                    Value = culturalActivityCategory.Id.ToString()
                                                }).Distinct().ToList();

            // insert to select list a first item with null value 
            CulturalActivityParentCategories.Insert(0, new SelectListItem()
            {
                Text = "None",
                Value = string.Empty
            });

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
                CulturalActivityCategory = await _db.CulturalActivityCategory.FindAsync(id);
                // if category doesn't exist return a message
                if (CulturalActivityCategory == null)
                {
                    return NotFound($"Unable to load Cultural Activity Category with ID '{id}'.");
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
                CulturalActivityCategory ExistingCulturalActivityCategory = await _db.CulturalActivityCategory.SingleOrDefaultAsync(x => x.CategoryName == CulturalActivityCategory.CategoryName);
                if (ExistingCulturalActivityCategory != null)
                {
                    StatusMessage = "Error. This category's name already exists! Please select a different name.";
                    return RedirectToPage();
                }

                // add new cultural activity category to database
                await _db.CulturalActivityCategory.AddAsync(CulturalActivityCategory);
                StatusMessage = "Category saved successfully!";
            }
            // if user is editing an existing category
            else
            {
                // get category's model from database based on id
                CulturalActivityCategory CulturalActivityCategoryFromDb = await _db.CulturalActivityCategory.FindAsync(id);

                // check if modelstate is valid
                if (!ModelState.IsValid)
                {
                    StatusMessage = "Error. Something went wrong!";
                    return RedirectToPage();
                }

                // assign new fields to old ones from form
                CulturalActivityCategoryFromDb.CategoryParent = CulturalActivityCategory.CategoryParent;
                CulturalActivityCategoryFromDb.CategoryName = CulturalActivityCategory.CategoryName;
                CulturalActivityCategoryFromDb.CategoryDescription = CulturalActivityCategory.CategoryDescription;
                CulturalActivityCategoryFromDb.CategoryIcon = CulturalActivityCategory.CategoryIcon;
                StatusMessage = "Category edited successfully!";
            }

            // save changes in database
            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get category's model from database based on id
            CulturalActivityCategory CulturalActivityCategory = await _db.CulturalActivityCategory.FindAsync(id);
            // if category doesn't exist return a message
            if (CulturalActivityCategory == null)
            {
                return NotFound($"Unable to load Cultural Activity Category with ID '{id}'.");
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveCulturalActivityCategory with parameter the CulturalActivityCategory model
            await Query.RemoveCulturalActivityCategory(CulturalActivityCategory);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage();
        }
    }
}
