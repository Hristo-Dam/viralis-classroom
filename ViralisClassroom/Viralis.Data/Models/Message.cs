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
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        public Guid SenderId { get; set; }
        public ApplicationUser Sender { get; set; } = null!;

        [Required]
        public string Content { get; set; } = null!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid? ParentMessageId { get; set; }
        public Message? ParentMessage { get; set; }

        public ICollection<Message> Replies { get; set; } = new List<Message>();
    }
}
