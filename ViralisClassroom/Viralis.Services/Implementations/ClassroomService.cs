using Microsoft.EntityFrameworkCore;
using Viralis.Common.Constants;
using Viralis.Common.ViewModels.Assignment;
using Viralis.Common.ViewModels.Classroom;
using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Interfaces.Classroom;

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

        public async Task<ClassroomDetailViewModel?> GetDetailAsync(Guid classroomId, Guid userId)
        {
            var classroom = await db.Classrooms
                .Include(c => c.Teachers)
                    .ThenInclude(ct => ct.Teacher)
                .Include(c => c.Students)
                    .ThenInclude(cs => cs.Student)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.AssignedStudents)
                        .ThenInclude(ua => ua.Submission)
                .FirstOrDefaultAsync(c => c.Id == classroomId);

            if (classroom == null) return null;

            bool isTeacher = classroom.Teachers.Any(ct => ct.TeacherId == userId);
            bool isStudent = classroom.Students.Any(cs => cs.StudentId == userId);

            if (!isTeacher && !isStudent) return null;

            var messages = await db.Messages
                .Where(m => m.ClassroomId == classroomId)
                .Include(m => m.Sender)
                .OrderByDescending(m => m.SentAt)
                .Take(50)
                .ToListAsync();

            return new ClassroomDetailViewModel
            {
                Id = classroom.Id,
                Name = classroom.Name,
                Subject = classroom.Subject,
                JoinCode = classroom.JoinCode,
                IsTeacher = isTeacher,
                Teachers = classroom.Teachers
                    .Select(ct => new MemberViewModel
                    {
                        Id = ct.Teacher.Id,
                        Email = ct.Teacher.Email!
                    }).ToList(),
                Students = classroom.Students
                    .Select(cs => new MemberViewModel
                    {
                        Id = cs.Student.Id,
                        Email = cs.Student.Email!
                    }).ToList(),
                Assignments = classroom.Assignments
                    .Select(a => new AssignmentListViewModel
                    {
                        Id = a.Id,
                        Title = a.Title,
                        Description = a.Description,
                        DueDate = a.DueDate,
                        AssignmentDate = a.AssignmentDate,
                        HasSubmitted = a.AssignedStudents
                            .Any(ua => ua.StudentId == userId && ua.Submission != null)
                    })
                    .OrderByDescending(a => a.DueDate)
                    .ToList(),
                RecentMessages = messages
                    .OrderBy(m => m.SentAt)
                    .Select(m => new ChatMessageViewModel
                    {
                        Id = m.Id,
                        SenderEmail = m.Sender.Email!,
                        SenderInitial = m.Sender.Email!.Substring(0, 1).ToUpper(),
                        Content = m.Content,
                        SentAt = m.SentAt.ToString("HH:mm"),
                        IsOwn = m.SenderId == userId
                    }).ToList()
            };
        }
    }
}
