using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class ClassroomStudentConfiguration : IEntityTypeConfiguration<ClassroomStudent>
    {
        public void Configure(EntityTypeBuilder<ClassroomStudent> builder)
        {
            builder.HasKey(cs => new { cs.ClassroomId, cs.StudentId });

            builder
                .HasOne(cs => cs.Classroom)
                .WithMany(c => c.Students)
                .HasForeignKey(cs => cs.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(cs => cs.Student)
                .WithMany(u => u.ClassroomStudents)
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
