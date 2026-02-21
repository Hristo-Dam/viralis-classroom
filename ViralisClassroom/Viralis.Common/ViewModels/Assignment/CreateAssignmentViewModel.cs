using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Viralis.Common.ViewModels.Assignment
{
    public class CreateAssignmentViewModel
    {
        public Guid ClassroomId { get; set; }

        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(7);
        public List<IFormFile>? Files { get; set; }
    }
}
