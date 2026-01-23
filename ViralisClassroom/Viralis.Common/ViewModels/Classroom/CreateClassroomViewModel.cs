using System.ComponentModel.DataAnnotations;

namespace Viralis.Common.ViewModels.Classroom
{
    public class CreateClassroomViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string Subject { get; set; } = null!;
    }
}
