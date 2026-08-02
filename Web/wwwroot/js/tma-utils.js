/* ===== TMA Utils =====
 * Shared constants and utility functions used across all TMA modules.
 * Single responsibility: provide common helpers with no domain logic.
 */

const TmaConstants = {
    CountryNames: null,
    TermNames: null,
    CountryOtherValue: 3,
    TermOtherValue: 2,
    ApiEndpoints: {
        SubmitApplication: '/api/tma/submit-application',
        Applications: '/api/tma/applications',
        Manager: '/api/tma/manager',
        Profile: '/api/tma/profile',
        SetLanguage: '/api/tma/set-language'
    },
    ChannelUrl: 'https://t.me/propertyintbilisi',
    ChannelName: '@propertyintbilisi',

    /* Refresh locale-dependent names from the i18n module */
    refreshLocaleNames: function () {
        this.CountryNames = [
            I18n.t('form.country.russia'),
            I18n.t('form.country.belarus'),
            I18n.t('form.country.ukraine'),
            I18n.t('form.country.other')
        ];
        this.TermNames = [
            I18n.t('form.term.6months'),
            I18n.t('form.term.1year'),
            I18n.t('form.term.other')
        ];
    }
};

const TmaUtils = {
    escapeHtml: function (text) {
        var div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    blockNewline: function (e) {
        if (e.key === 'Enter') { e.preventDefault(); }
    },

    stripNewlines: function (el) {
        el.value = el.value.replace(/[\r\n]+/g, ' ');
    },

    formatDate: function (isoDate) {
        return new Date(isoDate).toLocaleDateString('ru-RU');
    },

    openTelegramLink: function (url) {
        var tg = window.Telegram && window.Telegram.WebApp;
        if (tg && typeof tg.openTelegramLink === 'function') tg.openTelegramLink(url);
        else window.open(url, '_blank');
    }
};

/* Expose for inline onclick handlers */
window.TmaUtils = TmaUtils;
window.TmaConstants = TmaConstants;
