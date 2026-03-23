using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Moq;
using Viralis.Common.ViewModels.Assignment;
using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Implementations;

namespace Viralis.Tests
{
    public class AssignmentServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _db;
        private readonly Mock<IWebHostEnvironment> _envMock;
        private readonly AssignmentService _service;
        private readonly string _tempPath;

        public AssignmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new ApplicationDbContext(options);

            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempPath);

            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns(_tempPath);

            _service = new AssignmentService(_db, _envMock.Object);
        }

        public void Dispose()
        {
            _db.Dispose();
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, recursive: true);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private async Task<Guid> SeedUserAsync(string email = "user@test.com")
        {
            var user = new ApplicationUser { Id = Guid.NewGuid(), Email = email, UserName = email };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user.Id;
        }

        private async Task<(Guid classroomId, Guid assignmentId, Guid studentId)> SeedAssignmentWithStudentAsync()
        {
            var teacherId = await SeedUserAsync("teacher@test.com");
            var studentId = await SeedUserAsync("student@test.com");

            var classroom = new Classroom { Id = Guid.NewGuid(), Name = "Test Class", Subject = "Test", JoinCode = "TST001" };
            classroom.Teachers.Add(new ClassroomTeacher { TeacherId = teacherId, IsOwner = true });
            classroom.Students.Add(new ClassroomStudent { StudentId = studentId });
            _db.Classrooms.Add(classroom);
            await _db.SaveChangesAsync();

            var assignment = new Assignment
            {
                Id = Guid.NewGuid(),
                ClassroomId = classroom.Id,
                Title = "Test Assignment",
                Description = "Do the work",
                DueDate = DateTime.UtcNow.AddDays(7),
                AssigningTeacherId = teacherId
            };
            assignment.AssignedStudents.Add(new UserAssignment { StudentId = studentId });
            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync();

            return (classroom.Id, assignment.Id, studentId);
        }

        private static Mock<IFormFile> MakeFormFile(string filename = "test.txt", long size = 100)
        {
            var fileMock = new Mock<IFormFile>();
            var content = new byte[size];
            var stream = new MemoryStream(content);
            fileMock.Setup(f => f.FileName).Returns(filename);
            fileMock.Setup(f => f.Length).Returns(size);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns((Stream target, CancellationToken _) => stream.CopyToAsync(target));
            return fileMock;
        }

        // ── GetForClassroomAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetForClassroomAsync_NoAssignments_ReturnsEmptyList()
        {
            var result = await _service.GetForClassroomAsync(Guid.NewGuid(), Guid.NewGuid(), false);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetForClassroomAsync_AsTeacher_ReturnsSubmissionCount()
        {
            var (classroomId, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();

            _db.Submissions.Add(new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                TextContent = "Done"
            });
            await _db.SaveChangesAsync();

            var result = await _service.GetForClassroomAsync(classroomId, Guid.NewGuid(), isTeacher: true);

            Assert.Single(result);
            Assert.Equal("Test Assignment", result[0].Title);
            Assert.Equal(1, result[0].SubmissionCount);
        }

        [Fact]
        public async Task GetForClassroomAsync_AsStudent_HasSubmittedFalse_WhenNoSubmission()
        {
            var (classroomId, _, studentId) = await SeedAssignmentWithStudentAsync();

            var result = await _service.GetForClassroomAsync(classroomId, studentId, isTeacher: false);

            Assert.Single(result);
            Assert.False(result[0].HasSubmitted);
        }

        [Fact]
        public async Task GetForClassroomAsync_AsTeacher_SubmissionCountIsZero_WhenNoSubmissions()
        {
            var (classroomId, _, _) = await SeedAssignmentWithStudentAsync();

            var result = await _service.GetForClassroomAsync(classroomId, Guid.NewGuid(), isTeacher: true);

            Assert.Single(result);
            Assert.Equal(0, result[0].SubmissionCount);
        }

        // ── GetDetailAsync ────────────────────────────────────────────────────

        [Fact]
        public async Task GetDetailAsync_NotFound_ReturnsNull()
        {
            var result = await _service.GetDetailAsync(Guid.NewGuid(), Guid.NewGuid(), false);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetDetailAsync_AsTeacher_ReturnsSubmissionsList()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var teacherId = await _db.ClassroomTeachers
                .Select(ct => ct.TeacherId)
                .FirstAsync();

            var result = await _service.GetDetailAsync(assignmentId, teacherId, isTeacher: true);

            Assert.NotNull(result);
            Assert.True(result.IsTeacher);
            Assert.NotNull(result.Submissions);
            Assert.Single(result.Submissions!);
        }

        [Fact]
        public async Task GetDetailAsync_AsStudent_NoSubmission_MySubmissionIsNull()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();

            var result = await _service.GetDetailAsync(assignmentId, studentId, isTeacher: false);

            Assert.NotNull(result);
            Assert.False(result.IsTeacher);
            Assert.Null(result.MySubmission);
        }

        [Fact]
        public async Task GetDetailAsync_ReturnsCorrectTitle()
        {
            var (_, assignmentId, _) = await SeedAssignmentWithStudentAsync();

            var result = await _service.GetDetailAsync(assignmentId, Guid.NewGuid(), isTeacher: true);

            Assert.NotNull(result);
            Assert.Equal("Test Assignment", result.Title);
        }

        // ── SubmitAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task SubmitAsync_NoContentAndNoFiles_ThrowsInvalidOperation()
        {
            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = Guid.NewGuid(),
                TextContent = null,
                Files = null
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SubmitAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task SubmitAsync_NoContentWhitespaceAndNoFiles_ThrowsInvalidOperation()
        {
            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = Guid.NewGuid(),
                TextContent = "   ",
                Files = null
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SubmitAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task SubmitAsync_UserAssignmentNotFound_ThrowsInvalidOperation()
        {
            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = Guid.NewGuid(),
                TextContent = "My answer"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SubmitAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task SubmitAsync_AlreadySubmitted_ThrowsInvalidOperation()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();

            _db.Submissions.Add(new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                TextContent = "First submission"
            });
            await _db.SaveChangesAsync();

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignmentId,
                TextContent = "Second attempt"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SubmitAsync(model, studentId));
        }

        [Fact]
        public async Task SubmitAsync_ValidTextSubmission_CreatesSubmission()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignmentId,
                TextContent = "My answer to the assignment"
            };

            await _service.SubmitAsync(model, studentId);

            var submission = await _db.Submissions.FirstOrDefaultAsync(
                s => s.AssignmentId == assignmentId && s.StudentId == studentId);
            Assert.NotNull(submission);
            Assert.Equal("My answer to the assignment", submission.TextContent);
        }

        [Fact]
        public async Task SubmitAsync_WithFile_CreatesSubmissionWithFile()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();

            var fileMock = MakeFormFile("homework.txt");
            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignmentId,
                TextContent = null,
                Files = new List<IFormFile> { fileMock.Object }
            };

            await _service.SubmitAsync(model, studentId);

            var submission = await _db.Submissions
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);
            Assert.NotNull(submission);
            Assert.Single(submission.Files);
            Assert.Equal("homework.txt", submission.Files.First().FileName);
        }

        // ── GradeAsync ────────────────────────────────────────────────────────

        [Fact]
        public async Task GradeAsync_SubmissionNotFound_ThrowsInvalidOperation()
        {
            var model = new GradeSubmissionViewModel { SubmissionId = Guid.NewGuid(), Grade = 80 };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.GradeAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task GradeAsync_ValidGrade_SetsGradeAndFeedback()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var submission = new Submission { AssignmentId = assignmentId, StudentId = studentId, TextContent = "Done" };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            var model = new GradeSubmissionViewModel
            {
                SubmissionId = submission.Id,
                Grade = 92,
                Feedback = "Great work!"
            };

            await _service.GradeAsync(model, Guid.NewGuid());

            var updated = await _db.Submissions.FindAsync(submission.Id);
            Assert.NotNull(updated);
            Assert.Equal(92, updated.Grade);
            Assert.Equal("Great work!", updated.TeacherFeedback);
        }

        [Fact]
        public async Task GradeAsync_GradeAbove100_ClampsTo100()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var submission = new Submission { AssignmentId = assignmentId, StudentId = studentId, TextContent = "Done" };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            var model = new GradeSubmissionViewModel { SubmissionId = submission.Id, Grade = 150 };

            await _service.GradeAsync(model, Guid.NewGuid());

            var updated = await _db.Submissions.FindAsync(submission.Id);
            Assert.Equal(100, updated!.Grade);
        }

        [Fact]
        public async Task GradeAsync_GradeBelow0_ClampsTo0()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var submission = new Submission { AssignmentId = assignmentId, StudentId = studentId, TextContent = "Done" };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            var model = new GradeSubmissionViewModel { SubmissionId = submission.Id, Grade = -10 };

            await _service.GradeAsync(model, Guid.NewGuid());

            var updated = await _db.Submissions.FindAsync(submission.Id);
            Assert.Equal(0, updated!.Grade);
        }

        // ── EditSubmissionAsync ───────────────────────────────────────────────

        [Fact]
        public async Task EditSubmissionAsync_SubmissionNotFound_ThrowsInvalidOperation()
        {
            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = Guid.NewGuid(),
                TextContent = "Updated content"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.EditSubmissionAsync(model, Guid.NewGuid()));
        }

        [Fact]
        public async Task EditSubmissionAsync_NoContentAndNoFiles_ThrowsInvalidOperation()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var submission = new Submission { AssignmentId = assignmentId, StudentId = studentId, TextContent = "Old" };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignmentId,
                TextContent = null,
                Files = null
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.EditSubmissionAsync(model, studentId));
        }

        [Fact]
        public async Task EditSubmissionAsync_ValidTextContent_UpdatesSubmission()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var submission = new Submission { AssignmentId = assignmentId, StudentId = studentId, TextContent = "Old answer" };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignmentId,
                TextContent = "Updated answer"
            };

            await _service.EditSubmissionAsync(model, studentId);

            var updated = await _db.Submissions.FindAsync(submission.Id);
            Assert.NotNull(updated);
            Assert.Equal("Updated answer", updated.TextContent);
        }

        [Fact]
        public async Task EditSubmissionAsync_UpdatesSubmittedOnTimestamp()
        {
            var (_, assignmentId, studentId) = await SeedAssignmentWithStudentAsync();
            var originalTime = DateTime.UtcNow.AddDays(-1);
            var submission = new Submission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                TextContent = "Old",
                SubmittedOn = originalTime
            };
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();

            var model = new SubmitAssignmentViewModel
            {
                AssignmentId = assignmentId,
                TextContent = "Updated content"
            };

            await _service.EditSubmissionAsync(model, studentId);

            var updated = await _db.Submissions.FindAsync(submission.Id);
            Assert.NotNull(updated);
            Assert.True(updated.SubmittedOn > originalTime);
        }

        // ── CreateAsync ───────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ValidModel_CreatesAssignmentWithStudents()
        {
            var teacherId = await SeedUserAsync("teacher2@test.com");
            var studentId = await SeedUserAsync("student2@test.com");

            var classroom = new Classroom { Id = Guid.NewGuid(), Name = "Class", Subject = "S", JoinCode = "C00001" };
            classroom.Students.Add(new ClassroomStudent { StudentId = studentId });
            _db.Classrooms.Add(classroom);
            await _db.SaveChangesAsync();

            var model = new CreateAssignmentViewModel
            {
                ClassroomId = classroom.Id,
                Title = "HW 1",
                Description = "Do it",
                DueDate = DateTime.UtcNow.AddDays(5),
                Files = null
            };

            await _service.CreateAsync(model, teacherId);

            var assignment = await _db.Assignments
                .Include(a => a.AssignedStudents)
                .FirstOrDefaultAsync();

            Assert.NotNull(assignment);
            Assert.Equal("HW 1", assignment.Title);
            Assert.Single(assignment.AssignedStudents);
        }
    }
}
