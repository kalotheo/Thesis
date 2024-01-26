using System;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Thesis.Areas.Identity.Pages.Account.Manage
{
    public static class ManageNavPages
    {
        public static string Login => "Login";

        public static string Register => "Register";

        public static string Index => "Index";

        public static string Manage => "Manage";

        public static string Users => "Users";

        public static string RecommendedCulturalActivities => "Recommended Cultural Activities";

        public static string FavouriteCulturalActivities => "Favourite Cultural Activities";

        public static string Experts => "Experts";

        public static string CulturalActivities => "Cultural Activities";

        public static string CreateCulturalActivity => "Create Cultural Activity";

        public static string CulturalActivitiesCategories => "Cultural Activities Categories";

        public static string ListingCategories => "Listing Categories";

        public static string Listings => "Listings";

        public static string CreateListing => "Create Listing";

        public static string ManageListings => "Manage Listings";

        public static string Messages => "Messages";

        public static string ManageRequests => "Manage Requests";

        public static string Requests => "Requests";

        public static string CreateRequest => "Create Request";

        public static string Offers => "Offers";

        public static string FavouriteListings => "Favourite Listings";

        public static string ReviewsListings => "Reviews Listings";

        public static string ReviewsCulturalActivities => "Reviews Cultural Activities";

        public static string Email => "Email";

        public static string ChangePassword => "ChangePassword";

        public static string DownloadPersonalData => "DownloadPersonalData";

        public static string DeletePersonalData => "DeletePersonalData";

        public static string ExternalLogins => "ExternalLogins";

        public static string PersonalData => "PersonalData";

        public static string TwoFactorAuthentication => "TwoFactorAuthentication";

        public static string LoginNavClass(ViewContext viewContext) => PageNavClass(viewContext, Login);

        public static string RegisterNavClass(ViewContext viewContext) => PageNavClass(viewContext, Register);

        public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

        public static string ManageNavClass(ViewContext viewContext) => PageNavClass(viewContext, Manage);

        public static string UsersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Users);

        public static string RecommendedCulturalActivitiesNavClass(ViewContext viewContext) => PageNavClass(viewContext, RecommendedCulturalActivities);

        public static string FavouriteCulturalActivitiesNavClass(ViewContext viewContext) => PageNavClass(viewContext, FavouriteCulturalActivities);

        public static string ExpertsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Experts);

        public static string CulturalActivitiesNavClass(ViewContext viewContext) => PageNavClass(viewContext, CulturalActivities);

        public static string CreateCulturalActivityNavClass(ViewContext viewContext) => PageNavClass(viewContext, CreateCulturalActivity);

        public static string CulturalActivitiesCategoriesNavClass(ViewContext viewContext) => PageNavClass(viewContext, CulturalActivitiesCategories);

        public static string ListingCategoriesNavClass(ViewContext viewContext) => PageNavClass(viewContext, ListingCategories);

        public static string ListingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Listings);

        public static string CreateListingNavClass(ViewContext viewContext) => PageNavClass(viewContext, CreateListing);

        public static string ManageListingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ManageListings);

        public static string MessagesNavClass(ViewContext viewContext) => PageNavClass(viewContext, Messages);

        public static string RequestsNavClass(ViewContext viewContext) => PageNavClass(viewContext, Requests);

        public static string ManageRequestsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ManageRequests);

        public static string CreateRequestNavClass(ViewContext viewContext) => PageNavClass(viewContext, CreateRequest);

        public static string OffersNavClass(ViewContext viewContext) => PageNavClass(viewContext, Offers);

        public static string FavouriteListingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, FavouriteListings);

        public static string ReviewsListingsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ReviewsListings);

        public static string ReviewsCulturalActivitiesNavClass(ViewContext viewContext) => PageNavClass(viewContext, ReviewsCulturalActivities);

        public static string EmailNavClass(ViewContext viewContext) => PageNavClass(viewContext, Email);

        public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

        public static string DownloadPersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DownloadPersonalData);

        public static string DeletePersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, DeletePersonalData);

        public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

        public static string PersonalDataNavClass(ViewContext viewContext) => PageNavClass(viewContext, PersonalData);

        public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

        private static string PageNavClass(ViewContext viewContext, string page)
        {
            var activePage = viewContext.ViewData["ActivePage"] as string
                ?? System.IO.Path.GetFileNameWithoutExtension(viewContext.ActionDescriptor.DisplayName);
            return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase) ? "active" : null;
        }

    }
}


