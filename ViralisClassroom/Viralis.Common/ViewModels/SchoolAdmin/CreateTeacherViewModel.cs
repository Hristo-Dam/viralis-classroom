using System.ComponentModel.DataAnnotations;

namespace Viralis.Common.ViewModels.SchoolAdmin
{
    public class CreateTeacherViewModel
    {
        [Required, EmailAddress]
        [Display(Name = "Teacher Email")]
        public string Email { get; set; } = null!;

        [Required, MinLength(6)]
        [Display(Name = "Temporary Password")]
        [DataType(DataType.Password)]
        public string TemporaryPassword { get; set; } = null!;
    }
}
