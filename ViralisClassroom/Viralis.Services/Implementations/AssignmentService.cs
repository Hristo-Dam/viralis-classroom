using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Viralis.Common.ViewModels.Assignment;
using Viralis.Data;
using Viralis.Data.Models;
using Viralis.Services.Interfaces.Assignments;

namespace Viralis.Services.Implementations
{
    public class AssignmentService : IAssignmentService
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AssignmentService(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        public async Task CreateAsync(CreateAssignmentViewModel model, Guid teacherId)
        {
            var studentIds = await _db.ClassroomStudents
               .Where(cs => cs.ClassroomId == model.ClassroomId)
               .Select(cs => cs.StudentId)
               .ToListAsync();

            var assignment = new Assignment
            {
                ClassroomId = model.ClassroomId,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                AssigningTeacherId = teacherId
            };

            foreach (var studentId in studentIds)
            {
                assignment.AssignedStudents.Add(new UserAssignment
                {
                    StudentId = studentId
                });
            }

            // Handle files BEFORE saving — EF will handle the foreign key automatically
            if (model.Files != null && model.Files.Any())
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "assignments");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in model.Files)
                {
                    if (file.Length == 0) continue;

                    var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    assignment.Files.Add(new AssignmentFile
                    {
                        FileName = file.FileName,
                        FilePath = $"/uploads/assignments/{uniqueName}",
                        FileSize = file.Length
                    });
                }
            }

            _db.Assignments.Add(assignment);
            await _db.SaveChangesAsync(); // single save for everything
        }

        public async Task<List<AssignmentListViewModel>> GetForClassroomAsync(Guid classroomId, Guid userId, bool isTeacher)
        {
            var assignments = await _db.Assignments
                .Where(a => a.ClassroomId == classroomId)
                .Include(a => a.AssignedStudents)
                    .ThenInclude(ua => ua.Submission)
                .OrderByDescending(a => a.DueDate)
                .ToListAsync();

            return assignments.Select(a => new AssignmentListViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Description = a.Description,
                DueDate = a.DueDate,
                HasSubmitted = !isTeacher && a.AssignedStudents
                    .Any(ua => ua.StudentId == userId && ua.Submission != null),
                SubmissionCount = isTeacher
                    ? a.AssignedStudents.Count(ua => ua.Submission != null)
                    : 0
            }).ToList();
        }

        public async Task<AssignmentDetailViewModel?> GetDetailAsync(Guid assignmentId, Guid userId, bool isTeacher)
        {
            var assignment = await _db.Assignments
                .Include(a => a.Files)
                .Include(a => a.AssignedStudents)
                    .ThenInclude(ua => ua.Student)
                .Include(a => a.AssignedStudents)
                    .ThenInclude(ua => ua.Submission)
                        .ThenInclude(s => s!.Files)
                .FirstOrDefaultAsync(a => a.Id == assignmentId);

            if (assignment == null) return null;

            var model = new AssignmentDetailViewModel
            {
                Id = assignment.Id,
                ClassroomId = assignment.ClassroomId,
                Title = assignment.Title,
                Description = assignment.Description,
                DueDate = assignment.DueDate,
                IsTeacher = isTeacher,
                AssignmentFiles = assignment.Files.Select(f => new SubmissionFileViewModel  // add this
                {
                    FileName = f.FileName,
                    FilePath = f.FilePath
                }).ToList()
            };

            if (isTeacher)
            {
                model.Submissions = assignment.AssignedStudents.Select(ua =>
                    new StudentSubmissionViewModel
                    {
                        StudentId = ua.StudentId,
                        StudentEmail = ua.Student.Email!,
                        HasSubmitted = ua.Submission != null,
                        SubmissionId = ua.Submission?.Id,
                        TextContent = ua.Submission?.TextContent,
                        Grade = ua.Submission?.Grade,
                        Feedback = ua.Submission?.TeacherFeedback,
                        SubmittedOn = ua.Submission?.SubmittedOn,
                        Files = ua.Submission?.Files.Select(f => new SubmissionFileViewModel
                        {
                            FileName = f.FileName,
                            FilePath = f.FilePath
                        }).ToList() ?? new()
                    }).ToList();
            }
            else
            {
                var ua = assignment.AssignedStudents
                    .FirstOrDefault(ua => ua.StudentId == userId);

                if (ua?.Submission != null)
                {
                    model.MySubmission = new SubmissionViewModel
                    {
                        Id = ua.Submission.Id,
                        TextContent = ua.Submission.TextContent,
                        FileNames = ua.Submission.Files.Select(f => f.FileName).ToList(),
                        Grade = ua.Submission.Grade,
                        TeacherFeedback = ua.Submission.TeacherFeedback,
                        SubmittedOn = ua.Submission.SubmittedOn
                    };
                }
            }

            return model;
        }

        public async Task SubmitAsync(SubmitAssignmentViewModel model, Guid studentId)
        {
            bool hasComment = !string.IsNullOrWhiteSpace(model.TextContent);
            bool hasFiles = model.Files != null && model.Files.Any(f => f.Length > 0);

            if (!hasComment && !hasFiles)
                throw new InvalidOperationException("Please add a comment or attach at least one file.");

            var ua = await _db.UserAssignments
                .Include(ua => ua.Submission)
                .FirstOrDefaultAsync(ua =>
                    ua.AssignmentId == model.AssignmentId &&
                    ua.StudentId == studentId);

            if (ua == null)
                throw new InvalidOperationException("Assignment not found for this student.");

            // Check if already submitted
            bool alreadySubmitted = await _db.Submissions
                .AnyAsync(s => s.AssignmentId == model.AssignmentId && s.StudentId == studentId);

            if (alreadySubmitted)
                throw new InvalidOperationException("You have already submitted this assignment.");

            //if (ua.Submission != null)
            //    throw new InvalidOperationException("Already submitted.");

            var submission = new Submission
            {
                AssignmentId = model.AssignmentId,
                StudentId = studentId,
                TextContent = model.TextContent
            };

            // Handle file uploads
            if (hasFiles)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "submissions");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in model.Files!)
                {
                    if (file.Length == 0) continue;

                    var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    submission.Files.Add(new SubmissionFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = file.FileName,
                        FilePath = $"/uploads/submissions/{uniqueName}",
                        FileSize = file.Length
                    });
                }
            }

            // Add directly to DbContext instead of through navigation property
            _db.Submissions.Add(submission);
            await _db.SaveChangesAsync();
        }

        public async Task GradeAsync(GradeSubmissionViewModel model, Guid teacherId)
        {
            var submission = await _db.Submissions
                .FirstOrDefaultAsync(s => s.Id == model.SubmissionId);

            if (submission == null)
                throw new InvalidOperationException("Submission not found.");

            // Clamp grade to 0-100 regardless of what was submitted
            submission.Grade = Math.Clamp(model.Grade, 0, 100);
            submission.TeacherFeedback = model.Feedback;

            await _db.SaveChangesAsync();
        }

        public async Task EditSubmissionAsync(SubmitAssignmentViewModel model, Guid studentId)
        {
            var submission = await _db.Submissions
                .Include(s => s.Files)
                .FirstOrDefaultAsync(s =>
                    s.AssignmentId == model.AssignmentId &&
                    s.StudentId == studentId);

            if (submission == null)
                throw new InvalidOperationException("Submission not found.");

            bool hasComment = !string.IsNullOrWhiteSpace(model.TextContent);
            bool hasFiles = model.Files != null && model.Files.Any(f => f.Length > 0);

            if (!hasComment && !hasFiles)
                throw new InvalidOperationException("Please add a comment or attach at least one file.");

            submission.TextContent = model.TextContent;
            submission.SubmittedOn = DateTime.UtcNow;

            // Add new files if provided
            if (hasFiles)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "submissions");
                Directory.CreateDirectory(uploadsFolder);

                foreach (var file in model.Files!)
                {
                    if (file.Length == 0) continue;

                    var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    var filePath = Path.Combine(uploadsFolder, uniqueName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    submission.Files.Add(new SubmissionFile
                    {
                        Id = Guid.NewGuid(),
                        FileName = file.FileName,
                        FilePath = $"/uploads/submissions/{uniqueName}",
                        FileSize = file.Length
                    });
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}
