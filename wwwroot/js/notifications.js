/**
 * LitNovel — Real-time Notification via SignalR
 * Connects to /hubs/notifications using the JWT access token.
 * On receiving a notification: updates badge count + shows a toast.
 */
(function () {
    'use strict';

    // Token is embedded by the layout as a meta tag
    const tokenMeta = document.querySelector('meta[name="signalr-token"]');
    const token = tokenMeta ? tokenMeta.content : null;

    if (!token) return; // User not logged in

    // ── SignalR connection ──────────────────────────────────────────────────
    const connection = new signalR.HubConnectionBuilder()
        .withUrl('http://localhost:5181/hubs/notifications', {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('ReceiveNotification', function (notification) {
        incrementBadge();
        showToast(notification.message || 'Bạn có thông báo mới.');
    });

    async function startConnection() {
        try {
            await connection.start();
            console.log('[SignalR] Connected to NotificationHub.');
        } catch (err) {
            console.warn('[SignalR] Connection failed, will retry in 5s.', err);
            setTimeout(startConnection, 5000);
        }
    }

    startConnection();

    // ── Badge ───────────────────────────────────────────────────────────────
    function incrementBadge() {
        const badge = document.getElementById('notifBellCount');
        if (!badge) return;
        const current = parseInt(badge.textContent, 10) || 0;
        badge.textContent = current + 1 > 9 ? '9+' : String(current + 1);
        badge.classList.remove('hidden');
    }

    // ── Toast ────────────────────────────────────────────────────────────────
    function showToast(message) {
        const toast = document.createElement('div');
        toast.className = 'signalr-toast';
        toast.innerHTML =
            '<span class="signalr-toast__icon">🔔</span>' +
            '<span class="signalr-toast__text">' + escapeHtml(message) + '</span>';

        document.body.appendChild(toast);

        // Trigger animation
        requestAnimationFrame(function () {
            toast.classList.add('signalr-toast--visible');
        });

        setTimeout(function () {
            toast.classList.remove('signalr-toast--visible');
            toast.addEventListener('transitionend', function () { toast.remove(); }, { once: true });
        }, 4000);
    }

    function escapeHtml(str) {
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    // ── Toast CSS (injected once) ────────────────────────────────────────────
    if (!document.getElementById('signalr-toast-style')) {
        const style = document.createElement('style');
        style.id = 'signalr-toast-style';
        style.textContent = [
            '.signalr-toast {',
            '  position: fixed; bottom: 24px; right: 24px; z-index: 9999;',
            '  display: flex; align-items: center; gap: 10px;',
            '  background: #1e2435; color: #e2e8f0;',
            '  border: 1px solid #3b4a6b; border-radius: 12px;',
            '  padding: 14px 20px; box-shadow: 0 8px 32px rgba(0,0,0,.45);',
            '  max-width: 340px; font-size: 14px; line-height: 1.4;',
            '  opacity: 0; transform: translateY(20px);',
            '  transition: opacity .3s ease, transform .3s ease;',
            '}',
            '.signalr-toast--visible { opacity: 1; transform: translateY(0); }',
            '.signalr-toast__icon { font-size: 20px; flex-shrink: 0; }',
        ].join('\n');
        document.head.appendChild(style);
    }
}());
