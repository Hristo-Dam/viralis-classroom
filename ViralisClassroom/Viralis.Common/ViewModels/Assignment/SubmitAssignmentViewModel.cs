using Microsoft.AspNetCore.Http;

namespace Viralis.Common.ViewModels.Assignment
{
    public class SubmitAssignmentViewModel
    {
        public Guid AssignmentId { get; set; }
        public Guid ClassroomId { get; set; }
        public string? TextContent { get; set; }
        public List<IFormFile>? Files { get; set; }
    }
}
