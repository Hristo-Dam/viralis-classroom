const config = {
    iceServers: [{ urls: 'stun:stun.l.google.com:19302' }]
};

let pc; // RTCPeerConnection
let localStream;

// --- Initiate a call ---
document.getElementById('startCallBtn').addEventListener('click', async () => {
    localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
    document.getElementById('localVideo').srcObject = localStream;
    document.getElementById('videoCallPanel').style.display = 'block';

    pc = createPeerConnection();
    localStream.getTracks().forEach(track => pc.addTrack(track, localStream));

    const offer = await pc.createOffer();
    await pc.setLocalDescription(offer);

    await connection.invoke('SendOffer', classroomId, JSON.stringify(offer));
});

// --- Receive an offer (callee side) ---
connection.on('ReceiveOffer', async (callerId, sdp) => {
    localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
    document.getElementById('localVideo').srcObject = localStream;
    document.getElementById('videoCallPanel').style.display = 'block';

    pc = createPeerConnection();
    localStream.getTracks().forEach(track => pc.addTrack(track, localStream));

    await pc.setRemoteDescription(JSON.parse(sdp));
    const answer = await pc.createAnswer();
    await pc.setLocalDescription(answer);

    await connection.invoke('SendAnswer', classroomId, JSON.stringify(answer));
});

// --- Receive an answer (caller side) ---
connection.on('ReceiveAnswer', async (sdp) => {
    await pc.setRemoteDescription(JSON.parse(sdp));
});

// --- Exchange ICE candidates ---
connection.on('ReceiveIceCandidate', async (candidate) => {
    await pc.addIceCandidate(JSON.parse(candidate));
});

// --- Helper: create the RTCPeerConnection ---
function createPeerConnection() {
    const conn = new RTCPeerConnection(config);

    conn.onicecandidate = async (e) => {
        if (e.candidate) {
            await connection.invoke('SendIceCandidate', classroomId, JSON.stringify(e.candidate));
        }
    };

    conn.ontrack = (e) => {
        document.getElementById('remoteVideo').srcObject = e.streams[0];
    };

    return conn;
}

// --- End call ---
document.getElementById('endCallBtn').addEventListener('click', () => {
    pc?.close();
    localStream?.getTracks().forEach(t => t.stop());
    document.getElementById('videoCallPanel').style.display = 'none';
});