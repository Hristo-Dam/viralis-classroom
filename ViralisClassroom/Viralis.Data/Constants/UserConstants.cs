namespace Viralis.Data.Constants
{
    public class UserConstants
    {
        public static readonly Guid Id = Guid.NewGuid();
        public const string UserName = "seededUser@email.com";
        public const string Email = "seededUser@email.com";
        public const string PhoneNumber = "+676767676767";
        public const string ConcurrencyStamp = "a1b2c3d4-e5f6-7890-abcd-ef1234567890";

        public static readonly Guid AdminId = Guid.NewGuid();
        public const string AdminUserName = "admin@admin.com";
        public const string AdminEmail = "admin@admin.com";
        public const string AdminPhoneNumber = "+000000000000";
        public const string AdminConcurrencyStamp = "0f9e8d7c-6b5a-4321-0fed-cba987654321";

        public const string DefaultPassword = "Dev@123";

        // Demo seeded users
        public const string Student1Email = "seededstudent1@demo.com";
        public const string Student1UserName = "SeededStudent1";
        public const string Student1ConcurrencyStamp = "11111111-1111-1111-1111-111111111111";

        public const string Student2Email = "seededstudent2@demo.com";
        public const string Student2UserName = "SeededStudent2";
        public const string Student2ConcurrencyStamp = "22222222-2222-2222-2222-222222222222";

        public const string Student3Email = "seededstudent3@demo.com";
        public const string Student3UserName = "SeededStudent3";
        public const string Student3ConcurrencyStamp = "33333333-3333-3333-3333-333333333333";

        public const string TeacherEmail = "seededteacher@demo.com";
        public const string TeacherUserName = "SeededTeacher";
        public const string TeacherConcurrencyStamp = "44444444-4444-4444-4444-444444444444";

        public const string SchoolAdminEmail = "seededschooladmin@demo.com";
        public const string SchoolAdminUserName = "SeededSchoolAdmin";
        public const string SchoolAdminConcurrencyStamp = "55555555-5555-5555-5555-555555555555";

        // Demo classrooms
        public static readonly Guid Classroom1Id = new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        public static readonly Guid Classroom2Id = new Guid("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        public static readonly Guid Classroom3Id = new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc");

        // Demo school
        public static readonly Guid SchoolId = new Guid("dddddddd-dddd-dddd-dddd-dddddddddddd");

        // App demo users — Teachers
        public const string AppTeacher1Email = "appuserteacher1@mail.com";
        public const string AppTeacher1UserName = "AppUserTeacher1";
        public const string AppTeacher1ConcurrencyStamp = "a1a1a1a1-a1a1-a1a1-a1a1-a1a1a1a1a1a1";

        public const string AppTeacher2Email = "appuserteacher2@mail.com";
        public const string AppTeacher2UserName = "AppUserTeacher2";
        public const string AppTeacher2ConcurrencyStamp = "a2a2a2a2-a2a2-a2a2-a2a2-a2a2a2a2a2a2";

        // App demo users — Students
        public const string AppStudent1Email = "appuserstudent1@mail.com";
        public const string AppStudent1UserName = "AppUserStudent1";
        public const string AppStudent1ConcurrencyStamp = "b1b1b1b1-b1b1-b1b1-b1b1-b1b1b1b1b1b1";

        public const string AppStudent2Email = "appuserstudent2@mail.com";
        public const string AppStudent2UserName = "AppUserStudent2";
        public const string AppStudent2ConcurrencyStamp = "b2b2b2b2-b2b2-b2b2-b2b2-b2b2b2b2b2b2";

        public const string AppStudent3Email = "appuserstudent3@mail.com";
        public const string AppStudent3UserName = "AppUserStudent3";
        public const string AppStudent3ConcurrencyStamp = "b3b3b3b3-b3b3-b3b3-b3b3-b3b3b3b3b3b3";

        public const string AppStudent4Email = "appuserstudent4@mail.com";
        public const string AppStudent4UserName = "AppUserStudent4";
        public const string AppStudent4ConcurrencyStamp = "b4b4b4b4-b4b4-b4b4-b4b4-b4b4b4b4b4b4";

        public const string AppStudent5Email = "appuserstudent5@mail.com";
        public const string AppStudent5UserName = "AppUserStudent5";
        public const string AppStudent5ConcurrencyStamp = "b5b5b5b5-b5b5-b5b5-b5b5-b5b5b5b5b5b5";

        // App demo users — School Admin
        public const string AppSchoolAdmin1Email = "appuserschooladmin1@mail.com";
        public const string AppSchoolAdmin1UserName = "AppUserSchoolAdmin1";
        public const string AppSchoolAdmin1ConcurrencyStamp = "c1c1c1c1-c1c1-c1c1-c1c1-c1c1c1c1c1c1";

        // App demo classrooms
        public static readonly Guid AppClassroom1Id = new Guid("ee000001-0000-0000-0000-000000000000");
        public static readonly Guid AppClassroom2Id = new Guid("ee000002-0000-0000-0000-000000000000");
        public static readonly Guid AppClassroom3Id = new Guid("ee000003-0000-0000-0000-000000000000");

        // App demo school
        public static readonly Guid AppSchoolId = new Guid("ff000001-0000-0000-0000-000000000000");
    }
}
