using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Viralis.Data.Models
{
    public class Submission
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }
        public UserAssignment UserAssignment { get; set; } = null!;

        public string? TextContent { get; set; }
        public ICollection<string>? Links { get; set; }      // for external resources

        public ICollection<SubmissionFile> Files { get; set; }
            = new List<SubmissionFile>();

        public double? Grade { get; set; }             // null = ungraded
        public string? TeacherFeedback { get; set; }

        public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;
    }
}
