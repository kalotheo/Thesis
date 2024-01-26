using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Pages.CulturalActivities
{

    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public EditModel(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public int CurrentCategory { get; set; }

        [BindProperty]
        public CulturalActivity CulturalActivity { get; set; }

        [BindProperty]
        public FileCulturalAcitivityViewModel FileUpload { get; set; }

        public List<SelectListItem> CulturalActivityCategory { get; private set; }

        public List<SelectListItem> CulturalActivitySubCategory { get; private set; }

        public List<string> TagsList = new List<string>();

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        public async Task<IActionResult> OnGet(int id, int? category)
        {

            // get cultural activity model, including reviews model based on the id
            CulturalActivity = await _db.CulturalActivity.Include(x => x.CulturalActivityReviews).SingleOrDefaultAsync(m => m.Id == id);

            // if cultural activity doesn't exist return a message
            if (CulturalActivity == null)
            {
                return NotFound($"Unable to load cultural activity with id '{id}'.");
            }

            // create new select list with all cultural activity categories where parent isn't null
            // with text the name of the category and value the id of the category
            CulturalActivityCategory = (from culturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == null)
                                        select new SelectListItem()
                                        {
                                            Text = culturalActivityCategory.CategoryName,
                                            Value = culturalActivityCategory.Id.ToString()
                                        }).Distinct().ToList();


            if (category != null)
            {
                CurrentCategory = (int)category;

                // create new select list with all cultural activity categories where parent equals to category id
                // with text the name of the category and value the id of the category

                CulturalActivitySubCategory = (from culturalActivityCategory in _db.CulturalActivityCategory.Where(x => x.CategoryParent == category)
                                               select new SelectListItem()
                                               {
                                                   Text = culturalActivityCategory.CategoryName,
                                                   Value = culturalActivityCategory.Id.ToString()
                                               }).Distinct().ToList();

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

            return Page();
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

        public async Task<IActionResult> OnPost(int id)
        {
            // get cultural activity model based on id
            CulturalActivity CulturalActivityFromDb = await _db.CulturalActivity.FindAsync(id);

            // check if modelstate is valid
            if (!ModelState.IsValid)
            {
                StatusMessage = "Error. Something went wrong!";
                return RedirectToPage("Edit", new { id = id, category = CulturalActivityFromDb.CategoryId });
            }

            // user uploaded new images
            if (FileUpload.Files != null)
            {
                if (FileUpload.Files.Count > 0)
                {
                    List<string> images = new List<string>();
                    foreach (var file in FileUpload.Files)
                    {
                        // get path of directory of cultural activity images
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/culturalActivities");

                        //create folder if it doesn't exist
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

                    // if cultural activity images isn't null
                    if (CulturalActivity.Images != null)
                    {
                        // set cultural activity images combining input hidden field of images and uploaded images
                        CulturalActivityFromDb.Images = string.Join(",", CulturalActivity.Images, imagesString);
                    }
                    else
                    {
                        // set cultural activity images as filenames of uploaded images
                        CulturalActivityFromDb.Images = imagesString;
                    }
                }
            }

            // user didn't upload any new images
            else
            {
                // set cultural activity images as input hidden field of images
                CulturalActivityFromDb.Images = CulturalActivity.Images;
            }

            // assign new fields to old ones from form
            CulturalActivityFromDb.CategoryId = CulturalActivity.CategoryId;
            CulturalActivityFromDb.SubcategoryId = CulturalActivity.SubcategoryId;
            CulturalActivityFromDb.Title = CulturalActivity.Title;
            CulturalActivityFromDb.DateStart = CulturalActivity.DateStart;
            CulturalActivityFromDb.DateEnd = CulturalActivity.DateEnd;
            CulturalActivityFromDb.Place = CulturalActivity.Place;
            CulturalActivityFromDb.Description = CulturalActivity.Description;
            CulturalActivityFromDb.Cast = CulturalActivity.Cast;
            CulturalActivityFromDb.Media = CulturalActivity.Media;
            CulturalActivityFromDb.Tags = CulturalActivity.Tags;
            // save changes to database
            await _db.SaveChangesAsync();
            StatusMessage = "Cultural Activity has been successfully edited!";
            return RedirectToPage("View", new { id = id });
            
        }

        public async Task<IActionResult> OnPostDelete(int id)
        {
            // get cultural activity's model from database based on id
            CulturalActivity CulturalActivity = await _db.CulturalActivity.FindAsync(id);
            // if cultural activity doesn't exist return a message
            if (CulturalActivity == null)
            {
                return NotFound();
            }
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call RemoveCulturalActivity with parameter the CulturalActivityCategory model
            await Query.RemoveCulturalActivity(CulturalActivity);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("Index");
        }
    }
}
