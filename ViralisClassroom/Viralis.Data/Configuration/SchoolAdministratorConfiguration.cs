using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class SchoolAdministratorConfiguration : IEntityTypeConfiguration<SchoolAdministrator>
    {
        public void Configure(EntityTypeBuilder<SchoolAdministrator> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            // One school has one admin
            builder.HasOne(s => s.SchoolAdmin)
                .WithMany()
                .HasForeignKey(s => s.SchoolAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // One school has many teachers
            builder.HasMany(s => s.Teachers)
                .WithOne(u => u.SchoolAdmin)
                .HasForeignKey(u => u.SchoolAdminId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
