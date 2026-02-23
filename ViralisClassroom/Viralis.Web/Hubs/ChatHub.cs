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

        // ─── Classroom Group ──────────────────────────────────────────────────

        public async Task JoinClassroom(string classroomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, classroomId);
        }

        public async Task LeaveClassroom(string classroomId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, classroomId);
        }

        // ─── Chat ─────────────────────────────────────────────────────────────

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

            await Clients.Group(classroomId).SendAsync("ReceiveMessage", new
            {
                id = message.Id,
                senderEmail = user.Email,
                senderInitial = user.Email!.Substring(0, 1).ToUpper(),
                content = message.Content,
                sentAt = message.SentAt.ToString("HH:mm"),
                isOwn = false
            });
        }

        // ─── WebRTC Signaling ─────────────────────────────────────────────────
        //
        // These methods don't contain any WebRTC logic themselves — they are
        // pure relays. They receive a message from one peer and forward it to
        // everyone else in the same classroom group. The actual negotiation
        // happens in the browser via the RTCPeerConnection API.

        /// <summary>
        /// Called by a user who wants to start a video call.
        /// Notifies all other classroom members so they can choose to join.
        /// </summary>
        public async Task StartCall(string classroomId)
        {
            var userIdStr = Context.UserIdentifier;
            if (!Guid.TryParse(userIdStr, out var userId)) return;

            var user = await _db.Users.FindAsync(userId);
            if (user == null) return;

            // Tell everyone else in the room that this user is starting a call
            await Clients.OthersInGroup(classroomId).SendAsync("IncomingCall", new
            {
                callerId = userIdStr,
                callerEmail = user.Email,
                callerInitial = user.Email!.Substring(0, 1).ToUpper()
            });
        }

        /// <summary>
        /// The caller sends their SDP offer to all other peers.
        /// An SDP offer describes the caller's media capabilities (codecs, resolution, etc.).
        /// </summary>
        public async Task SendOffer(string classroomId, string sdp)
        {
            await Clients.OthersInGroup(classroomId).SendAsync("ReceiveOffer", Context.UserIdentifier, sdp);
        }

        /// <summary>
        /// The callee sends their SDP answer back.
        /// This completes the media capability negotiation between both peers.
        /// </summary>
        public async Task SendAnswer(string classroomId, string sdp)
        {
            await Clients.OthersInGroup(classroomId).SendAsync("ReceiveAnswer", sdp);
        }

        /// <summary>
        /// Both sides continuously send ICE candidates as they are discovered.
        /// ICE candidates are possible network paths (local IP, public IP via STUN, TURN relay).
        /// The browser tries each one until it finds a working connection.
        /// </summary>
        public async Task SendIceCandidate(string classroomId, string candidate)
        {
            await Clients.OthersInGroup(classroomId).SendAsync("ReceiveIceCandidate", candidate);
        }

        /// <summary>
        /// Notifies all peers that this user has ended the call and cleaned up.
        /// </summary>
        public async Task EndCall(string classroomId)
        {
            await Clients.OthersInGroup(classroomId).SendAsync("CallEnded");
        }
    }
}