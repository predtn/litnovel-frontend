/**
 * LitNovel realtime notifications.
 * SignalR is the primary channel; HTTP polling is only a fallback while the
 * hub is unavailable.
 */
(function () {
    'use strict';

    if (window.__litNovelNotificationHubStarted) return;
    window.__litNovelNotificationHubStarted = true;

    const tokenMeta = document.querySelector('meta[name="signalr-token"]');
    const token = tokenMeta ? tokenMeta.content : null;

    if (!token || !window.signalR) {
        window.startNotificationFallbackPolling?.();
        return;
    }

    const hubUrl =
        document.querySelector('meta[name="signalr-hub-url"]')?.content ||
        window.litNovelNotificationHubUrl ||
        'http://localhost:5181/hubs/notifications';

    const connection = new signalR.HubConnectionBuilder()
        .withUrl(hubUrl, {
            accessTokenFactory: () => token
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Warning)
        .build();

    connection.on('ReceiveNotification', function (notification) {
        incrementBadge();
        window.showToast?.(notification?.message || 'Bạn có thông báo mới.', 'info', 5000);
    });

    connection.onreconnecting(function () {
        setSignalRConnected(false);
        window.startNotificationFallbackPolling?.();
    });

    connection.onreconnected(function () {
        setSignalRConnected(true);
        window.stopNotificationFallbackPolling?.();
        window.loadUnreadNotificationSnapshot?.();
    });

    connection.onclose(function () {
        setSignalRConnected(false);
        window.startNotificationFallbackPolling?.();
        setTimeout(startConnection, 5000);
    });

    async function startConnection() {
        try {
            await connection.start();
            setSignalRConnected(true);
            window.stopNotificationFallbackPolling?.();
            window.loadUnreadNotificationSnapshot?.();
            console.log('[SignalR] Connected to NotificationHub.');
        } catch (err) {
            setSignalRConnected(false);
            window.startNotificationFallbackPolling?.();
            console.warn('[SignalR] Connection failed, will retry in 5s.', err);
            setTimeout(startConnection, 5000);
        }
    }

    function setSignalRConnected(isConnected) {
        window.litNovelNotificationSignalRConnected = isConnected;
    }

    function incrementBadge() {
        const badge = document.getElementById('notifBellCount');
        if (!badge) return;

        const current = badge.textContent === '9+' ? 9 : parseInt(badge.textContent, 10) || 0;
        const next = current + 1;

        badge.textContent = next > 9 ? '9+' : String(next);
        badge.classList.remove('hidden');
    }

    startConnection();
}());
