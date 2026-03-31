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
    }
}
