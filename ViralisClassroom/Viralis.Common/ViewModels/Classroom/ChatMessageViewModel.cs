namespace Viralis.Common.ViewModels.Classroom
{
    public class ChatMessageViewModel
    {
        public Guid Id { get; set; }
        public string SenderEmail { get; set; } = null!;
        public string SenderInitial { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string SentAt { get; set; } = null!;
        public bool IsOwn { get; set; }
    }
}
