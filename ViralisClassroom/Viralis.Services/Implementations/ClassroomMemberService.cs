using Microsoft.EntityFrameworkCore;
using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Interfaces;

namespace Viralis.Services.Implementations
{
    public class ClassroomMemberService : IClassroomMemberService
    {
        private readonly ApplicationDbContext _context;

        public ClassroomMemberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task JoinClassroomAsync(Guid classroomId, Guid studentId)
        {
            var exists = await _context.ClassroomStudents
                .AnyAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentId);

            if (exists)
                throw new InvalidOperationException("Already joined");

            var classroom = await _context.Classrooms
                .FirstOrDefaultAsync(c => c.Id == classroomId);

            if (classroom == null)
                throw new ArgumentException("Classroom not found");

            _context.ClassroomStudents.Add(new ClassroomStudent
            {
                ClassroomId = classroomId,
                StudentId = studentId
            });

            await _context.SaveChangesAsync();
        }
        public Task AddTeacherAsync(Guid classroomId, Guid teacherId)
        {
            throw new NotImplementedException();
        }

        public Task InviteStudentAsync(Guid classroomId, string email)
        {
            throw new NotImplementedException();
        }

        public Task RemoveStudentAsync(Guid classroomId, Guid studentId)
        {
            throw new NotImplementedException();
        }

        public Task RemoveTeacherAsync(Guid classroomId, Guid teacherId)
        {
            throw new NotImplementedException();
        }
    }
}
