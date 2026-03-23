using Microsoft.EntityFrameworkCore;
using Xunit;
using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Implementations;

namespace Viralis.Tests
{
    public class ClassroomMemberServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ClassroomMemberService _service;

        public ClassroomMemberServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _service = new ClassroomMemberService(_db);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<Guid> SeedClassroomAsync()
        {
            var classroom = new Classroom { Id = Guid.NewGuid(), Name = "Class", Subject = "S", JoinCode = "CODE01" };
            _db.Classrooms.Add(classroom);
            await _db.SaveChangesAsync();
            return classroom.Id;
        }

        private async Task<Guid> SeedStudentAsync()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "student@test.com", UserName = "student@test.com" };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user.Id;
        }

        // ── JoinClassroomAsync ────────────────────────────────────────────────

        [Fact]
        public async Task JoinClassroomAsync_AlreadyJoined_ThrowsInvalidOperation()
        {
            var classroomId = await SeedClassroomAsync();
            var studentId = await SeedStudentAsync();

            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroomId, StudentId = studentId });
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.JoinClassroomAsync(classroomId, studentId));
        }

        [Fact]
        public async Task JoinClassroomAsync_ClassroomNotFound_ThrowsArgumentException()
        {
            var studentId = await SeedStudentAsync();

            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.JoinClassroomAsync(Guid.NewGuid(), studentId));
        }

        [Fact]
        public async Task JoinClassroomAsync_ValidRequest_AddsStudent()
        {
            var classroomId = await SeedClassroomAsync();
            var studentId = await SeedStudentAsync();

            await _service.JoinClassroomAsync(classroomId, studentId);

            var exists = await _db.ClassroomStudents.AnyAsync(
                cs => cs.ClassroomId == classroomId && cs.StudentId == studentId);
            Assert.True(exists);
        }

        [Fact]
        public async Task JoinClassroomAsync_ValidRequest_SavesCorrectIds()
        {
            var classroomId = await SeedClassroomAsync();
            var studentId = await SeedStudentAsync();

            await _service.JoinClassroomAsync(classroomId, studentId);

            var record = await _db.ClassroomStudents.FirstOrDefaultAsync();
            Assert.NotNull(record);
            Assert.Equal(classroomId, record.ClassroomId);
            Assert.Equal(studentId, record.StudentId);
        }

        [Fact]
        public async Task JoinClassroomAsync_TwoDifferentStudents_BothJoin()
        {
            var classroomId = await SeedClassroomAsync();
            var student1 = new ApplicationUser { Id = Guid.NewGuid(), Email = "s1@test.com", UserName = "s1@test.com" };
            var student2 = new ApplicationUser { Id = Guid.NewGuid(), Email = "s2@test.com", UserName = "s2@test.com" };
            _db.Users.AddRange(student1, student2);
            await _db.SaveChangesAsync();

            await _service.JoinClassroomAsync(classroomId, student1.Id);
            await _service.JoinClassroomAsync(classroomId, student2.Id);

            var count = await _db.ClassroomStudents.CountAsync(cs => cs.ClassroomId == classroomId);
            Assert.Equal(2, count);
        }

        // ── Unimplemented methods throw NotImplementedException ───────────────

        [Fact]
        public async Task AddTeacherAsync_ThrowsNotImplementedException()
        {
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _service.AddTeacherAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task InviteStudentAsync_ThrowsNotImplementedException()
        {
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _service.InviteStudentAsync(Guid.NewGuid(), "test@test.com"));
        }

        [Fact]
        public async Task RemoveStudentAsync_ThrowsNotImplementedException()
        {
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _service.RemoveStudentAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        [Fact]
        public async Task RemoveTeacherAsync_ThrowsNotImplementedException()
        {
            await Assert.ThrowsAsync<NotImplementedException>(
                () => _service.RemoveTeacherAsync(Guid.NewGuid(), Guid.NewGuid()));
        }
    }
}
