using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viralis.Data.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        [Required]
        public Guid SenderId { get; set; }
        public ApplicationUser Sender { get; set; } = null!;

        [Required, MaxLength(750)]
        public string Content { get; set; } = null!;

        [Required]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid? ParentMessageId { get; set; }
        public Message? ParentMessage { get; set; }

        public ICollection<Message>? Replies { get; set; } = new List<Message>();
    }
}
