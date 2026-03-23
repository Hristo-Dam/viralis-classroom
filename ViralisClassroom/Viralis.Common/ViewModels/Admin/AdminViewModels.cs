namespace Viralis.Common.ViewModels.Admin
{
    public class AdminUserViewModel
    {
        public Guid   Id       { get; set; }
        public string Email    { get; set; } = string.Empty;
        public string Role     { get; set; } = string.Empty;
        public bool   IsLocked { get; set; }
    }

    public class AdminStatsViewModel
    {
        public int TotalUsers      { get; set; }
        public int TotalClassrooms { get; set; }
        public int TotalTeachers   { get; set; }
        public int TotalStudents   { get; set; }
    }
}
