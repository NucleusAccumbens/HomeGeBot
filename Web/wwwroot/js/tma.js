/* ===== TMA Main Entry Point =====
 * Navigation controller and application bootstrap.
 * Single responsibility: coordinate view switching and initialize the TMA.
 * Depends on: ApplicationForm, ApplicationsView, ProfileView, ModalController, TmaUtils, TmaConstants
 *
 * Dependency Inversion: this module depends on the abstractions (other modules),
 * not on concrete DOM details — each module owns its own DOM interaction.
 */

const NavigationController = {
    /* --- Tab switching (matches admin dashboard's openTab pattern) --- */

    switchView: function (evt, viewName) {
        this._deactivateAll();
        this._activateView(viewName);
        if (evt && evt.currentTarget) {
            evt.currentTarget.classList.add('active');
        }
        this._toggleBottomBar(viewName);
        this._onViewEnter(viewName);
    },

    _deactivateAll: function () {
        document.querySelectorAll('.view').forEach(function (v) { v.classList.remove('active'); });
        document.querySelectorAll('.tab-btn').forEach(function (t) { t.classList.remove('active'); });
    },

    _activateView: function (viewName) {
        var view = document.getElementById('view-' + viewName);
        if (view) view.classList.add('active');
    },

    /* --- Bottom bar swap: form nav bar on Application tab, standard tab bar elsewhere --- */

    _toggleBottomBar: function (viewName) {
        var tabBar = document.querySelector('.tab-bar');
        var formNavBar = document.getElementById('form-nav-bar');
        var titleEl = document.getElementById('top-bar-title');
        var counterEl = document.getElementById('step-counter');
        if (viewName === 'application') {
            if (tabBar) tabBar.style.display = 'none';
            if (formNavBar) formNavBar.style.display = 'flex';
            if (titleEl) titleEl.style.display = 'none';
            if (counterEl) counterEl.style.display = 'block';
            this._showNavStep(ApplicationForm._currentStep);
        } else {
            if (formNavBar) formNavBar.style.display = 'none';
            if (tabBar) tabBar.style.display = 'flex';
            if (counterEl) counterEl.style.display = 'none';
            if (titleEl) {
                var titleKey = viewName === 'applications' ? 'tab.applications' : 'tab.profile';
                titleEl.textContent = I18n.t(titleKey);
                titleEl.style.display = 'block';
            }
        }
    },

    /* --- Show the nav button set matching the current form step --- */

    _showNavStep: function (step) {
        var sets = document.querySelectorAll('.nav-buttons-set');
        sets.forEach(function (s) { s.style.display = 'none'; });
        var target = document.querySelector('.nav-buttons-set[data-nav-step="' + step + '"]');
        if (target) target.style.display = 'block';
    },

    _onViewEnter: function (viewName) {
        if (viewName === 'applications') ApplicationsView.load();
        if (viewName === 'profile') ProfileView.load();
    },

    /* --- Default tab selection (matches admin dashboard's setDefaultTab pattern) --- */

    setDefaultTab: function () {
        var tabBar = document.querySelector('.tab-bar');
        var requestedTab = tabBar ? tabBar.getAttribute('data-active-tab') : null;
        if (requestedTab) {
            var btn = document.querySelector('.tab-btn[data-tab="' + requestedTab + '"]');
            if (btn) {
                btn.click();
                return;
            }
        }
        var defaultOpen = document.getElementById('defaultOpen');
        if (defaultOpen) {
            defaultOpen.click();
        }
    },

    /* --- Programmatic tab switch (for buttons outside the tab bar) --- */

    switchToApplicationForm: function () {
        ApplicationForm.resetToStep1();
        var btn = document.querySelector('.tab-btn[data-tab="application"]');
        if (btn) {
            btn.click();
        } else {
            this._deactivateAll();
            this._activateView('application');
            this._toggleBottomBar('application');
        }
    },

    /* --- Programmatic switch to "My Applications" (used by the form nav bar icon) --- */

    switchToApplicationsView: function () {
        var btn = document.querySelector('.tab-btn[data-tab="applications"]');
        if (btn) {
            btn.click();
        } else {
            this._deactivateAll();
            this._activateView('applications');
            this._toggleBottomBar('applications');
            ApplicationsView.load();
        }
    },

    /* --- Keyboard visibility handling --- */

    _onKeyboardToggle: function (keyboardOpen) {
        var tabBar = document.querySelector('.tab-bar');
        var formNavBar = document.getElementById('form-nav-bar');
        var summaries = document.querySelectorAll('.summary');

        if (keyboardOpen) {
            if (tabBar) tabBar.style.display = 'none';
            if (formNavBar) formNavBar.style.display = 'none';
            summaries.forEach(function (s) { s.style.display = 'none'; });
        } else {
            summaries.forEach(function (s) { s.style.display = ''; });
            var activeView = document.querySelector('.view.active');
            var viewName = activeView ? activeView.id.replace('view-', '') : null;
            this._toggleBottomBar(viewName);
        }
    }
};

/* --- Channel link confirmation --- */
function confirmOpenChannel(e) {
    e.preventDefault();
    ModalController.show(
        I18n.t('confirm.openChannel', { channel: TmaConstants.ChannelName }),
        I18n.t('confirm.openChannelBtn'),
        function () { TmaUtils.openTelegramLink(TmaConstants.ChannelUrl); }
    );
}

/* --- Contact manager (super admin) --- */
function contactManager(e) {
    e.preventDefault();
    var username = ProfileView._managerUsername;
    if (!username) return;
    var url = 'https://t.me/' + username;
    ModalController.show(I18n.t('confirm.contactManager', { username: username }), I18n.t('confirm.contactManagerBtn'), function () {
        TmaUtils.openTelegramLink(url);
    });
}

/* --- Bootstrap --- */
async function initTma() {
    var tg = window.Telegram.WebApp;
    tg.expand();

    /* Initialize i18n and locale-dependent constants */
    I18n.init();
    TmaConstants.refreshLocaleNames();

    /* Language dropdown toggle */
    var langToggle = document.getElementById('lang-dropdown-toggle');
    var langMenu = document.getElementById('lang-dropdown-menu');
    if (langToggle && langMenu) {
        langToggle.addEventListener('click', function (e) {
            e.stopPropagation();
            langMenu.style.display = langMenu.style.display === 'none' ? 'block' : 'none';
        });
        document.addEventListener('click', function () {
            langMenu.style.display = 'none';
        });
        langMenu.querySelectorAll('.lang-dropdown-item').forEach(function (item) {
            item.addEventListener('click', function () {
                I18n.setLang(item.getAttribute('data-lang'));
                TmaConstants.refreshLocaleNames();
                langMenu.style.display = 'none';
            });
        });
    }

    /* Re-render dynamic content when language changes */
    document.addEventListener('i18n:changed', function () {
        TmaConstants.refreshLocaleNames();
        var activeView = document.querySelector('.view.active');
        if (activeView) {
            var viewName = activeView.id.replace('view-', '');
            if (viewName === 'applications') ApplicationsView.load();
            if (viewName === 'application' && typeof ApplicationForm._currentStep === 'number') {
                ApplicationForm._renderSummary(ApplicationForm._currentStep);
            }
            var titleEl = document.getElementById('top-bar-title');
            if (titleEl && viewName !== 'application') {
                var titleKey = viewName === 'applications' ? 'tab.applications' : 'tab.profile';
                titleEl.textContent = I18n.t(titleKey);
            }
        }
        var reviewEl = document.getElementById('review-instructions');
        if (reviewEl) {
            reviewEl.innerHTML = I18n.t('form.review.instructions', { channel: '<a href="https://t.me/propertyintbilisi" onclick="confirmOpenChannel(event)">@propertyintbilisi</a>' });
        }
    });

    /* Dismiss keyboard when tapping outside input fields */
    document.addEventListener('touchstart', function (e) {
        var el = document.activeElement;
        if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') && !el.contains(e.target)) {
            el.blur();
        }
    }, { passive: true });

    /* Place cursor at end of existing text when focusing an input */
    document.addEventListener('focus', function (e) {
        var el = e.target;
        if (el.tagName === 'INPUT' && el.type === 'text') {
            var len = el.value.length;
            setTimeout(function () {
                el.setSelectionRange(len, len);
            }, 0);
        }
    }, true);

    /* Hide bottom bar + summary when the keyboard opens (viewport height shrinks) */
    NavigationController._keyboardOpen = false;
    var viewportHeight = window.innerHeight;
    window.addEventListener('resize', function () {
        var newHeight = window.innerHeight;
        var keyboardOpen = newHeight < viewportHeight * 0.75;
        if (keyboardOpen !== NavigationController._keyboardOpen) {
            NavigationController._keyboardOpen = keyboardOpen;
            NavigationController._onKeyboardToggle(keyboardOpen);
        }
    });

    /* Preload applications for caching + prefill data */
    await ApplicationsView.preload();
    var apps = ApplicationsView.getCachedApps();
    if (apps && apps.length > 0) {
        ApplicationForm.prefillFromData(apps[0]);
    }

    ApplicationForm.showStep(1);
    NavigationController.setDefaultTab();
}

/* Expose for inline onclick handlers */
window.switchView = function (evt, viewName) { NavigationController.switchView(evt, viewName); };
window.switchToApplicationForm = function () { NavigationController.switchToApplicationForm(); };
window.switchToApplicationsView = function () { NavigationController.switchToApplicationsView(); };
window.confirmOpenChannel = confirmOpenChannel;
window.contactManager = contactManager;
