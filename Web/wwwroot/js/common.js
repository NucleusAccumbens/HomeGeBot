function showToast(msg) {
    var toast = document.getElementById('toast');
    if (!toast) return;
    toast.textContent = msg;
    toast.classList.add('show');
    setTimeout(function () { toast.classList.remove('show'); }, 2500);
}

/* ===== Dashboard Modal Controller ===== */

var _modalConfirmCallback = null;

function showModal(text, confirmLabel, onConfirm) {
    var overlay = document.getElementById('modal-overlay');
    var textEl = document.getElementById('modal-text');
    var confirmBtn = document.getElementById('modal-confirm-btn');
    if (!overlay || !textEl || !confirmBtn) return;

    textEl.textContent = text;
    confirmBtn.textContent = confirmLabel || 'OK';
    _modalConfirmCallback = onConfirm;
    overlay.classList.add('active');
}

function closeModal() {
    var overlay = document.getElementById('modal-overlay');
    if (overlay) overlay.classList.remove('active');
    _modalConfirmCallback = null;
}

function confirmModal() {
    if (_modalConfirmCallback) _modalConfirmCallback();
    closeModal();
}

function showLoading() {
    var overlay = document.getElementById('loading-overlay');
    if (overlay) overlay.classList.add('active');
}

function hideLoading() {
    var overlay = document.getElementById('loading-overlay');
    if (overlay) overlay.classList.remove('active');
}

function confirmAndOpenTelegramProfile(username) {
    var url = 'https://t.me/' + username;
    var message = 'Открыть профиль @' + username + ' в Telegram?';

    showModal(message, 'Открыть', function () {
        var tg = window.Telegram && window.Telegram.WebApp;
        if (tg && typeof tg.openTelegramLink === 'function') {
            tg.openTelegramLink(url);
        } else {
            window.open(url, '_blank');
        }
    });
}
