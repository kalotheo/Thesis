using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class MessagesDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public MessagesDetailsModel(ApplicationDbContext db, UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public ICollection<Message> Messages { get; set; }

        public User UserManager, UserSpeaker;

        public Expert ExpertSender { get; set; }

        [BindProperty]
        public Message Message { get; set; }

        [BindProperty]
        public List<IFormFile> UploadFiles { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        private async Task LoadAsync(User userManager, User userSpeaker)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(userManager);
            // get speaker id
            var speakerId = await _userManager.GetUserIdAsync(userSpeaker);
            // get list of messages that user has received and speaker has sent including user sender, receiver, listing, request
            ICollection<Message> MessagesReceived = await _db.Message.Include(x => x.UserSender).Include(x => x.UserReceiver).Include(x => x.Listing).Include(x => x.Request).Where(x => x.IdSender == speakerId && x.IdReceiver == userId).ToListAsync();
            // get list of messages that speaker has sent and user has sent including user sender, receiver, listing, request
            ICollection<Message> MessagesSent = await _db.Message.Include(x => x.UserSender).Include(x => x.UserReceiver).Include(x => x.Listing).Include(x => x.Request).Where(x => x.IdSender == userId && x.IdReceiver == speakerId).ToListAsync();
            // set messages that concatenates messages sent and received
            Messages = MessagesReceived.Concat(MessagesSent).OrderBy(x => x.MessageDate).ToList();
            // find expert that has speaker id
            ExpertSender = await _db.Expert.FindAsync(speakerId);
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // get all user's unread messages
            UnreadMessages = await Query.GetUnreadMessages(userId);
            // for every item in list of received messages
            foreach (var item in MessagesReceived)
            {
                // set boolean read as true
                item.Read = true;
            }
            // save changes in database
            await _db.SaveChangesAsync();
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            // get logged-in user
            UserManager = await _userManager.GetUserAsync(User);
            // get user speaker model based on id
            UserSpeaker = await _db.User.SingleOrDefaultAsync(x => x.UserName == id);
            // if user doesn't exist return a message
            if (UserManager == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            // if speaker doesn't exist return a message
            if (UserSpeaker == null)
            {
                return NotFound($"Unable to load speaker with username '{id}'.");
            }
            // call LoadAsync
            await LoadAsync(UserManager, UserSpeaker);
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
                        string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploadfiles/messages");

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

        public async Task<IActionResult> OnPost(string id)
        {
            // initialize Query class passing ApplicationDbContext to constructor
            Query = new Query(_db);
            // call AddMessage with parameter the Message model
            await Query.AddMessage(Message);
            StatusMessage = Query.StatusMessage;
            return RedirectToPage("MessagesDetails", new { id = id });
        }
    }
}
