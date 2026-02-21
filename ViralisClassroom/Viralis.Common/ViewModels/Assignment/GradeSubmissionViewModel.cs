using System.ComponentModel.DataAnnotations;

namespace Viralis.Common.ViewModels.Assignment
{
    public class GradeSubmissionViewModel
    {
        public Guid SubmissionId { get; set; }
        public Guid AssignmentId { get; set; }
        public Guid ClassroomId { get; set; }

        [Range(0, 100)]
        public double Grade { get; set; }

        public string? Feedback { get; set; }
    }
}
