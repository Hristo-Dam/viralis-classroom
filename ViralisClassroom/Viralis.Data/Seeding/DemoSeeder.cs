using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Viralis.Common.Constants;
using Viralis.Data.Constants;
using Viralis.Data.Models;

namespace Viralis.Data.Seeding
{
    public static class DemoSeeder
    {
        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            var student1 = await EnsureUser(userManager, UserConstants.Student1Email, UserConstants.Student1UserName, UserConstants.Student1ConcurrencyStamp);
            var student2 = await EnsureUser(userManager, UserConstants.Student2Email, UserConstants.Student2UserName, UserConstants.Student2ConcurrencyStamp);
            var student3 = await EnsureUser(userManager, UserConstants.Student3Email, UserConstants.Student3UserName, UserConstants.Student3ConcurrencyStamp);
            var teacher = await EnsureUser(userManager, UserConstants.TeacherEmail, UserConstants.TeacherUserName, UserConstants.TeacherConcurrencyStamp);
            var schoolAdmin = await EnsureUser(userManager, UserConstants.SchoolAdminEmail, UserConstants.SchoolAdminUserName, UserConstants.SchoolAdminConcurrencyStamp);

            await EnsureRole(userManager, student1, RoleConstants.STUDENT);
            await EnsureRole(userManager, student2, RoleConstants.STUDENT);
            await EnsureRole(userManager, student3, RoleConstants.STUDENT);
            await EnsureRole(userManager, teacher, RoleConstants.TEACHER);
            await EnsureRole(userManager, schoolAdmin, RoleConstants.SCHOOL_ADMINISTRATOR);

            await EnsureSchool(db, schoolAdmin);

            await EnsureClassroom(db, UserConstants.Classroom1Id, "Demo Classroom 1", "Mathematics", "JOIN01", teacher, student1);
            await EnsureClassroom(db, UserConstants.Classroom2Id, "Demo Classroom 2", "Science", "JOIN02", teacher, student2);
            await EnsureClassroom(db, UserConstants.Classroom3Id, "Demo Classroom 3", "History", "JOIN03", teacher, student3);

            await db.SaveChangesAsync();

            await SeedAppUsersAsync(userManager, db);
        }

        private static async Task SeedAppUsersAsync(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            var teacher1 = await EnsureUser(userManager, UserConstants.AppTeacher1Email, UserConstants.AppTeacher1UserName, UserConstants.AppTeacher1ConcurrencyStamp);
            var teacher2 = await EnsureUser(userManager, UserConstants.AppTeacher2Email, UserConstants.AppTeacher2UserName, UserConstants.AppTeacher2ConcurrencyStamp);
            var student1 = await EnsureUser(userManager, UserConstants.AppStudent1Email, UserConstants.AppStudent1UserName, UserConstants.AppStudent1ConcurrencyStamp);
            var student2 = await EnsureUser(userManager, UserConstants.AppStudent2Email, UserConstants.AppStudent2UserName, UserConstants.AppStudent2ConcurrencyStamp);
            var student3 = await EnsureUser(userManager, UserConstants.AppStudent3Email, UserConstants.AppStudent3UserName, UserConstants.AppStudent3ConcurrencyStamp);
            var student4 = await EnsureUser(userManager, UserConstants.AppStudent4Email, UserConstants.AppStudent4UserName, UserConstants.AppStudent4ConcurrencyStamp);
            var student5 = await EnsureUser(userManager, UserConstants.AppStudent5Email, UserConstants.AppStudent5UserName, UserConstants.AppStudent5ConcurrencyStamp);
            var schoolAdmin1 = await EnsureUser(userManager, UserConstants.AppSchoolAdmin1Email, UserConstants.AppSchoolAdmin1UserName, UserConstants.AppSchoolAdmin1ConcurrencyStamp);

            await EnsureRole(userManager, teacher1, RoleConstants.TEACHER);
            await EnsureRole(userManager, teacher2, RoleConstants.TEACHER);
            await EnsureRole(userManager, student1, RoleConstants.STUDENT);
            await EnsureRole(userManager, student2, RoleConstants.STUDENT);
            await EnsureRole(userManager, student3, RoleConstants.STUDENT);
            await EnsureRole(userManager, student4, RoleConstants.STUDENT);
            await EnsureRole(userManager, student5, RoleConstants.STUDENT);
            await EnsureRole(userManager, schoolAdmin1, RoleConstants.SCHOOL_ADMINISTRATOR);

            bool schoolExists = await db.SchoolAdministrators.AnyAsync(s => s.Id == UserConstants.AppSchoolId);
            if (!schoolExists)
            {
                db.SchoolAdministrators.Add(new SchoolAdministrator
                {
                    Id = UserConstants.AppSchoolId,
                    Name = "App Demo School",
                    SchoolAdminId = schoolAdmin1.Id,
                    Teachers = new List<ApplicationUser> { teacher1, teacher2 }
                });
            }

            // Algebra II  — teacher1; students 1, 2, 3
            await EnsureClassroomWithStudents(db, UserConstants.AppClassroom1Id, "Algebra II", "Mathematics", "JOIN11",
                teacher1, student1, student2, student3);

            // World History — teacher1; students 2, 4, 5  (student2 is in two classrooms)
            await EnsureClassroomWithStudents(db, UserConstants.AppClassroom2Id, "World History", "History", "JOIN12",
                teacher1, student2, student4, student5);

            // Biology — teacher2; students 1, 3, 4, 5
            await EnsureClassroomWithStudents(db, UserConstants.AppClassroom3Id, "Biology", "Science", "JOIN13",
                teacher2, student1, student3, student4, student5);

            await db.SaveChangesAsync();
        }

        private static async Task<ApplicationUser> EnsureUser(
            UserManager<ApplicationUser> userManager,
            string email,
            string userName,
            string concurrencyStamp)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = userName,
                    NormalizedUserName = userName.ToUpper(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    ConcurrencyStamp = concurrencyStamp,
                };
                await userManager.CreateAsync(user, UserConstants.DefaultPassword);
            }
            return user;
        }

        private static async Task EnsureRole(UserManager<ApplicationUser> userManager, ApplicationUser user, string role)
        {
            if (!await userManager.IsInRoleAsync(user, role))
            {
                await userManager.AddToRoleAsync(user, role);
            }
        }

        private static async Task EnsureSchool(ApplicationDbContext db, ApplicationUser schoolAdmin)
        {
            bool schoolExists = await db.SchoolAdministrators.AnyAsync(s => s.Id == UserConstants.SchoolId);
            if (!schoolExists)
            {
                db.SchoolAdministrators.Add(new SchoolAdministrator
                {
                    Id = UserConstants.SchoolId,
                    Name = "Demo School",
                    SchoolAdminId = schoolAdmin.Id,
                });
            }
        }

        private static async Task EnsureClassroom(
            ApplicationDbContext db,
            Guid classroomId,
            string name,
            string subject,
            string joinCode,
            ApplicationUser teacher,
            ApplicationUser student)
        {
            bool classroomExists = await db.Classrooms.AnyAsync(c => c.Id == classroomId);
            if (classroomExists)
            {
                return;
            }

            db.Classrooms.Add(new Classroom
            {
                Id = classroomId,
                Name = name,
                Subject = subject,
                JoinCode = joinCode,
                Teachers = new List<ClassroomTeacher>
                {
                    new ClassroomTeacher { TeacherId = teacher.Id, ClassroomId = classroomId, IsOwner = true }
                },
                Students = new List<ClassroomStudent>
                {
                    new ClassroomStudent { StudentId = student.Id, ClassroomId = classroomId }
                }
            });
        }

        private static async Task EnsureClassroomWithStudents(
            ApplicationDbContext db,
            Guid classroomId,
            string name,
            string subject,
            string joinCode,
            ApplicationUser teacher,
            params ApplicationUser[] students)
        {
            bool classroomExists = await db.Classrooms.AnyAsync(c => c.Id == classroomId);
            if (classroomExists) return;

            db.Classrooms.Add(new Classroom
            {
                Id = classroomId,
                Name = name,
                Subject = subject,
                JoinCode = joinCode,
                Teachers = new List<ClassroomTeacher>
                {
                    new ClassroomTeacher { TeacherId = teacher.Id, ClassroomId = classroomId, IsOwner = true }
                },
                Students = students
                    .Select(s => new ClassroomStudent { StudentId = s.Id, ClassroomId = classroomId })
                    .ToList()
            });
        }
    }
}
