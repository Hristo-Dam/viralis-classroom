using Viralis.Common.DTOs.Classroom;

namespace Viralis.Services.Interfaces
{
    public interface IClassroomService
    {
        Task CreateAsync(CreateClassroomViewModel model, Guid teacherId);
        Task<IEnumerable<ClassroomListViewModel>> GetUserClassroomsAsync(Guid userId);
    }
}
