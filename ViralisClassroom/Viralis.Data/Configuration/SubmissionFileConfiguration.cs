using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class SubmissionFileConfiguration : IEntityTypeConfiguration<SubmissionFile>
    {
        public void Configure(EntityTypeBuilder<SubmissionFile> builder)
        {
            builder.HasOne(f => f.Submission)
                   .WithMany(s => s.Files)
                   .HasForeignKey(f => f.SubmissionId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
