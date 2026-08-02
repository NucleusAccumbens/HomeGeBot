/* ===== TMA Applications View =====
 * Manages the "My Applications" tab: loading, caching, and rendering application cards.
 * Single responsibility: display the user's submitted applications as cards.
 * Depends on: TmaConstants, TmaUtils, ModalController
 */

const ApplicationsView = {
    _cachedApps: null,

    /* --- Data loading --- */

    load: async function () {
        if (this._cachedApps) {
            this._render(this._cachedApps);
            return;
        }
        try {
            var tg = window.Telegram.WebApp;
            var response = await fetch(TmaConstants.ApiEndpoints.Applications + '?initData=' + encodeURIComponent(tg.initData));
            if (response.ok) {
                this._cachedApps = await response.json();
                this._render(this._cachedApps);
            }
        } catch (e) { }
    },

    /* --- Used by init to pre-fetch without rendering --- */

    preload: async function () {
        try {
            var tg = window.Telegram.WebApp;
            var response = await fetch(TmaConstants.ApiEndpoints.Applications + '?initData=' + encodeURIComponent(tg.initData));
            if (response.ok) {
                this._cachedApps = await response.json();
            }
        } catch (e) { }
    },

    getCachedApps: function () {
        return this._cachedApps;
    },

    /* --- Rendering --- */

    _render: function (apps) {
        var list = document.getElementById('apps-list');
        var empty = document.getElementById('apps-empty');
        if (!list) return;

        if (!apps || apps.length === 0) {
            list.innerHTML = '';
            if (empty) empty.style.display = 'block';
            return;
        }
        if (empty) empty.style.display = 'none';

        list.innerHTML = '<div class="card-list">' + apps.map(this._renderCard, this).join('') + '</div>';
    },

    _renderCard: function (app, i) {
        var cardClass = i % 3 === 0 ? 'light-card' : i % 3 === 1 ? 'dark-card' : 'third-card';
        var country = app.country === TmaConstants.CountryOtherValue && app.countryOther
            ? app.countryOther : TmaConstants.CountryNames[app.country] || '—';
        var term = app.term === TmaConstants.TermOtherValue && app.termOther
            ? app.termOther : TmaConstants.TermNames[app.term] || '—';
        var petsText = app.hasPets ? I18n.t('summary.hasPets.yes') : I18n.t('summary.hasPets.no');
        var date = TmaUtils.formatDate(app.createdAt);
        var managerUsername = app.managerUsername ? TmaUtils.escapeHtml(app.managerUsername) : '—';

        var contactBtn = '';
        if (app.managerUsername) {
            contactBtn = '<button class="contact-btn" onclick="openManagerChat(\'' + TmaUtils.escapeHtml(app.managerUsername) + '\')">' + I18n.t('apps.contactManager') + '</button>';
        }

        var svgGlobe = '<svg class="grid-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M2 12h20M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>';
        var svgBriefcase = '<svg class="grid-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="2" y="7" width="20" height="14" rx="2"/><path d="M16 7V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v2"/></svg>';
        var svgPaw = '<svg class="grid-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="4" r="2"/><circle cx="18" cy="8" r="2"/><circle cx="4" cy="8" r="2"/><circle cx="20" cy="16" r="2"/><path d="M7 20a4 4 0 0 0 8 0M12 12a4 4 0 0 0 4 4M8 16a4 4 0 0 0 4-4M12 6v6"/></svg>';
        var svgCalendar = '<svg class="grid-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="4" width="18" height="18" rx="2"/><path d="M16 2v4M8 2v4M3 10h18"/></svg>';

        return '<div class="item-card ' + cardClass + '">'
            + '<div class="item-card-header">'
            +   '<div class="card-title-group">'
            +     '<span class="card-subtitle">' + date + '</span>'
            +   '</div>'
            + '</div>'
            + '<div class="card-divider"></div>'
            + '<div class="item-card-grid">'
            +   '<div class="grid-item">' + svgGlobe + '<span class="text">' + TmaUtils.escapeHtml(country) + '</span></div>'
            +   '<div class="grid-item">' + svgBriefcase + '<span class="text">' + TmaUtils.escapeHtml(app.profession || '—') + '</span></div>'
            +   '<div class="grid-item">' + svgPaw + '<span class="text">' + petsText + '</span></div>'
            +   '<div class="grid-item">' + svgCalendar + '<span class="text">' + TmaUtils.escapeHtml(term) + '</span></div>'
            + '</div>'
            + contactBtn
            + '</div>';
    }
};

/* Expose for inline onclick handlers */
window.openManagerChat = function (username) {
    var url = 'https://t.me/' + username;
    ModalController.show(I18n.t('confirm.contactManager', { username: username }), I18n.t('confirm.contactManagerBtn'), function () {
        TmaUtils.openTelegramLink(url);
    });
};
