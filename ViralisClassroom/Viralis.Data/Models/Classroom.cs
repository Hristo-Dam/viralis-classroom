using System.ComponentModel.DataAnnotations;

namespace Viralis.Data.Models
{
    public class Classroom
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(150)]
        public string Name { get; set; } = null!;

        [Required]
        public string Subject { get; set; } = null!;

        public ICollection<ClassroomTeacher> Teachers { get; set; }
            = new List<ClassroomTeacher>();

        public ICollection<ClassroomStudent> Students { get; set; }
            = new List<ClassroomStudent>();

        public ICollection<Assignment> Assignments { get; set; }
            = new List<Assignment>();

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();
    }
}
