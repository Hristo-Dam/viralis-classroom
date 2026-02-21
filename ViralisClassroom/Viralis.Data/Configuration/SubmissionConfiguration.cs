using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.TextContent)
                .IsRequired(false);

            builder.Property(s => s.Grade)
                .IsRequired(false);

            builder.Property(s => s.TeacherFeedback)
                .IsRequired(false);
        }
    }
}
