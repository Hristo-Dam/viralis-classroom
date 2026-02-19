namespace Viralis.Common.ViewModels.Assignment
{
    public class AssignmentListViewModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime DueDate { get; set; }
        public bool IsOverdue => DateTime.UtcNow > DueDate;
        public bool HasSubmitted { get; set; } // for students
        public int SubmissionCount { get; set; } // for teachers
    }
}
