using Viralis.Common.ViewModels.Classroom;

namespace Viralis.Services.Interfaces.Classroom
{
    public interface IClassroomService
    {
        Task CreateAsync(CreateClassroomViewModel model, Guid teacherId);
        Task<IEnumerable<ClassroomListViewModel>> GetUserClassroomsAsync(Guid userId);
        Task JoinByCodeAsync(string joinCode, Guid studentId);
        Task<ClassroomDetailViewModel?> GetDetailAsync(Guid classroomId, Guid userId);

    }
}
