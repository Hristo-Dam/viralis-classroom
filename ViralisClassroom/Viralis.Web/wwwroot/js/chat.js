const classroomId = window.chatConfig.classroomId;
const currentUserEmail = window.chatConfig.currentUserEmail;
const chatMessages = document.getElementById('chatMessages');
const chatInput = document.getElementById('chatInput');
const chatSend = document.getElementById('chatSend');

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

function appendMessage(initial, email, content, time, isOwn) {
    const div = document.createElement('div');
    div.className = `chat-message ${isOwn ? 'own' : ''}`;
    div.innerHTML = `
        ${!isOwn ? `<div class="chat-avatar">${initial}</div>` : ''}
        <div class="chat-bubble-wrap">
            ${!isOwn ? `<span class="chat-sender">${email}</span>` : ''}
            <div class="chat-bubble">${escapeHtml(content)}</div>
            <span class="chat-time">${time}</span>
        </div>
    `;
    chatMessages.appendChild(div);
}

function escapeHtml(text) {
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

    try {
        await connection.invoke('SendMessage', classroomId, content);
    } catch (err) {
        console.error('Send failed:', err);
    }
}

chatSend.addEventListener('click', sendMessage);

chatInput.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        sendMessage();
    }
});

chatInput.addEventListener('input', function () {
    this.style.height = 'auto';
    this.style.height = Math.min(this.scrollHeight, 120) + 'px';
});

connection.start()
    .then(() => connection.invoke('JoinClassroom', classroomId))
    .then(() => scrollToBottom())
    .catch(err => console.error('SignalR connection failed:', err));