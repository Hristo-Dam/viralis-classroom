using Viralis.Common.ViewModels.Assignment;

namespace Viralis.Common.ViewModels.Classroom
{
    public class ClassroomDetailViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public string JoinCode { get; set; } = null!;
        public bool IsTeacher { get; set; }
        public List<MemberViewModel> Teachers { get; set; } = new();
        public List<MemberViewModel> Students { get; set; } = new();
        public List<AssignmentListViewModel> Assignments { get; set; } = new();
        public List<ChatMessageViewModel> RecentMessages { get; set; } = new();
    }

    public class MemberViewModel
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
    }
}
