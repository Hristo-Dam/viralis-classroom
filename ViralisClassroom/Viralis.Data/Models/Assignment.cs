using Viralis.Data.Models;

public class Assignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ClassroomId { get; set; }
    public Classroom Classroom { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;

    public DateTime AssignmentDate { get; set; } = DateTime.UtcNow;
    public DateTime DueDate { get; set; }

    public Guid AssigningTeacherId { get; set; }
    public ApplicationUser AssigningTeacher { get; set; } = null!;

    public ICollection<UserAssignment> AssignedStudents{ get; set; }
        = new List<UserAssignment>();

    public ICollection<AssignmentFile> Files { get; set; } = new List<AssignmentFile>();
}
