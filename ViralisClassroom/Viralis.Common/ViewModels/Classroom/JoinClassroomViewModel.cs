using System.ComponentModel.DataAnnotations;

namespace Viralis.Common.ViewModels.Classroom
{
    public class JoinClassroomViewModel
    {
        [Required(ErrorMessage = "Please enter a join code")]
        public string JoinCode { get; set; } = null!;
    }
}
