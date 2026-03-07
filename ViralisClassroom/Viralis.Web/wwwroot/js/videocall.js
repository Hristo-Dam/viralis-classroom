// ─── videocall.js ─────────────────────────────────────────────────────────────
//
// This file handles all WebRTC video call logic for the classroom page.
// It reuses the SignalR `connection` object that chat.js already created,
// so both files must be loaded on the same page (chat.js first).
//
// HOW WEBRTC WORKS (quick reference):
//   1. Caller clicks "Start Call" → gets camera/mic → creates RTCPeerConnection
//   2. Caller creates an SDP "offer" (describes media capabilities) and sends
//      it to the others via SignalR (SendOffer hub method)
//   3. Callee receives the offer → gets their own camera/mic → creates answer
//      and sends it back via SignalR (SendAnswer hub method)
//   4. Both sides exchange ICE candidates (network path options) via SignalR
//   5. The browser picks the best ICE path and video flows peer-to-peer.
//      SignalR is no longer involved once the connection is established.

(function () {
    'use strict';

    // ── Config ───────────────────────────────────────────────────────────────
    // STUN server: helps each browser discover its public IP address.
    // Google's public STUN servers are free and reliable for development.
    // For production, also add a TURN server here (see README at the bottom).
    const RTC_CONFIG = {
        iceServers: [
            { urls: 'stun:stun.l.google.com:19302' },
            { urls: 'stun:stun1.l.google.com:19302' }
        ]
    };

    // ── State ────────────────────────────────────────────────────────────────
    let peerConnection = null;   // The RTCPeerConnection — one per call
    let localStream = null;      // Our camera + microphone MediaStream
    let isMuted = false;
    let isCameraOff = false;

    // classroomId and connection come from the page / chat.js
    const classroomId = window.chatConfig?.classroomId;

    // ── DOM references ───────────────────────────────────────────────────────
    const startCallBtn = document.getElementById('startCallBtn');
    const callPanel = document.getElementById('videoCallPanel');
    const localVideo = document.getElementById('localVideo');
    const remoteVideo = document.getElementById('remoteVideo');
    const endCallBtn = document.getElementById('endCallBtn');
    const muteBtn = document.getElementById('muteBtn');
    const cameraBtn = document.getElementById('cameraBtn');
    const incomingAlert = document.getElementById('incomingCallAlert');
    const incomingLabel = document.getElementById('incomingCallerLabel');
    const acceptCallBtn = document.getElementById('acceptCallBtn');
    const declineCallBtn = document.getElementById('declineCallBtn');
    const callStatus = document.getElementById('callStatus');

    // Guard: if the video UI elements aren't on this page, do nothing.
    if (!startCallBtn || !callPanel) return;

    // ═════════════════════════════════════════════════════════════════════════
    // SIGNALR EVENT HANDLERS
    // These are messages coming IN from other users via the hub.
    // ═════════════════════════════════════════════════════════════════════════

    // Someone else in the classroom started a call → show incoming call UI
    connection.on('IncomingCall', function (caller) {
        // Don't show the alert if we're already in a call
        if (callPanel.classList.contains('active')) return;

        incomingLabel.textContent = `${caller.callerEmail} is starting a video call`;
        incomingAlert.classList.add('active');

        // Auto-dismiss the alert after 30 seconds if not answered
        setTimeout(() => incomingAlert.classList.remove('active'), 30000);
    });

    // We received an SDP offer from the caller → we are the callee
    connection.on('ReceiveOffer', async function (callerId, sdp) {
        try {
            setCallStatus('Connecting...');

            // If we haven't joined yet (declined the alert but received offer anyway), ignore
            if (!localStream) return;

            createPeerConnection();

            // setRemoteDescription tells our RTCPeerConnection what the caller can do
            await peerConnection.setRemoteDescription(JSON.parse(sdp));

            // createAnswer generates our own SDP based on what the caller offered
            const answer = await peerConnection.createAnswer();

            // setLocalDescription registers our answer locally and starts ICE gathering
            await peerConnection.setLocalDescription(answer);

            // Send our answer to the caller via SignalR
            await connection.invoke('SendAnswer', classroomId, JSON.stringify(answer));
        } catch (err) {
            console.error('[WebRTC] Failed to handle offer:', err);
            setCallStatus('Connection failed');
        }
    });

    // We received an SDP answer from the callee → we are the caller
    connection.on('ReceiveAnswer', async function (sdp) {
        try {
            // This completes the SDP negotiation — both sides now know each other's capabilities
            await peerConnection.setRemoteDescription(JSON.parse(sdp));
        } catch (err) {
            console.error('[WebRTC] Failed to handle answer:', err);
        }
    });

    // We received an ICE candidate from the other peer
    // ICE candidates are possible network routes — the browser tests each one
    connection.on('ReceiveIceCandidate', async function (candidate) {
        try {
            if (peerConnection) {
                await peerConnection.addIceCandidate(JSON.parse(candidate));
            }
        } catch (err) {
            // This is often harmless — candidates can arrive in any order
            console.warn('[WebRTC] Failed to add ICE candidate:', err);
        }
    });

    // The other person ended the call
    connection.on('CallEnded', function () {
        setCallStatus('Call ended by the other participant');
        setTimeout(cleanupCall, 2000);
    });

    // ═════════════════════════════════════════════════════════════════════════
    // BUTTON HANDLERS — User-initiated actions
    // ═════════════════════════════════════════════════════════════════════════

    // "Start Call" button — the user wants to initiate a call
    startCallBtn.addEventListener('click', async function () {
        try {
            console.log('[WebRTC] Starting call...');
            await startLocalStream();
            showCallPanel();
            createPeerConnection();
            setCallStatus('Calling...');

            console.log('[WebRTC] Notifying classroom via SignalR...');
            await connection.invoke('StartCall', classroomId);

            // Create and send our SDP offer
            // createOffer() generates an SDP that describes our audio/video capabilities
            const offer = await peerConnection.createOffer();
            await peerConnection.setLocalDescription(offer);
            console.log('[WebRTC] Offer created and sent.');
            await connection.invoke('SendOffer', classroomId, JSON.stringify(offer));
        } catch (err) {
            console.error('[WebRTC] startCallBtn error:', err);
            handleMediaError(err);
        }
    });

    // "Accept" on the incoming call alert
    acceptCallBtn.addEventListener('click', async function () {
        incomingAlert.classList.remove('active');
        try {
            await startLocalStream();
            showCallPanel();
            setCallStatus('Joining call...');
            // The ReceiveOffer handler will complete the handshake
        } catch (err) {
            handleMediaError(err);
        }
    });

    // "Decline" on the incoming call alert
    declineCallBtn.addEventListener('click', function () {
        incomingAlert.classList.remove('active');
    });

    // "End Call" button
    endCallBtn.addEventListener('click', async function () {
        await connection.invoke('EndCall', classroomId);
        cleanupCall();
    });

    // "Mute/Unmute" toggle
    muteBtn.addEventListener('click', function () {
        if (!localStream) return;
        isMuted = !isMuted;
        localStream.getAudioTracks().forEach(track => track.enabled = !isMuted);
        muteBtn.textContent = isMuted ? '🔇 Unmute' : '🎤 Mute';
        muteBtn.classList.toggle('control-active', isMuted);
    });

    // "Camera on/off" toggle
    cameraBtn.addEventListener('click', function () {
        if (!localStream) return;
        isCameraOff = !isCameraOff;
        localStream.getVideoTracks().forEach(track => track.enabled = !isCameraOff);
        cameraBtn.textContent = isCameraOff ? '📷 Show Camera' : '📹 Hide Camera';
        cameraBtn.classList.toggle('control-active', isCameraOff);
    });

    // ═════════════════════════════════════════════════════════════════════════
    // CORE WEBRTC HELPERS
    // ═════════════════════════════════════════════════════════════════════════

    /// Asks the browser for camera and microphone access.
    /// Tries video+audio first, then falls back to video-only or audio-only
    /// so a missing microphone or camera doesn't block the whole call.
    async function startLocalStream() {
        console.log('[WebRTC] Requesting camera and microphone...');

        try {
            // Ideal: get both video and audio
            localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
            console.log('[WebRTC] Got video + audio stream.');
        } catch (firstErr) {
            console.warn('[WebRTC] Could not get video+audio, trying video-only.', firstErr);
            try {
                // Fallback: video only (no mic)
                localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: false });
                console.log('[WebRTC] Got video-only stream (no microphone available).');
                setCallStatus('No microphone detected — joining with video only.');
            } catch (secondErr) {
                console.warn('[WebRTC] Could not get video-only, trying audio-only.', secondErr);
                try {
                    // Last resort: audio only (no camera)
                    localStream = await navigator.mediaDevices.getUserMedia({ video: false, audio: true });
                    console.log('[WebRTC] Got audio-only stream (no camera available).');
                    setCallStatus('No camera detected — joining with audio only.');
                } catch (thirdErr) {
                    // Nothing worked — surface the original error
                    throw firstErr;
                }
            }
        }

        // Attach stream to the local video element
        localVideo.srcObject = localStream;

        // Some browsers need an explicit play() call — autoplay attribute alone isn't always enough
        try {
            await localVideo.play();
        } catch (playErr) {
            // play() can throw if the user hasn't interacted with the page yet,
            // but since this runs inside a click handler that's very unlikely.
            console.warn('[WebRTC] localVideo.play() was interrupted:', playErr);
        }

        console.log('[WebRTC] Local stream tracks:', localStream.getTracks().map(t => `${t.kind}(${t.label})`));
    }

    /// Creates the RTCPeerConnection and wires up its event handlers.
    /// This is the core WebRTC object — it manages the entire peer connection lifecycle.
    function createPeerConnection() {
        console.log('[WebRTC] Creating RTCPeerConnection...');
        peerConnection = new RTCPeerConnection(RTC_CONFIG);

        // Add our local audio/video tracks to the connection so the other peer receives them
        localStream.getTracks().forEach(track => {
            console.log(`[WebRTC] Adding local track: ${track.kind} (${track.label})`);
            peerConnection.addTrack(track, localStream);
        });

        // onicecandidate fires whenever the browser discovers a new network path.
        // We relay each candidate to the other peer via SignalR so they can try it.
        peerConnection.onicecandidate = async function (event) {
            if (event.candidate) {
                console.log('[WebRTC] Sending ICE candidate:', event.candidate.type);
                await connection.invoke('SendIceCandidate', classroomId, JSON.stringify(event.candidate));
            }
        };

        // ontrack fires when we start receiving the remote peer's audio/video stream.
        // We attach it to the remoteVideo element to display it.
        peerConnection.ontrack = function (event) {
            console.log('[WebRTC] Received remote track:', event.track.kind);
            remoteVideo.srcObject = event.streams[0];
            // Explicit play() in case autoplay is suppressed
            remoteVideo.play().catch(e => console.warn('[WebRTC] remoteVideo.play() interrupted:', e));
            setCallStatus('Connected');
        };

        // Monitor connection state for UI feedback
        peerConnection.onconnectionstatechange = function () {
            const state = peerConnection.connectionState;
            console.log('[WebRTC] Connection state:', state);
            if (state === 'connected') {
                setCallStatus('Connected');
            } else if (state === 'disconnected' || state === 'failed') {
                setCallStatus('Connection lost');
                setTimeout(cleanupCall, 3000);
            }
        };

        peerConnection.oniceconnectionstatechange = function () {
            console.log('[WebRTC] ICE state:', peerConnection.iceConnectionState);
        };
    }

    /// Stops all media tracks and resets the UI back to its default state.
    function cleanupCall() {
        if (peerConnection) {
            peerConnection.close();
            peerConnection = null;
        }
        if (localStream) {
            localStream.getTracks().forEach(track => track.stop());
            localStream = null;
        }
        localVideo.srcObject = null;
        remoteVideo.srcObject = null;
        isMuted = false;
        isCameraOff = false;
        muteBtn.textContent = '🎤 Mute';
        cameraBtn.textContent = '📹 Hide Camera';
        muteBtn.classList.remove('control-active');
        cameraBtn.classList.remove('control-active');
        hideCallPanel();
    }

    function showCallPanel() {
        callPanel.classList.add('active');
        startCallBtn.style.display = 'none';
    }

    function hideCallPanel() {
        callPanel.classList.remove('active');
        startCallBtn.style.display = '';
        setCallStatus('');
    }

    function setCallStatus(text) {
        if (callStatus) callStatus.textContent = text;
    }

    function handleMediaError(err) {
        console.error('[WebRTC] Media error:', err);
        if (err.name === 'NotAllowedError') {
            setCallStatus('Camera/microphone access was denied. Please allow access in your browser and try again.');
        } else if (err.name === 'NotFoundError') {
            setCallStatus('No camera or microphone found on this device.');
        } else {
            setCallStatus('Could not start video: ' + err.message);
        }
        hideCallPanel();
        startCallBtn.style.display = '';
    }

})();

// ─── PRODUCTION NOTE: Adding a TURN Server ────────────────────────────────────
//
// The STUN servers above work for most users but will fail for ~15% who are
// behind strict corporate firewalls or symmetric NATs.
//
// For production, add a TURN server to RTC_CONFIG like this:
//
//   const RTC_CONFIG = {
//       iceServers: [
//           { urls: 'stun:stun.l.google.com:19302' },
//           {
//               urls: 'turn:your-turn-server.com:3478',
//               username: 'your-username',
//               credential: 'your-password'
//           }
//       ]
//   };
//
// Free/cheap options:
//   - Metered.ca (free tier available): https://www.metered.ca/tools/openrelay/
//   - Twilio TURN: https://www.twilio.com/docs/stun-turn
//   - Self-host Coturn: https://github.com/coturn/coturn
//
// IMPORTANT: Never hardcode TURN credentials in client JS for production.
// Instead, create a controller endpoint that generates short-lived credentials
// and fetch them before each call.