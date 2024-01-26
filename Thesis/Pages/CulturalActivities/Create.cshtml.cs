using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.CulturalActivities
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public int? CurrentCategory { get; set; }

        [BindProperty]
        public CulturalActivity CulturalActivity { get; set; }

        [BindProperty]
        public FileCulturalAcitivityViewModel FileUpload { get; set; }
        
        public List<SelectListItem> CulturalActivityCategory { get; set; }

        public List<SelectListItem> CulturalActivitySubCategory { get; private set; }

        public List<string> TagsList = new List<string>();

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;
        
        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task OnGet(int? category)
        {
            // create new select list with all cultural activity categories where parent isn't null
            // with text the name of the category and value the id of the category
            CulturalActivityCategory = (from culturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == null)
                                        select new SelectListItem()
                                        {
                                            Text = culturalActivityCategory.CategoryName,
                                            Value = culturalActivityCategory.Id.ToString()
                                        }).Distinct().ToList();

            // insert to select list a first item with null value 
            CulturalActivityCategory.Insert(0, new SelectListItem{ Text = "-", Value = string.Empty });

            // if category isn't null
            if (category != null)
            {
                CurrentCategory = category;

                // create new select list with all cultural activity categories where parent equals to category id
                // with text the name of the category and value the id of the category

                CulturalActivitySubCategory = (from culturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == category)
                                               select new SelectListItem()
                                               {
                                                   Text = culturalActivityCategory.CategoryName,
                                                   Value = culturalActivityCategory.Id.ToString()
                                               }).Distinct().ToList();

                // insert to select list a first item with null value 
                CulturalActivitySubCategory.Insert(0, new SelectListItem { Text = "-", Value = string.Empty });
            }

            // get all cultural activities and group them by their tags
            IEnumerable<IGrouping<string, CulturalActivity>> CulturalActivities = _db.CulturalActivity.ToList().GroupBy(x => x.Tags);
            List<string> culturalActivitiesTags = new List<string>();

            // for every cultural activity
            foreach (var item in CulturalActivities)
            {
                // get cultural activities tags to a string list splitted by comma
                culturalActivitiesTags = item.Key.Split(',').ToList();

                // for every tag in list
                foreach (var tag in culturalActivitiesTags)
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
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/culturalActivities");

                        // create folder if it doesn't exist
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);

                        // get filename
                        string fileName = file.FileName;

                        if (Regex.IsMatch(fileName, @"\p{IsGreek}"))
                        {
                            // get image extension
                            string extension = Path.GetExtension(fileName);
                            // generate a random number
                            Random rnd = new Random();
                            // append this number with the underscore to fileName
                            fileName = rnd.Next() + "_img" + extension;
                        }

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
                return RedirectToPage();
            }

            // if user uploaded images
            if (FileUpload.Files != null)
            {
                if (FileUpload.Files.Count > 0)
                {
                    List<string> images = new List<string>();
                    foreach (var file in FileUpload.Files)
                    {
                        // get path of directory of cultural activity images
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/culturalActivities");

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
                    CulturalActivity.Images = string.Join(",", images);
                }
            }
            // add cultural activity model to database
            await _db.CulturalActivity.AddAsync(CulturalActivity);
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Cultural Activity has been successfully created!";
            return RedirectToPage("Index");
        }
    }
}
