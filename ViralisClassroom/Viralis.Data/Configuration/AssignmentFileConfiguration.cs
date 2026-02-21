using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class AssignmentFileConfiguration : IEntityTypeConfiguration<AssignmentFile>
    {
        public void Configure(EntityTypeBuilder<AssignmentFile> builder)
        {
            builder.HasKey(f => f.Id);
            builder.Property(f => f.FileName).IsRequired().HasMaxLength(255);
            builder.Property(f => f.FilePath).IsRequired().HasMaxLength(500);

            builder.HasOne(f => f.Assignment)
                .WithMany(a => a.Files)
                .HasForeignKey(f => f.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
