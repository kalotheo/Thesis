using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailSender _emailSender;

        public IndexModel(ApplicationDbContext db,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailSender emailSender)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public User UserManager { get; set; }

        [BindProperty]
        public FileUserViewModel FileUpload { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public Expert Expert { get; set; }

        public bool IsEmailConfirmed { get; set; }

        public List<SelectListItem> ExpertAvailability { get; private set; }

        public List<SelectListItem> ExpertExperience { get; private set; }

        public List<string> TagsList = new List<string>();

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public class InputModel
        {
            [DataType(DataType.Password)]
            [Display(Name = "Current password")]
            public string OldPassword { get; set; }

            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        private async Task LoadAsync(User user)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // get user model based on user id
            UserManager = await _db.User.FindAsync(userId);
            // get user model based on user id
            Expert = await _db.Expert.FindAsync(userId);

            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            // create new select list with availability options for experts
            ExpertAvailability = new List<SelectListItem>
            {
                new SelectListItem {Text = "-", Value = string.Empty},
                new SelectListItem {Text = "On-Demand", Value = "On-Demand"},
                new SelectListItem {Text = "Part-Time", Value = "Part-Time"},
                new SelectListItem {Text = "Full-Time", Value = "Full-Time"}
            };

            // create new select list with experience options for experts
            ExpertExperience = new List<SelectListItem>
            {
                new SelectListItem {Text = "-", Value = string.Empty},
                new SelectListItem {Text = "1 Year", Value = "1 Year"},
                new SelectListItem {Text = "2 - 5 Years", Value = "2 - 5 Years"},
                new SelectListItem {Text = "6 - 9 Years", Value = "6 - 9 Years"},
                new SelectListItem {Text = "10+ Years", Value = "10+ Years"},
            };

            // get all experts and group them by their tags
            IEnumerable<IGrouping<string, Expert>> Experts = _db.Expert.ToList().GroupBy(x => x.Tags);
            List<string> expertsTags = new List<string>();

            // for every expert
            foreach (var item in Experts)
            {
                // get experts tags to a string list splitted by comma
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

            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var hasPassword = await _userManager.HasPasswordAsync(user);
            if (!hasPassword)
            {
                return RedirectToPage("./SetPassword");
            }
            // call LoadAsync
            await LoadAsync(user);
            return Page();
        }

        private async Task SendVerification(User user, bool verification)
        {
            var userId = await _userManager.GetUserIdAsync(user);
            var email = await _userManager.GetEmailAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { area = "Identity", userId = userId, code = code },
                protocol: Request.Scheme);
            await _emailSender.SendEmailAsync(
                email,
                "Confirm your email",
                $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            if (verification)
            {
                StatusMessage = "Verification email sent. Please check your email.";
            }
            else
            {
                StatusMessage = "Confirmation link to change email sent. Please check your email.";
            }
        }

        public async Task<IActionResult> OnPostSendVerificationEmailAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = _userManager.GetUserId(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            // get expert model based on user id
            Expert ExpertFromDb = await _db.Expert.FindAsync(userId);

            // if expert exists
            if (ExpertFromDb != null)
            {
                if (!ModelState.IsValid)
                {
                    await LoadAsync(user);
                    return Page();
                }
            }

            await SendVerification(user, true);
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // get user model based on user id
            User UserManagerFromDb = await _db.User.FindAsync(userId);
            // get expert model based on user id
            Expert ExpertFromDb = await _db.Expert.FindAsync(userId);

            // if user doesn't exist return a message
            if (UserManagerFromDb == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // if expert exists
            if (ExpertFromDb != null)
            {
                // check if modelstate is valid
                if (!ModelState.IsValid)
                {
                    StatusMessage = "Error. Something went wrong!";
                    await LoadAsync(user);
                    return Page();
                }
            }

            var userName = user.UserName;
            var firstName = user.FirstName;
            var lastName = user.LastName;
            var email = user.Email;

            if (UserManager.UserName != userName)
            {
                // check if username exists
                var userNameExists = await _userManager.FindByNameAsync(UserManager.UserName);
                if (userNameExists != null)
                {
                    StatusMessage = "Error. Username already taken. Select a different username.";
                    return RedirectToPage();
                }
                // set user name
                var changeUserNameResult = await _userManager.SetUserNameAsync(user, UserManager.UserName);
                // if it fails
                if (!changeUserNameResult.Succeeded)
                {
                    foreach (var error in changeUserNameResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        StatusMessage = "Error. " + error.Description;
                    }
                    return RedirectToPage();
                }
            }

            if (UserManager.FirstName != firstName)
            {
                UserManagerFromDb.FirstName = UserManager.FirstName;
            }

            if (UserManager.LastName != lastName)
            {
                UserManagerFromDb.LastName = UserManager.LastName;
            }

            if (UserManager.Email != email)
            {
                // check if email exists
                var emailExists = await _userManager.FindByEmailAsync(UserManager.Email);
                if (emailExists != null)
                {
                    StatusMessage = "Error. Email already taken. Select a different email.";
                    return RedirectToPage();
                }
                // set email
                var changeEmailResult = await _userManager.SetEmailAsync(user, UserManager.Email);
                // if it fails
                if (!changeEmailResult.Succeeded)
                {
                    foreach (var error in changeEmailResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        StatusMessage = "Error. " + error.Description;
                    }
                    return RedirectToPage();
                }

                await SendVerification(user, false);
            }

            var phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (UserManager.PhoneNumber != phoneNumber)
            {
                // set phone number to user
                var changePhoneResult = await _userManager.SetPhoneNumberAsync(UserManagerFromDb, UserManager.PhoneNumber);
                if (!changePhoneResult.Succeeded)
                {
                    foreach (var error in changePhoneResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        StatusMessage = "Error. " + error.Description;
                    }
                    return RedirectToPage();
                }
            }

            if (Input.OldPassword != null && Input.NewPassword != null)
            {
                var changePasswordResult = await _userManager.ChangePasswordAsync(user, Input.OldPassword, Input.NewPassword);
                if (!changePasswordResult.Succeeded)
                {
                    foreach (var error in changePasswordResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                        StatusMessage = "Error. " + error.Description;
                    }
                    return RedirectToPage();
                }
            }

            // if user uploaded profile picture
            if (FileUpload.Files != null)
            {
                if (FileUpload.Files.Length > 0)
                {
                    // get path of directory of profile images
                    string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/profiles");

                    // create folder if it doesn't exist
                    if (!Directory.Exists(path))
                        Directory.CreateDirectory(path);

                    // get filename
                    string fileName = FileUpload.Files.FileName;

                    // if file exists in directory
                    if (System.IO.File.Exists(Path.Combine(path, fileName)))
                    {
                        // generate a random number
                        Random rnd = new Random();
                        // append this number with the underscore to fileName
                        fileName =  rnd.Next() + "_" + fileName;
                    }

                    // combine path with filename
                    string fileNameWithPath = Path.Combine(path, fileName);

                    using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                    {
                        // copy image to path
                        FileUpload.Files.CopyTo(stream);
                        // set user profile picture the filename of the image
                        UserManagerFromDb.ProfilePicture = fileName;
                    }
                }
            }

            // update user
            await _userManager.UpdateAsync(UserManagerFromDb);
            // refresh cookie of user
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";

            // if user has enabled expert fields
            if (Expert.Availability != null)
            {
                // if user registers himself as expert
                if (ExpertFromDb == null)
                {                    
                    // check if modelstate is valid
                    if (!ModelState.IsValid)
                    {
                        StatusMessage = "Error. Something went wrong!";
                        await LoadAsync(user);
                        return Page();
                    }

                    // add expert model to database
                    await _db.Expert.AddAsync(Expert);
                    // add role expert to user
                    await _userManager.AddToRoleAsync(user, "Expert");
                    // save changes to database
                    await _db.SaveChangesAsync();
                    // refresh cookie of user
                    await _signInManager.RefreshSignInAsync(user);
                    StatusMessage = "Your expert profile has been created";
                    return RedirectToPage();
                }
                // if expert edits his profile
                else
                {
                    // check if modelstate is valid
                    if (!ModelState.IsValid)
                    {
                        StatusMessage = "Error. Something went wrong!";
                        await LoadAsync(user);
                        return Page();
                    }

                    // assign new fields to old ones from form
                    ExpertFromDb.HourlyRate = Expert.HourlyRate;
                    ExpertFromDb.Availability = Expert.Availability;
                    ExpertFromDb.Experience = Expert.Experience;
                    ExpertFromDb.ProfileInfo = Expert.ProfileInfo;
                    ExpertFromDb.Tags = Expert.Tags;
                    // save changes to database
                    await _db.SaveChangesAsync();
                    StatusMessage = "Your expert profile has been updated";
                    return RedirectToPage();
                }
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDelete()
        {
            // get logged-in user
            var user = await _userManager.GetUserAsync(User);
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // get user model based on user id
            Expert Expert = await _db.Expert.FindAsync(userId);
            // if expert doesn't exist return not found
            if (Expert == null)
            {
                return NotFound();
            }
            // get list of messages where user is either the sender or the receiver
            ICollection<Message> Messages = await _db.Message.Where(x => x.IdSender == userId || x.IdReceiver == userId).ToListAsync();
            // for every message set the listingId to null
            foreach (var item in Messages)
            {
                item.ListingId = null;
            }
            // get list of offers where expert's id equals to user's id
            ICollection<Offer> Offers = await _db.Offer.Where(x => x.ExpertId == userId).ToListAsync();
            // remove all offers from database
            _db.Offer.RemoveRange(Offers);
            // get list of listings where expert's id equals to user's id
            ICollection<Listing> Listings = await _db.Listing.Where(x => x.ExpertId == userId).Include(x => x.ListingReviews).ToListAsync();

            List<ReviewListing> ExpertReviewListings = new List<ReviewListing>();

            // for every listing
            foreach (var listing in Listings)
            {
                // for every listing's review
                foreach (var item in listing.ListingReviews)
                {
                    // if ExpertReviewListings list doesn't contain item
                    if (!ExpertReviewListings.Contains(item))
                    {
                        // add item to list
                        ExpertReviewListings.Add(item);
                    }
                }
            }

            // remove all reviews of expert's listings from database
            _db.ReviewListing.RemoveRange(ExpertReviewListings);
            // remove all listings from database
            _db.Listing.RemoveRange(Listings);
            // remove expert from database
            _db.Expert.Remove(Expert);
            // remove role expert from user
            await _userManager.RemoveFromRoleAsync(user, "Expert");
            // save changes to database
            await _db.SaveChangesAsync();
            // refresh cookie of user
            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your expert profile has been deleted successfully!";
            return RedirectToPage();
        }

    }
}
