using Microsoft.AspNetCore.Identity;

namespace Viralis.Data.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public Guid? SchoolAdminId { get; set; }
        public SchoolAdministrator? SchoolAdmin { get; set; }
        public ICollection<ClassroomStudent> ClassroomStudents { get; set; }
            = new List<ClassroomStudent>();

        public ICollection<ClassroomTeacher> ClassroomTeachers { get; set; }
            = new List<ClassroomTeacher>();

        public ICollection<Submission> Submissions { get; set; }
            = new List<Submission>();

        public ICollection<Message> Messages { get; set; }
            = new List<Message>();

        public ICollection<UserAssignment> UserAssignments { get; set; }
            = new List<UserAssignment>();
    }
}
