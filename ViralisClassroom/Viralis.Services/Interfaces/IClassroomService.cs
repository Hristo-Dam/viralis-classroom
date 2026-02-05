using Viralis.Common.ViewModels.Classroom;

namespace Viralis.Services.Interfaces
{
    public interface IClassroomService
    {
        Task CreateAsync(CreateClassroomViewModel model, Guid teacherId);
        Task<IEnumerable<ClassroomListViewModel>> GetUserClassroomsAsync(Guid userId);
        Task JoinByCodeAsync(string joinCode, Guid studentId);

    }
}
