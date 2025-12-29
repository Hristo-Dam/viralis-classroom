namespace Viralis.Data.Models
{
    public class UserAssignment
    {
        public Guid AssignmentId { get; set; }
        public Assignment Assignment { get; set; } = null!;

        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; } = null!;

        public Guid SubmissionId { get; set; }
        public Submission? Submission { get; set; }
    }
}
