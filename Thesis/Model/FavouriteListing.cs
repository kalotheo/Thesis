using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class FavouriteListing
    {
        [Key]
        public int Id { get; set; }

        public User User { get; set; }

        [Required]
        [ForeignKey("User")]
        public string IdUser { get; set; }

        public Listing Listing { get; set; }

        [ForeignKey("Listing")]
        public int ListingId { get; set; }
    }
}
