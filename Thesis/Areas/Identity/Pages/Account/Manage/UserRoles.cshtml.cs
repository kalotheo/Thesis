using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class UserRolesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRolesModel(ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public User UserSelected { get; set; }

        [BindProperty]
        public List<SelectListItem> ApplicationRoles { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task LoadAsync(User user, string id)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // get selected user's model based on username
            UserSelected = await _db.User.SingleOrDefaultAsync(x => x.UserName == id);
            // get selected user's roles
            var userInRole = _db.UserRoles.Where(x => x.UserId == UserSelected.Id).Select(x => x.RoleId);
            // create a select list with roles and select assigned user's roles 
            ApplicationRoles = await _roleManager.Roles.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Name,
                Selected = userInRole.Contains(x.Id)
            }).ToListAsync();

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGet(string id)
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // call LoadAsync
            await LoadAsync(user, id);
            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            // get selected user's model based on id
            User UserPost = await _db.User.FindAsync(UserSelected.Id);
            // get selected user's roles
            var roles = await _userManager.GetRolesAsync(UserPost);
            // remove all roles from user
            var result = await _userManager.RemoveFromRolesAsync(UserPost, roles);
            // if it fails
            if (!result.Succeeded)
            {
                StatusMessage = "Error. Cannot remove user existing roles!";
                return RedirectToPage();
            }
            // add all selected roles to user
            result = await _userManager.AddToRolesAsync(UserPost, ApplicationRoles.Where(x => x.Selected).Select(x => x.Text));
            // if it fails
            if (!result.Succeeded)
            {
                StatusMessage = "Error. Cannot add selected roles to user!";
                return RedirectToPage();
            }
            // save changes to database
            await _db.SaveChangesAsync();
            // get logged-in user
            User user = await _userManager.GetUserAsync(User);
            // if user matches selected user
            if (user == UserPost)
            {
                // refresh cookie of user
                await _signInManager.RefreshSignInAsync(user);
                // if user has selected not to be an admin anymore
                if (!ApplicationRoles.Where(x => x.Selected).Select(x => x.Text).Contains("Admin"))
                {
                    // redirect him to index
                    StatusMessage = "Roles successfully changed!";
                    return RedirectToPage("Index");
                }
            }

            StatusMessage = "Roles successfully changed!";
            return RedirectToPage("Users");
            
        }
    }
}
