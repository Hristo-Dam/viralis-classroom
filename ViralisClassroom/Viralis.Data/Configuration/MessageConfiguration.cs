using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Viralis.Data.Models;

namespace Viralis.Data.Configuration
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {

            // Message → Classroom (many-to-one)
            builder.HasOne(m => m.Classroom)
                .WithMany(c => c.Messages)
                .HasForeignKey(m => m.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Message → Sender (ApplicationUser)
            builder.HasOne(m => m.Sender)
                .WithMany(u => u.Messages)
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Self-referencing: Message replies
            builder.HasOne(m => m.ParentMessage)
                .WithMany(m => m.Replies)
                .HasForeignKey(m => m.ParentMessageId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
