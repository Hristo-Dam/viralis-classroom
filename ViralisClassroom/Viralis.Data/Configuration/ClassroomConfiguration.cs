using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class ClassroomConfiguration : IEntityTypeConfiguration<Classroom>
    {
        public void Configure(EntityTypeBuilder<Classroom> builder)
        {
            // Primary Key
            builder.HasKey(c => c.Id);

            // Classroom Name
            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(100);

            // One-to-many: Classroom → Assignments
            builder
                .HasMany(c => c.Assignments)
                .WithOne(a => a.Classroom)
                .HasForeignKey(a => a.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many: Classroom → Messages
            builder
                .HasMany(c => c.Messages)
                .WithOne(m => m.Classroom)
                .HasForeignKey(m => m.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many (via explicit join entity): Classroom → ClassroomStudents
            builder
                .HasMany(c => c.Students)
                .WithOne(cs => cs.Classroom)
                .HasForeignKey(cs => cs.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-many (via explicit join entity): Classroom → ClassroomTeachers
            builder
                .HasMany(c => c.Teachers)
                .WithOne(ct => ct.Classroom)
                .HasForeignKey(ct => ct.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
