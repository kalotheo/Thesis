using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.Requests
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public User UserManager { get; set; }

        [BindProperty]
        public Request request { get; set; }

        [BindProperty]
        public FileRequestViewModel FileUpload { get; set; }

        public List<SelectListItem> RequestTimeRange { get; private set; }

        public List<string> TagsList = new List<string>();

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet()
        {
            // get user's model based on id
            UserManager = await _userManager.GetUserAsync(User);
            // if user doesn't exist return a message
            if (UserManager == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            // create new select list with time range list
            // with text the name and value of the time range
            RequestTimeRange = new List<SelectListItem>
            {
                new SelectListItem {Text = "-", Value = string.Empty},
                new SelectListItem {Text = "Part Time", Value = "Part Time"},
                new SelectListItem {Text = "Full Time", Value = "Full Time"}
            };

            // get all requests and group them by their tags
            IEnumerable<IGrouping<string, Request>> Requests = _db.Request.ToList().GroupBy(x => x.Tags);
            List<string> requestsTags = new List<string>();

            // for every request
            foreach (var item in Requests)
            {
                // get requests tags to a string list splitted by comma
                requestsTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in requestsTags)
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

        public async Task<IActionResult> OnPost()
        {
            // check if modelstate is valid
            if (!ModelState.IsValid)
            {
                StatusMessage = "Error. Something went wrong!";
                return Page();
            }

            // user uploaded new images
            if (FileUpload.Files != null)
            {
                if (FileUpload.Files.Count > 0)
                {
                    List<string> images = new List<string>();
                    foreach (var file in FileUpload.Files)
                    {
                        // get path of directory of request images
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/requests");

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
                    request.Images = string.Join(",", images);
                }
            }
            // set request date to current datetime
            request.DateAdded = DateTime.Now;
            // add request model to database
            await _db.Request.AddAsync(request);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Request has been successfully created!";
            return RedirectToPage("Index");
        }
    }
}
