using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Viralis.Common.ViewModels.Classroom;
using Viralis.Common.Constants;

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
            var teacherRoleId = await db.Roles
                .Where(r => r.Name == RoleConstants.TEACHER)
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            if (teacherRoleId == Guid.Empty)
                throw new InvalidOperationException("Teacher role does not exist.");

            bool isTeacher = await db.UserRoles
                .AnyAsync(ur =>
                    ur.UserId == teacherId &&
                    ur.RoleId == teacherRoleId);

            if (!isTeacher)
                throw new UnauthorizedAccessException("Only teachers can create classrooms.");

            var classroom = new Classroom
            {
                Name = model.Name,
                Subject = model.Subject
            };

            classroom.Teachers.Add(new ClassroomTeacher
            {
                TeacherId = teacherId,
                IsOwner = true
            });

            classroom.JoinCode = Guid.NewGuid().ToString("N")[..6].ToUpper();

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

        public async Task JoinByCodeAsync(string joinCode, Guid studentId)
        {
            var classroom = await db.Classrooms
                .FirstOrDefaultAsync(c => c.JoinCode == joinCode);

            if (classroom == null)
                throw new ArgumentException("Invalid join code");

            bool alreadyJoined = await db.ClassroomStudents
                .AnyAsync(cs =>
                    cs.ClassroomId == classroom.Id &&
                    cs.StudentId == studentId);

            if (alreadyJoined)
                throw new InvalidOperationException("Already joined");

            db.ClassroomStudents.Add(new ClassroomStudent
            {
                ClassroomId = classroom.Id,
                StudentId = studentId
            });

            await db.SaveChangesAsync();
        }
    }
}
