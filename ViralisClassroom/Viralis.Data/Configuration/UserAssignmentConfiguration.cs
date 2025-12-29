using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

public class UserAssignmentConfiguration
    : IEntityTypeConfiguration<UserAssignment>
{
    public void Configure(EntityTypeBuilder<UserAssignment> builder)
    {
        builder.HasKey(ua => new { ua.AssignmentId, ua.StudentId });

        builder.HasOne(ua => ua.Assignment)
            .WithMany(a => a.AssignedStudents)
            .HasForeignKey(ua => ua.AssignmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Student)
            .WithMany(u => u.UserAssignments)
            .HasForeignKey(ua => ua.StudentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ua => ua.Submission)
            .WithOne(s => s.UserAssignment)
            .HasForeignKey<Submission>(s => new { s.AssignmentId, s.StudentId })
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("UserAssignments");
    }
}
