using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Viralis.Data;
using Viralis.Data.Models;

namespace Viralis.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly ApplicationDbContext _db;

        public ChatHub(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task JoinClassroom(string classroomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, classroomId);
        }

        public async Task LeaveClassroom(string classroomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, classroomId);
        }

        public async Task SendMessage(string classroomId, string content)
        {
            if (string.IsNullOrWhiteSpace(content) || content.Length > 750)
                return;

            var userIdStr = Context.UserIdentifier;
            if (!Guid.TryParse(userIdStr, out var userId))
                return;

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;

            var message = new Message
            {
                ClassroomId = Guid.Parse(classroomId),
                SenderId = userId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow
            };

            _db.Messages.Add(message);
            await _db.SaveChangesAsync();

            // Broadcast to everyone in the classroom group
            await Clients.Group(classroomId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderEmail = user.Email,
                senderInitial = user.Email!.Substring(0, 1).ToUpper(),
                content = message.Content,
                sentAt = message.SentAt.ToString("HH:mm"),
                isOwn = false // client will determine this
            });
        }
    }
}
