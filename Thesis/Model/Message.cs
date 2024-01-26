using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Thesis.Model
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime MessageDate { get; set; }

        [Required]
        [Display(Name = "Message")]
        public string MessageText { get; set; }

        public bool Read { get; set; }

        public User UserSender { get; set; }

        [Required]
        [ForeignKey("UserSender")]
        public string IdSender { get; set; }

        public User UserReceiver { get; set; }

        [Required]
        [ForeignKey("UserReceiver")]
        public string IdReceiver { get; set; }

        public Listing Listing { get; set; }

        [ForeignKey("Listing")]
        public int? ListingId { get; set; }

        public Request Request { get; set; }

        [ForeignKey("Request")]
        public int? RequestId { get; set; }
    }
}
