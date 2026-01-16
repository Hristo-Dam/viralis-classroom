using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

public class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Description)
            .IsRequired();

        // Relationship: Assignment → Teacher
        builder.HasOne(a => a.AssigningTeacher)
            .WithMany()
            .HasForeignKey(a => a.AssigningTeacherId)
            .OnDelete(DeleteBehavior.Restrict);

        // Relationship: Assignment → Classroom
        builder.HasOne(a => a.Classroom)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.ClassroomId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable("Assignments");
    }
}
