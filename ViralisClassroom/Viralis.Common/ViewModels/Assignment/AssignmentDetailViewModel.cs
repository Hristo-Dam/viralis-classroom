namespace Viralis.Common.ViewModels.Assignment
{
    public class AssignmentDetailViewModel
    {
        public Guid Id { get; set; }
        public Guid ClassroomId { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public bool IsOverdue => DateTime.UtcNow > DueDate;
        public bool IsTeacher { get; set; }

        // Student view
        public SubmissionViewModel? MySubmission { get; set; }

        // Teacher view
        public List<StudentSubmissionViewModel> Submissions { get; set; } = new();
    }
    public class SubmissionViewModel
    {
        public Guid Id { get; set; }
        public string? TextContent { get; set; }
        public List<string> FileNames { get; set; } = new();
        public double? Grade { get; set; }
        public string? TeacherFeedback { get; set; }
        public DateTime SubmittedOn { get; set; }
    }

    public class StudentSubmissionViewModel
    {
        public Guid StudentId { get; set; }
        public string StudentEmail { get; set; } = null!;
        public bool HasSubmitted { get; set; }
        public Guid? SubmissionId { get; set; }
        public double? Grade { get; set; }
        public DateTime? SubmittedOn { get; set; }
    }
}
