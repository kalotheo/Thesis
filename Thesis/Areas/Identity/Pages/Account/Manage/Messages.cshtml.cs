using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class MessagesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;

        public MessagesModel(ApplicationDbContext db,
            UserManager<User> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public Dictionary<string, string[]> UserMessages = new Dictionary<string, string[]>();

        public Dictionary<string, int> UnreadMessagesFromOneUser = new Dictionary<string, int>();

        private Query Query;

        [ViewData]
        public int UnreadMessages { get; set; }

        private async Task LoadAsync(User user)
        {
            // get userId of logged-in user
            var userId = await _userManager.GetUserIdAsync(user);
            // get list of messages where sender or receiver is user including user sender, receiver, listing, request ordered by message date
            ICollection<Message> Messages = await _db.Message.Include(x => x.UserSender).Include(x => x.UserReceiver).Include(x => x.Listing).Include(x => x.Request).Where(x => x.IdSender == userId || x.IdReceiver == userId).OrderByDescending(x => x.MessageDate).ToListAsync();
            
            // for every message in messages list
            foreach (var message in Messages)
            {
                // if dictionary doesn't contain key of username's sender nor user sender id's is equal to user id
                if (!UserMessages.ContainsKey(message.UserSender.UserName) && !message.UserSender.Id.Equals(userId))
                {
                    // add username of sender as key to dictionary and values the id, his first and last name
                    UserMessages.Add(message.UserSender.UserName, new string[] { message.UserSender.Id, message.UserSender.FirstName, message.UserSender.LastName, message.MessageDate.ToString() });
                }
                // if dictionary doesn't contain key of username's receiver nor user receiver id's is equal to user id
                else if (!UserMessages.ContainsKey(message.UserReceiver.UserName) && !message.UserReceiver.Id.Equals(userId))
                {
                    // add username of receiver as key to dictionary and values the id, his first and last name
                    UserMessages.Add(message.UserReceiver.UserName, new string[] { message.UserReceiver.Id, message.UserReceiver.FirstName, message.UserReceiver.LastName, message.MessageDate.ToString() });
                }
            }

            // for every message in userMessages
            foreach (var item in UserMessages)
            {
                // get number of unread messages where user is the receiver and sender is the id and boolean read is set to false
                int unreadMessages = _db.Message.Where(x => x.IdReceiver == userId && x.IdSender == item.Value[0] && x.Read == false).Count();
                // add sender id as key to dictionary and value of unread messages as value
                UnreadMessagesFromOneUser.Add(item.Value[0], unreadMessages);
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
            // call LoadAsync
            await LoadAsync(user);
            return Page();
        }
    }
}
