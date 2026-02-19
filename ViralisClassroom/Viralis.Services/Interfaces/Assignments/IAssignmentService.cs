using Viralis.Common.ViewModels.Assignment;

namespace Viralis.Services.Interfaces.Assignments
{
    public interface IAssignmentService
    {
        Task CreateAsync(CreateAssignmentViewModel model, Guid teacherId);
        Task<List<AssignmentListViewModel>> GetForClassroomAsync(Guid classroomId, Guid userId, bool isTeacher);
        Task<AssignmentDetailViewModel?> GetDetailAsync(Guid assignmentId, Guid userId, bool isTeacher);
        Task SubmitAsync(SubmitAssignmentViewModel model, Guid studentId);
        Task GradeAsync(GradeSubmissionViewModel model, Guid teacherId);
    }
}
