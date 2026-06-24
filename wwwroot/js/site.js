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
        if (res.ok) {
            btn.dataset.favorited = isFav ? 'false' : 'true';
            btn.querySelector('.fav-text').textContent = isFav ? 'Yêu thích' : 'Bỏ yêu thích';
            showToast(isFav ? 'Đã bỏ yêu thích' : 'Đã thêm vào yêu thích', 'success');
        }
    } catch (e) { showToast('Có lỗi xảy ra', 'error'); }
}

// ─── Notification mark read ───
async function markNotifRead(id) {
    try { await fetch(`/api/notifications/${id}/read`, { method: 'PUT', headers: getHeaders() }); } catch(e){}
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

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSiteFeedback);
} else {
    initSiteFeedback();
}
