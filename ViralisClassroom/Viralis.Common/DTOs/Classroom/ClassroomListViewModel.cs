namespace Viralis.Common.DTOs.Classroom
{
    public class ClassroomListViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public bool IsTeacher { get; set; }
    }
}
