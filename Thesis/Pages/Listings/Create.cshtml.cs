using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Listings
{
    [Authorize(Roles = "Expert")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public Expert Expert { get; set; }

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

        public async Task OnGet()
        {
            // get expert's model based on id
            Expert = await _db.Expert.FindAsync(_userManager.GetUserId(User));

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
            
        }

        public IActionResult OnPostSave(IList<IFormFile> UploadFiles)
        {
            try
            {
                foreach (var file in UploadFiles)
                {
                    if (UploadFiles != null)
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
                            Response.Clear();
                            // pass the filename to response
                            Response.Headers.Add("name", fileName);
                            Response.ContentType = "application/json; charset=utf-8";
                            Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "File uploaded succesfully";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = 204;
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "No Content";
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = e.Message;
            }
            return Content("");
        }

        public async Task<IActionResult> OnPost()
        {
            // check if modelstate is valid
            if (!ModelState.IsValid)
            {
                StatusMessage = "Error. Something went wrong!";
                return Page();
            }

            // if user uploaded images
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
                    Listing.Images = string.Join(",", images);
                }
            }
            // set listing date to current datetime
            Listing.Date = DateTime.Now;
            Listing.Visibility = true;
            // add listing model to database
            await _db.Listing.AddAsync(Listing);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Listing has been successfully created!";
            return RedirectToPage("Index");
        }
    }
}
