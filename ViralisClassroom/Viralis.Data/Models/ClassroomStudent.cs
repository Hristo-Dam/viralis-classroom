namespace Viralis.Data.Models
{
    public class ClassroomStudent
    {
        public Guid ClassroomId { get; set; }
        public Classroom Classroom { get; set; } = null!;

        public Guid StudentId { get; set; }
        public ApplicationUser Student { get; set; } = null!;
    }
}
