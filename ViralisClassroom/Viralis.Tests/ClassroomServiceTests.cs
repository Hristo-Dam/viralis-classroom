using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Viralis.Common.Constants;
using Viralis.Common.ViewModels.Classroom;
using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Implementations;

namespace Viralis.Tests
{
    public class ClassroomServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly ClassroomService _service;

        public ClassroomServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);
            _service = new ClassroomService(_db);
        }

        public void Dispose() => _db.Dispose();

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<(Guid roleId, Guid userId)> SeedTeacherAsync()
        {
            var role = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = RoleConstants.TEACHER, NormalizedName = RoleConstants.TEACHER.ToUpper() };
            _db.Roles.Add(role);

            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = "teacher@test.com", UserName = "teacher@test.com" };
            _db.Users.Add(user);

            _db.UserRoles.Add(new IdentityUserRole<Guid> { UserId = user.Id, RoleId = role.Id });

            await _db.SaveChangesAsync();
            return (role.Id, user.Id);
        }

        private async Task<Guid> SeedStudentAsync(string email = "student@test.com")
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = email, UserName = email };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user.Id;
        }

        private async Task<Guid> SeedClassroomAsync(Guid teacherId, string joinCode = "ABC123")
        {
            var classroom = new Classroom { Id = Guid.NewGuid(), Name = "Math 101", Subject = "Math", JoinCode = joinCode };
            classroom.Teachers.Add(new ClassroomTeacher { TeacherId = teacherId, IsOwner = true });
            _db.Classrooms.Add(classroom);
            await _db.SaveChangesAsync();
            return classroom.Id;
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_TeacherRoleNotFound_ThrowsInvalidOperation()
        {
            // No roles seeded
            var model = new CreateClassroomViewModel { Name = "Class A", Subject = "Science" };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAsync_UserIsNotTeacher_ThrowsUnauthorized()
        {
            var role = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = RoleConstants.TEACHER, NormalizedName = RoleConstants.TEACHER.ToUpper() };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync();

            var model = new CreateClassroomViewModel { Name = "Class A", Subject = "Science" };

            await Assert.ThrowsAsync<UnauthorizedAccessException>(
                () => _service.CreateAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task CreateAsync_ValidTeacher_CreatesClassroom()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var model = new CreateClassroomViewModel { Name = "Physics 101", Subject = "Physics" };

            await _service.CreateAsync(model, teacherId);

            var classroom = await _db.Classrooms.Include(c => c.Teachers).FirstOrDefaultAsync();
            Assert.NotNull(classroom);
            Assert.Equal("Physics 101", classroom.Name);
            Assert.Equal("Physics", classroom.Subject);
            Assert.NotEmpty(classroom.JoinCode);
            Assert.Single(classroom.Teachers);
            Assert.True(classroom.Teachers.First().IsOwner);
        }

        [Fact]
        public async Task CreateAsync_ValidTeacher_GeneratesUniqueJoinCode()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var model1 = new CreateClassroomViewModel { Name = "Class 1", Subject = "Math" };
            var model2 = new CreateClassroomViewModel { Name = "Class 2", Subject = "Math" };

            await _service.CreateAsync(model1, teacherId);
            await _service.CreateAsync(model2, teacherId);

            var classrooms = await _db.Classrooms.ToListAsync();
            Assert.Equal(2, classrooms.Count);
            Assert.NotEqual(classrooms[0].JoinCode, classrooms[1].JoinCode);
        }

        // ── GetUserClassroomsAsync ────────────────────────────────────────────

        [Fact]
        public async Task GetUserClassroomsAsync_TeacherWithClassrooms_ReturnsTeacherClassrooms()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            await SeedClassroomAsync(teacherId);

            var result = (await _service.GetUserClassroomsAsync(teacherId)).ToList();

            Assert.Single(result);
            Assert.True(result[0].IsTeacher);
            Assert.Equal("Math 101", result[0].Name);
        }

        [Fact]
        public async Task GetUserClassroomsAsync_StudentInClassroom_ReturnsStudentClassrooms()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var studentId = await SeedStudentAsync();
            var classroomId = await SeedClassroomAsync(teacherId);

            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroomId, StudentId = studentId });
            await _db.SaveChangesAsync();

            var result = (await _service.GetUserClassroomsAsync(studentId)).ToList();

            Assert.Single(result);
            Assert.False(result[0].IsTeacher);
        }

        [Fact]
        public async Task GetUserClassroomsAsync_NoClassrooms_ReturnsEmpty()
        {
            var userId = Guid.NewGuid();

            var result = await _service.GetUserClassroomsAsync(userId);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserClassroomsAsync_ResultsOrderedByName()
        {
            var (_, teacherId) = await SeedTeacherAsync();

            var c1 = new Classroom { Id = Guid.NewGuid(), Name = "Zebra Class", Subject = "Z", JoinCode = "ZZZ111" };
            c1.Teachers.Add(new ClassroomTeacher { TeacherId = teacherId, IsOwner = true });
            var c2 = new Classroom { Id = Guid.NewGuid(), Name = "Alpha Class", Subject = "A", JoinCode = "AAA111" };
            c2.Teachers.Add(new ClassroomTeacher { TeacherId = teacherId, IsOwner = true });
            _db.Classrooms.AddRange(c1, c2);
            await _db.SaveChangesAsync();

            var result = (await _service.GetUserClassroomsAsync(teacherId)).ToList();

            Assert.Equal("Alpha Class", result[0].Name);
            Assert.Equal("Zebra Class", result[1].Name);
        }

        // ── JoinByCodeAsync ───────────────────────────────────────────────────

        [Fact]
        public async Task JoinByCodeAsync_InvalidCode_ThrowsArgumentException()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.JoinByCodeAsync("INVALID", Guid.NewGuid()));
        }

        [Fact]
        public async Task JoinByCodeAsync_AlreadyJoined_ThrowsInvalidOperation()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var studentId = await SeedStudentAsync();
            var classroomId = await SeedClassroomAsync(teacherId, "XYZ789");

            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroomId, StudentId = studentId });
            await _db.SaveChangesAsync();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.JoinByCodeAsync("XYZ789", studentId));
        }

        [Fact]
        public async Task JoinByCodeAsync_ValidCode_AddsStudent()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var studentId = await SeedStudentAsync();
            await SeedClassroomAsync(teacherId, "JOIN01");

            await _service.JoinByCodeAsync("JOIN01", studentId);

            var joined = await _db.ClassroomStudents.AnyAsync(cs => cs.StudentId == studentId);
            Assert.True(joined);
        }

        // ── GetDetailAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetDetailAsync_ClassroomNotFound_ReturnsNull()
        {
            var result = await _service.GetDetailAsync(Guid.NewGuid(), Guid.NewGuid());
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailAsync_UserNotMember_ReturnsNull()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var classroomId = await SeedClassroomAsync(teacherId);
            var randomUserId = Guid.NewGuid();

            var result = await _service.GetDetailAsync(classroomId, randomUserId);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailAsync_TeacherAccess_ReturnsDetailWithIsTeacherTrue()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var classroomId = await SeedClassroomAsync(teacherId);

            var result = await _service.GetDetailAsync(classroomId, teacherId);

            Assert.NotNull(result);
            Assert.True(result.IsTeacher);
            Assert.Equal("Math 101", result.Name);
        }

        [Fact]
        public async Task GetDetailAsync_StudentAccess_ReturnsDetailWithIsTeacherFalse()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var studentId = await SeedStudentAsync();
            var classroomId = await SeedClassroomAsync(teacherId);

            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroomId, StudentId = studentId });
            await _db.SaveChangesAsync();

            var result = await _service.GetDetailAsync(classroomId, studentId);

            Assert.NotNull(result);
            Assert.False(result.IsTeacher);
        }

        [Fact]
        public async Task GetDetailAsync_ReturnsCorrectMemberLists()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var studentId = await SeedStudentAsync("student2@test.com");
            var classroomId = await SeedClassroomAsync(teacherId);

            _db.ClassroomStudents.Add(new ClassroomStudent { ClassroomId = classroomId, StudentId = studentId });
            await _db.SaveChangesAsync();

            var result = await _service.GetDetailAsync(classroomId, teacherId);

            Assert.NotNull(result);
            Assert.Single(result.Teachers);
            Assert.Single(result.Students);
        }

        [Fact]
        public async Task GetDetailAsync_IncludesJoinCode_WhenTeacher()
        {
            var (_, teacherId) = await SeedTeacherAsync();
            var classroomId = await SeedClassroomAsync(teacherId, "SECRET");

            var result = await _service.GetDetailAsync(classroomId, teacherId);

            Assert.NotNull(result);
            Assert.Equal("SECRET", result.JoinCode);
        }
    }
}
