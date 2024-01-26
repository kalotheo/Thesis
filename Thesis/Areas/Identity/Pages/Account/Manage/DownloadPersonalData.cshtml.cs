using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Thesis.Data;
using Thesis.Model;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;
        private Dictionary<string, string> personalData;

        public DownloadPersonalDataModel(ApplicationDbContext db,
            UserManager<User> userManager,
            ILogger<DownloadPersonalDataModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public PropertyInfo[] RetrieveProperties(object obj)
        {
            var type = obj.GetType();

            return type.GetProperties();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var userId = _userManager.GetUserId(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' asked for their personal data.", userId);

            // Only include personal data for download
            personalData = new Dictionary<string, string>();

            var personalDataProps = typeof(User).GetProperties().Where(
                            prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));
            foreach (var p in personalDataProps)
            {
                personalData.Add(p.Name, p.GetValue(user)?.ToString() ?? "null");
            }

            // get list of favourite's cultural activities where favourite's user id equals to user's id
            ICollection<FavouriteCulturalActivity> FavouriteCulturalActivities = await _db.FavouriteCulturalActivity.Where(x => x.IdUser == userId).ToListAsync();
            AddListToPersonalData(FavouriteCulturalActivities);

            // get list of reviews cultural activities where reviewer's id equals to user's id
            ICollection<ReviewCulturalActivity> ReviewCulturalActivities = await _db.ReviewCulturalActivity.Where(x => x.IdReviewer == userId).ToListAsync();
            AddListToPersonalData(ReviewCulturalActivities);

            // get list of requests where request's user id equals to user's id
            ICollection<Request> Requests = await _db.Request.Where(x => x.UserId == userId).ToListAsync();
            AddListToPersonalData(Requests);

            // get list of messages where user is either the sender or the receiver
            ICollection<Message> Messages = await _db.Message.Where(x => x.IdSender == userId || x.IdReceiver == userId).ToListAsync();
            AddListToPersonalData(Messages);

            // get list of favourite's listings where favourite's user id equals to user's id
            ICollection<FavouriteListing> FavouriteListings = await _db.FavouriteListing.Where(x => x.IdUser == userId).ToListAsync();
            AddListToPersonalData(FavouriteListings);

            // get list of reviews listings where reviewer's id equals to user's id
            ICollection<ReviewListing> ReviewListings = await _db.ReviewListing.Where(x => x.IdReviewer == userId).ToListAsync();
            AddListToPersonalData(ReviewListings);

            if (User.IsInRole("Expert"))
            {
                // get expert's profile
                Expert Expert = await _db.Expert.FindAsync(userId);
                var personalExpertDataProps = typeof(Expert).GetProperties();
                foreach (var p in personalExpertDataProps)
                {
                    if (!personalData.ContainsKey(p.Name))
                    {
                        if (p.GetValue(Expert)?.ToString() != null)
                        {
                            if (!p.GetValue(Expert).ToString().Contains("Thesis"))
                            {
                                personalData.Add(p.Name, p.GetValue(Expert)?.ToString());
                            }
                        }
                     }
                }

                // get list of listings where expert's id equals to user's id
                ICollection<Listing> Listings = await _db.Listing.Where(x => x.ExpertId == userId).ToListAsync();
                AddListToPersonalData(Listings);

                // get list of offers where expert's id equals to user's id
                ICollection<Offer> Offers = await _db.Offer.Where(x => x.ExpertId == userId).ToListAsync();
                AddListToPersonalData(Offers);

            }

            var logins = await _userManager.GetLoginsAsync(user);
            foreach (var l in logins)
            {
                personalData.Add($"{l.LoginProvider} external login provider key", l.ProviderKey);
            }

            Response.Headers.Add("Content-Disposition", "attachment; filename=PersonalData.json");
            return new FileContentResult(JsonSerializer.SerializeToUtf8Bytes(personalData), "application/json");
        }

        private void AddListToPersonalData<T>(ICollection<T> source)
        {
            foreach (var item in source)
            {
                foreach (var property in RetrieveProperties(item))
                {
                    if (!personalData.ContainsKey(property.Name))
                    {
                        if (property.GetValue(item)?.ToString() != null)
                        {
                            if (!property.GetValue(item).ToString().Contains("Thesis"))
                            {
                                personalData.Add(property.Name, property.GetValue(item)?.ToString());
                            }
                        }
                    }
                }
            }
        }

    }
}
