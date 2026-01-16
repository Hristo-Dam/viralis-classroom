using Viralis.Common.DTOs;

namespace Viralis.Services.Interfaces
{
    public interface IClassroomService
    {
        Task CreateAsync(CreateClassroomViewModel model, Guid teacherId);
    }
}
