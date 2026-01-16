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
    }
}
