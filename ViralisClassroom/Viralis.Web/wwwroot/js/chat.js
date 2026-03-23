const classroomId      = window.chatConfig.classroomId;
const currentUserEmail = window.chatConfig.currentUserEmail;
const chatMessages     = document.getElementById('chatMessages');
const chatInput        = document.getElementById('chatInput');
const chatSend         = document.getElementById('chatSend');

const connection = new signalR.HubConnectionBuilder()
    .withUrl('/chatHub')
    .withAutomaticReconnect()
    .build();

connection.on('ReceiveMessage', function (msg) {
    const empty = document.getElementById('chatEmpty');
    if (empty) empty.remove();

    const isOwn = msg.senderEmail === currentUserEmail;
    appendMessage(msg.senderInitial, msg.senderEmail, msg.content, msg.sentAt, isOwn);
    scrollToBottom();
});

/**
 * Renders a flat discussion-board style message row:
 *
 *  [Avatar]  SenderEmail  ·  12:34
 *            Message text here...
 */
function appendMessage(initial, email, content, time, isOwn) {
    const div = document.createElement('div');
    div.className = `chat-message${isOwn ? ' own' : ''}`;
    div.innerHTML = `
        <div class="chat-avatar" title="${escapeHtml(email)}">${escapeHtml(initial)}</div>
        <div class="chat-bubble-wrap">
            <div class="chat-meta-row">
                <span class="chat-sender">${isOwn ? 'You' : escapeHtml(email)}</span>
                <span class="chat-time">${escapeHtml(time)}</span>
            </div>
            <div class="chat-bubble">${escapeHtml(content)}</div>
        </div>
    `;
    chatMessages.appendChild(div);
}

function escapeHtml(text) {
    if (typeof text !== 'string') return '';
    return text
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}

function scrollToBottom() {
    chatMessages.scrollTop = chatMessages.scrollHeight;
}

async function sendMessage() {
    const content = chatInput.value.trim();
    if (!content) return;

    chatInput.value = '';
    chatInput.style.height = 'auto';
    chatSend.disabled = true;

    try {
        await connection.invoke('SendMessage', classroomId, content);
    } catch (err) {
        console.error('[Chat] Send failed:', err);
    } finally {
        chatSend.disabled = false;
        chatInput.focus();
    }
}

chatSend.addEventListener('click', sendMessage);

chatInput.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

// Auto-grow textarea up to 120px
chatInput.addEventListener('input', function () {
    this.style.height = 'auto';
    this.style.height = Math.min(this.scrollHeight, 120) + 'px';
    chatSend.disabled = this.value.trim().length === 0;
});

// Disable send btn when empty on load
chatSend.disabled = true;

connection.start()
    .then(() => connection.invoke('JoinClassroom', classroomId))
    .then(() => scrollToBottom())
    .catch(err => console.error('[Chat] SignalR connection failed:', err));
