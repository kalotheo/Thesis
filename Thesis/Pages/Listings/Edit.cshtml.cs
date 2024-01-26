using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Listings
{
    [Authorize(Roles = "Expert")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public EditModel(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public Listing Listing { get; set; }

        [BindProperty]
        public FileListingViewModel FileUpload { get; set; }

        public List<SelectListItem> ListingCategory { get; private set; }

        public List<SelectListItem> ListingAvailability { get; private set; }

        public List<string> TagsList = new List<string>();

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            // if logged-in user doesn't own the listing return an unauthorized message
            Listing authorizedListing = await _db.Listing.Where(x => x.ExpertId.Equals(_userManager.GetUserId(User))).SingleOrDefaultAsync(x => x.Id == id);
            if (authorizedListing == null)
            {
                return Unauthorized();
            }

            // get listing model including reviews based on the id
            Listing = await _db.Listing.Include(x => x.ListingReviews).SingleOrDefaultAsync(x => x.Id == id);

            // if listing doesn't exist return a message
            if (Listing == null)
            {
                return NotFound($"Unable to load listing with id '{id}'.");
            }

            // create new select list with all listing categories
            // with text the name of the category and value the id of the category
            ListingCategory = (from listingCategory in _db.ListingCategory
                               select new SelectListItem()
                               {
                                   Text = listingCategory.CategoryName,
                                   Value = listingCategory.Id.ToString()
                               }).Distinct().ToList();

            // create new select list with availability options
            ListingAvailability = new List<SelectListItem>
            {
                new SelectListItem {Text = "-", Value = string.Empty},
                new SelectListItem {Text = "Same Day", Value = "Same Day"},
                new SelectListItem {Text = "1-Day Notice", Value = "1-Day Notice"},
                new SelectListItem {Text = "3-Day Notice", Value = "3-Day Notice"},
                new SelectListItem {Text = "7-Day Notice", Value = "7-Day Notice"}
            };

            // get all listings and group them by their tags
            IEnumerable<IGrouping<string, Listing>> Listings = _db.Listing.ToList().GroupBy(x => x.Tags);
            List<string> listingsTags = new List<string>();

            // for every listing
            foreach (var item in Listings)
            {
                // get listings tags to a string list splitted by comma
                listingsTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in listingsTags)
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
            UnreadMessages = await Query.GetUnreadMessages(_userManager.GetUserId(User));

            return Page();
        }


        public async Task<IActionResult> OnPost(int id)
        {
            // get listing model based on the id
            Listing ListingFromDb = await _db.Listing.FindAsync(id);

            // check if modelstate is valid
            if (!ModelState.IsValid)
            {
                StatusMessage = "Error. Something went wrong!";
                return RedirectToPage("Edit", new { id = id });
            }

            // user uploaded new images
            if (FileUpload.Files != null)
            {
                if (FileUpload.Files.Count > 0)
                {
                    List<string> images = new List<string>();
                    foreach (var file in FileUpload.Files)
                    {
                        // get path of directory of listing images
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/listings");

                        // create folder if it doesn't exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        // get filename
                        string fileName = file.FileName;

                        // if file exists in directory
                        if (System.IO.File.Exists(Path.Combine(path, fileName)))
                        {
                            // generate a random number
                            Random rnd = new Random();
                            // append this number with the underscore to fileName
                            fileName = rnd.Next() + "_" + fileName;
                        }

                        // combine path with filename
                        string fileNameWithPath = Path.Combine(path, fileName);

                        using (var stream = new FileStream(fileNameWithPath, FileMode.Create))
                        {
                            // copy images to path
                            file.CopyTo(stream);
                            // add it to string list
                            images.Add(fileName);
                        }
                    }

                    // join strings from array with comma
                    string imagesString = string.Join(",", images);

                    // if listing images isn't null
                    if (Listing.Images != null)
                    {
                        // set listing images combining input hidden field of images and uploaded images
                        ListingFromDb.Images = string.Join(",", Listing.Images, imagesString);
                    }
                    else
                    {
                        // set listing images as filenames of uploaded images
                        ListingFromDb.Images = imagesString;
                    }
                }
            }

            // user didn't upload any new images
            else
            {
                // set listing images as input hidden field of images
                ListingFromDb.Images = Listing.Images;
            }

            // assign new fields to old ones from form
            ListingFromDb.CategoryId = Listing.CategoryId;
            ListingFromDb.Title = Listing.Title;
            ListingFromDb.HourlyRate = Listing.HourlyRate;
            ListingFromDb.Availability = Listing.Availability;
            ListingFromDb.Tags = Listing.Tags;
            ListingFromDb.Description = Listing.Description;
            ListingFromDb.Visibility = Listing.Visibility;
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Listing has been successfully edited!";
            return RedirectToPage("View", new { id = id });
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get listing's model from database based on id
            Listing Listing = await _db.Listing.FindAsync(id);
            // if listing doesn't exist return a message
            if (Listing == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveListing with parameter the Listing model
            await Query.RemoveListing(Listing);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("Index");
        }
    }
}
