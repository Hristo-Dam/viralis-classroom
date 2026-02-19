using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Viralis.Data.Configuration;
using Viralis.Data.Models;

namespace Viralis.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        // DbSets
        public DbSet<Classroom> Classrooms { get; set; }
        public DbSet<ClassroomStudent> ClassroomStudents { get; set; }
        public DbSet<ClassroomTeacher> ClassroomTeachers { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<UserAssignment> UserAssignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<SchoolAdministrator> SchoolAdministrators { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.ApplyConfiguration(new ClassroomStudentConfiguration());
            builder.ApplyConfiguration(new ClassroomTeacherConfigration());
            builder.ApplyConfiguration(new UserAssignmentConfiguration());
            builder.ApplyConfiguration(new AssignmentConfiguration());
            builder.ApplyConfiguration(new SchoolAdministratorConfiguration());

            base.OnModelCreating(builder);
        }
    }
}
