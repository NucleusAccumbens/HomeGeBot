/* ===== TMA Modal Controller =====
 * Manages the confirmation modal overlay.
 * Single responsibility: show/close/confirm a modal dialog.
 * Open/closed principle: new modal types can extend without modifying existing logic.
 */

const ModalController = {
    _confirmCallback: null,

    show: function (text, confirmLabel, onConfirm) {
        var overlay = document.getElementById('modal-overlay');
        var textEl = document.getElementById('modal-text');
        var confirmBtn = document.getElementById('modal-confirm-btn');
        if (!overlay || !textEl || !confirmBtn) return;

        textEl.textContent = text;
        confirmBtn.textContent = confirmLabel || I18n.t('modal.confirm');
        this._confirmCallback = onConfirm;
        overlay.classList.add('active');
    },

    close: function () {
        var overlay = document.getElementById('modal-overlay');
        if (overlay) overlay.classList.remove('active');
        this._confirmCallback = null;
    },

    confirm: function () {
        if (this._confirmCallback) this._confirmCallback();
        this.close();
    }
};

/* Expose for inline onclick handlers */
window.showModal = function (text, confirmLabel, onConfirm) {
    ModalController.show(text, confirmLabel, onConfirm);
};
window.closeModal = function () {
    ModalController.close();
};
window.confirmModal = function () {
    ModalController.confirm();
};
