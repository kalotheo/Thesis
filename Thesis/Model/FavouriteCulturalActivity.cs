using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class FavouriteCulturalActivity
    {
        [Key]
        public int Id { get; set; }

        public User User { get; set; }

        [Required]
        [ForeignKey("User")]
        public string IdUser { get; set; }

        public CulturalActivity CulturalActivity { get; set; }

        [ForeignKey("CulturalActivity")]
        public int CulturalActivityId { get; set; }
    }
}
