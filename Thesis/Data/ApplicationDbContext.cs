using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Thesis.Model;

namespace Thesis.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        public DbSet<User> User { get; set; }

        public DbSet<CulturalActivityCategory> CulturalActivityCategory { get; set; }

        public DbSet<CulturalActivity> CulturalActivity { get; set; }

        public DbSet<ReviewCulturalActivity> ReviewCulturalActivity { get; set; }

        public DbSet<FavouriteCulturalActivity> FavouriteCulturalActivity { get; set; }

        public DbSet<Expert> Expert { get; set; }

        public DbSet<ListingCategory> ListingCategory { get; set; }

        public DbSet<Listing> Listing { get; set; }

        public DbSet<ReviewListing> ReviewListing { get; set; }

        public DbSet<FavouriteListing> FavouriteListing { get; set; }

        public DbSet<Message> Message { get; set; }

        public DbSet<Request> Request { get; set; }

        public DbSet<Offer> Offer { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "User");
            });
            builder.Entity<IdentityRole>(entity =>
            {
                entity.ToTable(name: "Role");
            });
            builder.Entity<IdentityUserRole<string>>(entity =>
            {
                entity.ToTable("UserRoles");
            });
            builder.Entity<IdentityUserClaim<string>>(entity =>
            {
                entity.ToTable("UserClaims");
            });
            builder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.ToTable("UserLogins");
            });
            builder.Entity<IdentityRoleClaim<string>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });
            builder.Entity<IdentityUserToken<string>>(entity =>
            {
                entity.ToTable("UserTokens");
            });
            builder.Entity("Thesis.Model.Message", b =>
            {
                b.HasOne("Thesis.Model.User", "UserReceiver")
                    .WithMany()
                    .HasForeignKey("IdReceiver")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                b.HasOne("Thesis.Model.User", "UserSender")
                    .WithMany()
                    .HasForeignKey("IdSender")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

            });
            builder.Entity("Thesis.Model.FavouriteListing", b =>
            {
                b.HasOne("Thesis.Model.User", "User")
                    .WithMany("UserFavouriteListings")
                    .HasForeignKey("IdUser")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
            });
            builder.Entity("Thesis.Model.Offer", b =>
            {
                b.HasOne("Thesis.Model.Expert", "Expert")
                    .WithMany("ExpertsOffers")
                    .HasForeignKey("ExpertId")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                b.HasOne("Thesis.Model.Request", "Request")
                    .WithMany("RequestOffers")
                    .HasForeignKey("RequestId")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
            });
            builder.Entity("Thesis.Model.ReviewListing", b =>
            {
                b.HasOne("Thesis.Model.User", "User")
                    .WithMany("UserReviewsListings")
                    .HasForeignKey("IdReviewer")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();

                b.HasOne("Thesis.Model.Listing", "Listing")
                    .WithMany("ListingReviews")
                    .HasForeignKey("ListingId")
                    .OnDelete(DeleteBehavior.NoAction)
                    .IsRequired();
            });
        }
    }
}
