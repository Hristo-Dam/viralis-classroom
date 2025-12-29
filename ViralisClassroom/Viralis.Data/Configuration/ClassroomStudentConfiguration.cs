using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class ClassroomStudentConfiguration : IEntityTypeConfiguration<ClassroomStudent>
    {
        public void Configure(EntityTypeBuilder<ClassroomStudent> builder)
        {
            builder
                .HasOne(cs => cs.Classroom)
                .WithMany(c => c.Students)
                .HasForeignKey(cs => cs.ClassroomId);

            builder
                .HasOne(cs => cs.Student)
                .WithMany(u => u.ClassroomStudents)
                .HasForeignKey(cs => cs.StudentId);

            builder.HasKey(cs => new { cs.ClassroomId, cs.StudentId });
        }
    }
}
