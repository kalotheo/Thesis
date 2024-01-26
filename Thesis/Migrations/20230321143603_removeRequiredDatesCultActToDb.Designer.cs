﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Thesis.Data;

namespace Thesis.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230321143603_removeRequiredDatesCultActToDb")]
    partial class removeRequiredDatesCultActToDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.17")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("Role");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("RoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("UserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("UserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("UserTokens");
                });

            modelBuilder.Entity("Thesis.Model.CulturalActivity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<double?>("AverageRating")
                        .HasColumnType("float");

                    b.Property<string>("Cast")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int?>("CountAllRatings")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DateStart")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Images")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Media")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Place")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("SubcategoryId")
                        .HasColumnType("int");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("SubcategoryId");

                    b.ToTable("CulturalActivity");
                });

            modelBuilder.Entity("Thesis.Model.CulturalActivityCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CategoryDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CategoryIcon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("CategoryParent")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("CategoryParent");

                    b.ToTable("CulturalActivityCategory");
                });

            modelBuilder.Entity("Thesis.Model.Expert", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Availability")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("AverageRating")
                        .HasColumnType("float");

                    b.Property<int?>("CountAllRatings")
                        .HasColumnType("int");

                    b.Property<string>("Experience")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("HourlyRate")
                        .HasColumnType("float");

                    b.Property<string>("ProfileInfo")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Expert");
                });

            modelBuilder.Entity("Thesis.Model.FavouriteCulturalActivity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CulturalActivityId")
                        .HasColumnType("int");

                    b.Property<string>("IdUser")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("CulturalActivityId");

                    b.HasIndex("IdUser");

                    b.ToTable("FavouriteCulturalActivity");
                });

            modelBuilder.Entity("Thesis.Model.FavouriteListing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("IdUser")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ListingId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IdUser");

                    b.HasIndex("ListingId");

                    b.ToTable("FavouriteListing");
                });

            modelBuilder.Entity("Thesis.Model.Listing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Availability")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<double?>("AverageRating")
                        .HasColumnType("float");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("int");

                    b.Property<int?>("CountAllRatings")
                        .HasColumnType("int");

                    b.Property<DateTime>("Date")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ExpertId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("HourlyRate")
                        .HasColumnType("float");

                    b.Property<string>("Images")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Tags")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Visibility")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.HasIndex("ExpertId");

                    b.ToTable("Listing");
                });

            modelBuilder.Entity("Thesis.Model.ListingCategory", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("CategoryDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CategoryIcon")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CategoryName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ListingCategory");
                });

            modelBuilder.Entity("Thesis.Model.Message", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("IdReceiver")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("IdSender")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int?>("ListingId")
                        .HasColumnType("int");

                    b.Property<DateTime>("MessageDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("MessageText")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("Read")
                        .HasColumnType("bit");

                    b.Property<int?>("RequestId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("IdReceiver");

                    b.HasIndex("IdSender");

                    b.HasIndex("ListingId");

                    b.HasIndex("RequestId");

                    b.ToTable("Message");
                });

            modelBuilder.Entity("Thesis.Model.Offer", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("ExpertId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("OfferDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OfferText")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("RequestId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ExpertId");

                    b.HasIndex("RequestId");

                    b.ToTable("Offer");
                });

            modelBuilder.Entity("Thesis.Model.Request", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateAdded")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateEnd")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("DateStart")
                        .HasColumnType("datetime2");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Images")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Salary")
                        .HasColumnType("float");

                    b.Property<string>("TimeRange")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Request");
                });

            modelBuilder.Entity("Thesis.Model.ReviewCulturalActivity", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("CulturalActivityId")
                        .HasColumnType("int");

                    b.Property<string>("IdReviewer")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<double>("Rating")
                        .HasColumnType("float");

                    b.Property<DateTime>("ReviewDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewMessage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CulturalActivityId");

                    b.HasIndex("IdReviewer");

                    b.ToTable("ReviewCulturalActivity");
                });

            modelBuilder.Entity("Thesis.Model.ReviewListing", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("IdReviewer")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("ListingId")
                        .HasColumnType("int");

                    b.Property<double>("Rating")
                        .HasColumnType("float");

                    b.Property<DateTime>("ReviewDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("ReviewMessage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("IdReviewer");

                    b.HasIndex("ListingId");

                    b.ToTable("ReviewListing");
                });

            modelBuilder.Entity("Thesis.Model.User", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("FavouriteCategories")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FavouriteTags")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("ProfilePicture")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Thesis.Model.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Thesis.Model.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Thesis.Model.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Thesis.Model.User", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Thesis.Model.CulturalActivity", b =>
                {
                    b.HasOne("Thesis.Model.CulturalActivityCategory", "CulturalActivityMainCategory")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("Thesis.Model.CulturalActivityCategory", "CulturalActivitySubcategory")
                        .WithMany()
                        .HasForeignKey("SubcategoryId");

                    b.Navigation("CulturalActivityMainCategory");

                    b.Navigation("CulturalActivitySubcategory");
                });

            modelBuilder.Entity("Thesis.Model.CulturalActivityCategory", b =>
                {
                    b.HasOne("Thesis.Model.CulturalActivityCategory", "CulturalActivityParent")
                        .WithMany()
                        .HasForeignKey("CategoryParent");

                    b.Navigation("CulturalActivityParent");
                });

            modelBuilder.Entity("Thesis.Model.Expert", b =>
                {
                    b.HasOne("Thesis.Model.User", "User")
                        .WithMany()
                        .HasForeignKey("Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Thesis.Model.FavouriteCulturalActivity", b =>
                {
                    b.HasOne("Thesis.Model.CulturalActivity", "CulturalActivity")
                        .WithMany()
                        .HasForeignKey("CulturalActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Thesis.Model.User", "User")
                        .WithMany("UserFavouriteCulturalActivities")
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CulturalActivity");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Thesis.Model.FavouriteListing", b =>
                {
                    b.HasOne("Thesis.Model.User", "User")
                        .WithMany("UserFavouriteListings")
                        .HasForeignKey("IdUser")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Thesis.Model.Listing", "Listing")
                        .WithMany()
                        .HasForeignKey("ListingId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Listing");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Thesis.Model.Listing", b =>
                {
                    b.HasOne("Thesis.Model.ListingCategory", "ListingCategory")
                        .WithMany()
                        .HasForeignKey("CategoryId");

                    b.HasOne("Thesis.Model.Expert", "Expert")
                        .WithMany("ExpertsListings")
                        .HasForeignKey("ExpertId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Expert");

                    b.Navigation("ListingCategory");
                });

            modelBuilder.Entity("Thesis.Model.Message", b =>
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

                    b.HasOne("Thesis.Model.Listing", "Listing")
                        .WithMany()
                        .HasForeignKey("ListingId");

                    b.HasOne("Thesis.Model.Request", "Request")
                        .WithMany()
                        .HasForeignKey("RequestId");

                    b.Navigation("Listing");

                    b.Navigation("Request");

                    b.Navigation("UserReceiver");

                    b.Navigation("UserSender");
                });

            modelBuilder.Entity("Thesis.Model.Offer", b =>
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

                    b.Navigation("Expert");

                    b.Navigation("Request");
                });

            modelBuilder.Entity("Thesis.Model.Request", b =>
                {
                    b.HasOne("Thesis.Model.User", "User")
                        .WithMany("UserRequests")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("User");
                });

            modelBuilder.Entity("Thesis.Model.ReviewCulturalActivity", b =>
                {
                    b.HasOne("Thesis.Model.CulturalActivity", "CulturalActivity")
                        .WithMany("CulturalActivityReviews")
                        .HasForeignKey("CulturalActivityId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Thesis.Model.User", "User")
                        .WithMany("UserReviewsCulturalActivities")
                        .HasForeignKey("IdReviewer")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CulturalActivity");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Thesis.Model.ReviewListing", b =>
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

                    b.Navigation("Listing");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Thesis.Model.CulturalActivity", b =>
                {
                    b.Navigation("CulturalActivityReviews");
                });

            modelBuilder.Entity("Thesis.Model.Expert", b =>
                {
                    b.Navigation("ExpertsListings");

                    b.Navigation("ExpertsOffers");
                });

            modelBuilder.Entity("Thesis.Model.Listing", b =>
                {
                    b.Navigation("ListingReviews");
                });

            modelBuilder.Entity("Thesis.Model.Request", b =>
                {
                    b.Navigation("RequestOffers");
                });

            modelBuilder.Entity("Thesis.Model.User", b =>
                {
                    b.Navigation("UserFavouriteCulturalActivities");

                    b.Navigation("UserFavouriteListings");

                    b.Navigation("UserRequests");

                    b.Navigation("UserReviewsCulturalActivities");

                    b.Navigation("UserReviewsListings");
                });
#pragma warning restore 612, 618
        }
    }
}