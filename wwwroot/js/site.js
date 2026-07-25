// LitNovel — site.js
// ─── Navigation ───
const THEME_STORAGE_KEY = 'litnovel-theme';

function getActiveTheme() {
    return document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
}

function applyTheme(theme) {
    const nextTheme = theme === 'dark' ? 'dark' : 'light';
    if (nextTheme === 'dark') {
        document.documentElement.setAttribute('data-theme', 'dark');
    } else {
        document.documentElement.removeAttribute('data-theme');
    }

    try {
        localStorage.setItem(THEME_STORAGE_KEY, nextTheme);
    } catch (e) {
        // Ignore storage failures in private browsing modes.
    }

    updateThemeToggleButtons();
}

function updateThemeToggleButtons() {
    const isDark = getActiveTheme() === 'dark';
    document.querySelectorAll('[data-theme-toggle]').forEach(button => {
        button.setAttribute('aria-pressed', String(isDark));
        button.setAttribute('aria-label', isDark ? 'Switch to light mode' : 'Switch to dark mode');
        button.setAttribute('title', isDark ? 'Light mode' : 'Dark mode');
    });
}

function initThemeToggle() {
    updateThemeToggleButtons();
    document.querySelectorAll('[data-theme-toggle]').forEach(button => {
        if (button.dataset.themeToggleBound === 'true') return;
        button.addEventListener('click', () => {
            applyTheme(getActiveTheme() === 'dark' ? 'light' : 'dark');
        });
        button.dataset.themeToggleBound = 'true';
    });
}

function toggleMobileMenu() {
    const menu = document.getElementById('mobileMenu');
    if (menu) menu.classList.toggle('open');
}
function toggleDropdown() {
    const dd = document.getElementById('userDropdown');
    if (dd) dd.classList.toggle('open');
}
document.addEventListener('click', function (e) {
    const dd = document.getElementById('userDropdown');
    if (dd && !dd.contains(e.target)) dd.classList.remove('open');
});

// ─── Toast ───
function showToast(message, type = 'info', duration = 3000) {
    const container = document.getElementById('toastContainer');
    if (!container) return;
    const icons = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' };
    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.innerHTML = `<span class="toast-icon">${icons[type] || 'ℹ'}</span><span>${message}</span>`;
    toast.onclick = () => toast.remove();
    container.appendChild(toast);
    setTimeout(() => toast.remove(), duration);
}

// ─── Modal ───
// Safe toast renderer. This replaces the legacy implementation above without
// injecting server-provided messages as HTML.
showToast = function (message, type = 'info', duration = 3000) {
    const container = document.getElementById('toastContainer');
    if (!container) return;

    const validTypes = new Set(['success', 'error', 'warning', 'info']);
    const toastType = validTypes.has(type) ? type : 'info';
    const icons = { success: 'OK', error: 'X', warning: '!', info: 'i' };

    const toast = document.createElement('div');
    toast.className = `toast toast-${toastType}`;

    const icon = document.createElement('span');
    icon.className = 'toast-icon';
    icon.textContent = icons[toastType] || icons.info;

    const text = document.createElement('span');
    text.textContent = message || '';

    toast.append(icon, text);
    toast.onclick = () => toast.remove();
    container.appendChild(toast);
    setTimeout(() => {
        if (toast.isConnected) toast.remove();
    }, duration);
};

function openModal(id) {
    const m = document.getElementById(id);
    if (m) { m.classList.add('open'); document.body.style.overflow = 'hidden'; }
}
function closeModal(id) {
    const m = document.getElementById(id);
    if (m) { m.classList.remove('open'); document.body.style.overflow = ''; }
}
document.addEventListener('click', function (e) {
    if (e.target.classList.contains('modal-overlay')) {
        e.target.classList.remove('open');
        document.body.style.overflow = '';
    }
});

// ─── Star Rating ───
function initStarRating(containerId, inputId) {
    const container = document.getElementById(containerId);
    const input = document.getElementById(inputId);
    if (!container || !input) return;
    const stars = container.querySelectorAll('.star');
    stars.forEach((star, i) => {
        star.addEventListener('mouseover', () => highlightStars(stars, i));
        star.addEventListener('mouseleave', () => highlightStars(stars, parseInt(input.value || 0) - 1));
        star.addEventListener('click', () => { input.value = i + 1; highlightStars(stars, i); });
    });
}
function highlightStars(stars, upTo) {
    stars.forEach((s, i) => s.classList.toggle('star-filled', i <= upTo));
}

// ─── Favorite toggle ───
async function toggleFavorite(novelId, btn) {
    const isFav = btn.dataset.favorited === 'true';
    try {
        const method = isFav ? 'DELETE' : 'POST';
        const res = await fetch(`/api/novels/${novelId}/favorites`, { method, headers: getHeaders() });
        if (!res.ok) {
            showToast('Có lỗi xảy ra', 'error');
            return;
        }

        btn.dataset.favorited = isFav ? 'false' : 'true';
        btn.querySelector('.fav-text').textContent = isFav ? 'Yêu thích' : 'Bỏ yêu thích';
        showToast(isFav ? 'Đã bỏ yêu thích' : 'Đã thêm vào yêu thích', 'success');
    } catch (e) { showToast('Có lỗi xảy ra', 'error'); }
}

// ─── Novel like toggle ───
async function toggleNovelLike(novelId, btn) {
    const isLiked = btn.dataset.liked === 'true';
    try {
        const method = isLiked ? 'DELETE' : 'POST';
        const res = await fetch(`/api/novels/${novelId}/likes`, { method, headers: getHeaders() });
        if (!res.ok) {
            showToast('Có lỗi xảy ra', 'error');
            return;
        }

        btn.dataset.liked = isLiked ? 'false' : 'true';
        btn.querySelector('.like-text').textContent = isLiked ? 'Thích' : 'Bỏ thích';
        showToast(isLiked ? 'Đã bỏ thích truyện' : 'Đã thích truyện', 'success');

        const likeCount = document.getElementById('novelLikeCount');
        if (likeCount) {
            const current = Number.parseInt(likeCount.dataset.count || likeCount.textContent || '0', 10) || 0;
            const next = Math.max(0, current + (isLiked ? -1 : 1));
            likeCount.dataset.count = next.toString();
            likeCount.textContent = formatCompactNumber(next);
        }
    } catch (e) { showToast('Có lỗi xảy ra', 'error'); }
}

function formatCompactNumber(value) {
    if (value >= 1000000) return `${(value / 1000000).toFixed(value >= 10000000 ? 0 : 1).replace(/\.0$/, '')}M`;
    if (value >= 1000) return `${(value / 1000).toFixed(value >= 10000 ? 0 : 1).replace(/\.0$/, '')}K`;
    return value.toString();
}

// ─── Notification mark read ───
async function markNotifRead(id) {
    try { await fetch(`/api/notifications/${id}/read`, { method: 'PUT', headers: getHeaders() }); } catch(e){}
}

// ─── Lightweight realtime fallback ───
let lastUnreadCount = null;
let notificationFallbackTimer = null;
const notificationFallbackIntervalMs = 60000;

async function loadUnreadNotificationSnapshot() {
    const bell = document.getElementById('notifBell');
    if (!bell || bell.dataset.authenticated !== 'true') return;

    try {
        const res = await fetch('/api/notifications?isRead=false&page=1&size=1', { headers: getHeaders() });
        if (res.status === 401 || res.status === 403) {
            stopNotificationFallbackPolling();
            return;
        }
        if (!res.ok) return;
        const payload = await res.json();
        const data = payload?.data ?? payload;
        const unreadCount = Number(data?.unreadCount ?? data?.totalElements ?? (Array.isArray(data) ? data.length : data?.items?.length ?? 0));
        const latest = Array.isArray(data?.items) ? data.items[0] : Array.isArray(data) ? data[0] : null;

        updateNotificationBadge(unreadCount);
        if (lastUnreadCount !== null && unreadCount > lastUnreadCount) {
            const message = latest?.message || latest?.body || latest?.title || 'Bạn có thông báo mới.';
            showToast(message, 'info', 5000);
        }
        lastUnreadCount = unreadCount;
    } catch (e) {
        // Ignore transient polling failures.
    }
}

function startNotificationFallbackPolling() {
    const bell = document.getElementById('notifBell');
    if (!bell || bell.dataset.authenticated !== 'true') return;
    if (notificationFallbackTimer || window.litNovelNotificationSignalRConnected) return;

    notificationFallbackTimer = setInterval(loadUnreadNotificationSnapshot, notificationFallbackIntervalMs);
}

function stopNotificationFallbackPolling() {
    if (!notificationFallbackTimer) return;
    clearInterval(notificationFallbackTimer);
    notificationFallbackTimer = null;
}

function updateNotificationBadge(count) {
    const badge = document.getElementById('notifBellCount');
    if (!badge) return;

    if (count > 0) {
        badge.textContent = count > 9 ? '9+' : String(count);
        badge.classList.remove('hidden');
    } else {
        badge.textContent = '';
        badge.classList.add('hidden');
    }
}

function initRealtimeFallbacks() {
    loadUnreadNotificationSnapshot();
    setTimeout(() => {
        if (!window.litNovelNotificationSignalRConnected) {
            startNotificationFallbackPolling();
        }
    }, 5000);
}

window.loadUnreadNotificationSnapshot = loadUnreadNotificationSnapshot;
window.updateNotificationBadge = updateNotificationBadge;
window.startNotificationFallbackPolling = startNotificationFallbackPolling;
window.stopNotificationFallbackPolling = stopNotificationFallbackPolling;

// ─── Helpers ───
function getHeaders() {
    const token = getCookie('litnovel_token');
    return { 'Content-Type': 'application/json', ...(token ? { 'Authorization': `Bearer ${token}` } : {}) };
}
function getCookie(name) {
    const v = document.cookie.match('(^|;) ?' + name + '=([^;]*)(;|$)');
    return v ? v[2] : null;
}

// ─── Confirm dialog ───
function confirmAction(message, onConfirm) {
    const modal = document.getElementById('confirmModal');
    if (!modal) return;
    const messageEl = document.getElementById('confirmMessage');
    const confirmBtn = document.getElementById('confirmBtn');
    if (!messageEl || !confirmBtn) return;
    messageEl.textContent = message;
    confirmBtn.onclick = () => { closeModal('confirmModal'); onConfirm(); };
    openModal('confirmModal');
}

function confirmSubmit(form, message) {
    if (!form) return false;
    if (form.dataset.confirmed === 'true') {
        delete form.dataset.confirmed;
        return true;
    }

    confirmAction(message, () => {
        form.dataset.confirmed = 'true';
        if (typeof form.requestSubmit === 'function') {
            form.requestSubmit();
        } else {
            form.submit();
        }
    });

    return false;
}

window.confirmAction = confirmAction;
window.confirmSubmit = confirmSubmit;

// ─── Search debounce ───
function debounce(fn, delay) {
    let timer;
    return function (...args) { clearTimeout(timer); timer = setTimeout(() => fn(...args), delay); };
}

// ─── URL params helper ───
function updateQueryParam(key, value) {
    const url = new URL(window.location);
    if (value) url.searchParams.set(key, value);
    else url.searchParams.delete(key);
    window.location = url.toString();
}

// ─── Tab switching ───
function switchTab(tabId, contentPrefix) {
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('[data-tab-content]').forEach(c => c.classList.add('hidden'));
    document.querySelector(`[data-tab="${tabId}"]`)?.classList.add('active');
    document.querySelector(`[data-tab-content="${tabId}"]`)?.classList.remove('hidden');
}

const PAGE_TRANSITION_MS = 500;
let pageTransitionPending = false;

function getPageTransitionDelay() {
    return window.matchMedia?.('(prefers-reduced-motion: reduce)').matches ? 0 : PAGE_TRANSITION_MS;
}

function startPageTransition(callback) {
    if (pageTransitionPending) return;
    const transition = document.getElementById('pageTransition');
    if (!transition) {
        callback();
        return;
    }

    pageTransitionPending = true;
    transition.classList.remove('is-active');
    void transition.offsetWidth;
    transition.classList.add('is-active');

    setTimeout(callback, getPageTransitionDelay());
}

function resetPageTransition() {
    pageTransitionPending = false;
    document.getElementById('pageTransition')?.classList.remove('is-active');
}

function isModifiedNavigation(event) {
    return event.defaultPrevented || event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey;
}

function shouldSkipPageTransitionLink(link, event) {
    if (!link || isModifiedNavigation(event)) return true;
    if (link.hasAttribute('download')) return true;
    if (link.dataset.noPageTransition === 'true' || link.closest('[data-no-page-transition="true"]')) return true;
    if (link.target && link.target !== '_self') return true;

    const href = link.getAttribute('href');
    if (!href || href.startsWith('#') || href.startsWith('javascript:') || href.startsWith('mailto:') || href.startsWith('tel:')) return true;

    const nextUrl = new URL(href, window.location.href);
    if (nextUrl.origin !== window.location.origin) return true;

    const current = new URL(window.location.href);
    const sameDocumentHash = nextUrl.pathname === current.pathname
        && nextUrl.search === current.search
        && nextUrl.hash
        && nextUrl.hash !== current.hash;

    return sameDocumentHash;
}

function initPageTransitions() {
    document.addEventListener('click', event => {
        const link = event.target.closest?.('a[href]');
        if (shouldSkipPageTransitionLink(link, event)) return;

        event.preventDefault();
        startPageTransition(() => {
            window.location.href = link.href;
        });
    });

    document.addEventListener('submit', event => {
        const form = event.target;
        if (!(form instanceof HTMLFormElement)) return;
        if (event.defaultPrevented || form.dataset.noPageTransition === 'true') return;
        if ((form.method || 'get').toLowerCase() !== 'get') return;
        if (form.target && form.target !== '_self') return;

        event.preventDefault();
        const nextUrl = new URL(form.action || window.location.href, window.location.href);
        nextUrl.search = new URLSearchParams(new FormData(form)).toString();

        startPageTransition(() => {
            window.location.href = nextUrl.toString();
        });
    });

    window.addEventListener('pageshow', resetPageTransition);
    window.addEventListener('pagehide', resetPageTransition);
}

// Auto-init
function initSiteFeedback() {
    // Auto-close success alerts
    document.querySelectorAll('.alert-auto-close').forEach(a => {
        setTimeout(() => a.remove(), 4000);
    });

    // Show toasts from server-injected flash messages.
    document.querySelectorAll('.server-toast, #serverToast').forEach(toastMsg => {
        showToast(toastMsg.dataset.message, toastMsg.dataset.type || 'info');
        toastMsg.remove();
    });
}

function initSite() {
    initThemeToggle();
    initPageTransitions();
    initSiteFeedback();
    initRealtimeFallbacks();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSite);
} else {
    initSite();
}
