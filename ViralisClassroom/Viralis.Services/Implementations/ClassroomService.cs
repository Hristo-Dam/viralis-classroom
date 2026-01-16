using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Common.DTOs;
using Viralis.Services.Interfaces;

namespace Viralis.Services.Implementations
{
    public class ClassroomService : IClassroomService
    {
        private readonly ApplicationDbContext db;

        public ClassroomService(ApplicationDbContext db)
        {
            this.db = db;
        }

        public async Task CreateAsync(CreateClassroomViewModel model, Guid teacherId)
        {
            var classroom = new Classroom
            {
                Name = model.Name,
                Subject = model.Subject,
                OwnerTeacherId = teacherId
            };

            classroom.Teachers.Add(new ClassroomTeacher
            {
                TeacherId = teacherId
            });

            db.Classrooms.Add(classroom);
            await db.SaveChangesAsync();
        }
    }
}
