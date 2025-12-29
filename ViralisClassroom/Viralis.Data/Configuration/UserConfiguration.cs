using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            // Configure ClassroomStudent relationship
            builder
                .HasMany(u => u.ClassroomStudents)
                .WithOne(cs => cs.Student)
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure ClassroomTeacher relationship
            builder
                .HasMany(u => u.ClassroomTeachers)
                .WithOne(ct => ct.Teacher)
                .HasForeignKey(ct => ct.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Messages relation
            builder
                .HasMany(u => u.Messages)
                .WithOne(m => m.Sender)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Cascade);

            // UserAssignments relation
            builder
                .HasMany(u => u.UserAssignments)
                .WithOne(ua => ua.Student)
                .HasForeignKey(ua => ua.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
