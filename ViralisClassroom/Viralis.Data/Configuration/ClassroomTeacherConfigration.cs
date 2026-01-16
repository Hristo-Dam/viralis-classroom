using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class ClassroomTeacherConfigration : IEntityTypeConfiguration<ClassroomTeacher>
    {
        public void Configure(EntityTypeBuilder<ClassroomTeacher> builder)
        {
            builder.HasKey(ct => new { ct.ClassroomId, ct.TeacherId });

            builder
                .HasOne(ct => ct.Classroom)
                .WithMany(c => c.Teachers)
                .HasForeignKey(ct => ct.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(ct => ct.Teacher)
                .WithMany(u => u.ClassroomTeachers)
                .HasForeignKey(ct => ct.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
