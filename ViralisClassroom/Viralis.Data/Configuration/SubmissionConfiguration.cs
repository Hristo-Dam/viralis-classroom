using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
    {
        public void Configure(EntityTypeBuilder<Submission> builder)
        {
            builder
                .HasMany(s => s.Files)
                .WithOne(f => f.Submission)
                .HasForeignKey(f => f.SubmissionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .Property(s => s.Links)
                .HasConversion(
                    v => string.Join(',', v ?? new List<string>()),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        }
    }
}
