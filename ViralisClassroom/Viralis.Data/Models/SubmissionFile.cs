using System.ComponentModel.DataAnnotations;

namespace Viralis.Data.Models
{
    public class SubmissionFile
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; }

        [Required, MaxLength(500)]
        public string FilePath { get; set; }
        public long FileSize { get; set; } // In bytes

        public Guid SubmissionId { get; set; }
        public Submission Submission { get; set; }
    }
}
