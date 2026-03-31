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

        public const string DefaultPassword = "123456";

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
    }
}
