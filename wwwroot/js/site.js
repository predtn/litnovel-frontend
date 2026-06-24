// LitNovel — site.js
// ─── Navigation ───
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

// ─── Reading Progress (auto-save) ───
let progressTimer = null;
function initReadingProgress(chapterId) {
    const content = document.getElementById('chapterContent');
    if (!content) return;
    window.addEventListener('scroll', () => {
        clearTimeout(progressTimer);
        progressTimer = setTimeout(() => {
            const scrolled = window.scrollY + window.innerHeight;
            const total = document.documentElement.scrollHeight;
            const pct = Math.min(100, Math.round((scrolled / total) * 100));
            saveProgress(chapterId, pct);
        }, 1000);
    });
}
async function saveProgress(chapterId, pct) {
    try {
        await fetch(`/api/chapters/${chapterId}/progress`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ progressPercentage: pct })
        });
    } catch (e) { /* silent */ }
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

async function loadUnreadNotificationSnapshot() {
    const bell = document.getElementById('notifBell');
    if (!bell || bell.dataset.authenticated !== 'true') return;

    try {
        const res = await fetch('/api/notifications?isRead=false&page=1&size=1', { headers: getHeaders() });
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

async function loadHomepageAnnouncements() {
    const root = document.getElementById('homeAnnouncements');
    const list = document.getElementById('homeAnnouncementsList');
    if (!root || !list) return;

    try {
        const res = await fetch('/api/announcements', { headers: getHeaders() });
        if (!res.ok) return;
        const payload = await res.json();
        const announcements = (payload?.data ?? payload ?? [])
            .filter(item => item?.isActive)
            .filter(item => {
                const now = Date.now();
                const start = item.startDate ? Date.parse(item.startDate) : 0;
                const end = item.endDate ? Date.parse(item.endDate) : Number.POSITIVE_INFINITY;
                return start <= now && end >= now;
            })
            .sort((a, b) => Date.parse(b.startDate || 0) - Date.parse(a.startDate || 0))
            .slice(0, 3);

        renderHomepageAnnouncements(root, list, announcements);
    } catch (e) {
        // Ignore transient polling failures.
    }
}

function renderHomepageAnnouncements(root, list, announcements) {
    if (!announcements.length) {
        list.replaceChildren();
        root.classList.add('hidden');
        return;
    }

    const currentIds = Array.from(list.querySelectorAll('[data-announcement-id]')).map(item => item.dataset.announcementId).join(',');
    const nextIds = announcements.map(item => String(item.id)).join(',');
    if (currentIds === nextIds) return;

    const fragment = document.createDocumentFragment();
    announcements.forEach(item => {
        const article = document.createElement('article');
        article.className = 'home-announcement';
        article.dataset.announcementId = item.id;
        article.innerHTML = `
            <div class="home-announcement__icon" aria-hidden="true">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M3 11v2a2 2 0 0 0 2 2h2l4 4V5L7 9H5a2 2 0 0 0-2 2Z"/><path d="M16 9a5 5 0 0 1 0 6"/><path d="M19 6a9 9 0 0 1 0 12"/></svg>
            </div>
            <div class="home-announcement__body">
                <h2 class="home-announcement__title"></h2>
                <div class="home-announcement__content"></div>
            </div>`;
        article.querySelector('.home-announcement__title').textContent = item.title || '';
        article.querySelector('.home-announcement__content').innerHTML = item.content || '';
        fragment.appendChild(article);
    });

    list.replaceChildren(fragment);
    root.classList.remove('hidden');
}

function initRealtimeFallbacks() {
    loadUnreadNotificationSnapshot();
    setInterval(loadUnreadNotificationSnapshot, 15000);

    loadHomepageAnnouncements();
    setInterval(loadHomepageAnnouncements, 30000);
}

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
    if (!modal) { if (confirm(message)) onConfirm(); return; }
    document.getElementById('confirmMessage').textContent = message;
    document.getElementById('confirmBtn').onclick = () => { closeModal('confirmModal'); onConfirm(); };
    openModal('confirmModal');
}

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
    initSiteFeedback();
    initRealtimeFallbacks();
}

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSite);
} else {
    initSite();
}
