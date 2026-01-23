using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Interfaces;
using Viralis.Common.DTOs.Classroom;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<ClassroomListViewModel>> GetUserClassroomsAsync(Guid userId)
        {
            // Get classrooms where the user is a TEACHER
            var teacherClassrooms = await db.ClassroomTeachers
                .Where(ct => ct.TeacherId == userId)
                .Select(ct => new ClassroomListViewModel
                {
                    Id = ct.Classroom.Id,
                    Name = ct.Classroom.Name,
                    Subject = ct.Classroom.Subject,
                    IsTeacher = true
                })
                .ToListAsync();

            // Get classrooms where the user is a STUDENT
            var studentClassrooms = await db.ClassroomStudents
                .Where(cs => cs.StudentId == userId)
                .Select(cs => new ClassroomListViewModel
                {
                    Id = cs.Classroom.Id,
                    Name = cs.Classroom.Name,
                    Subject = cs.Classroom.Subject,
                    IsTeacher = false
                })
                .ToListAsync();

            // Merge and remove duplicates (in case a teacher is also enrolled as student)
            var result = teacherClassrooms
                .Concat(studentClassrooms)
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => c.Name)
                .ToList();

            return result;
        }
    }
}
