/* ===== TMA Profile View =====
 * Manages the "My Page" tab: populates user info from Telegram initData.
 * Single responsibility: render the user's profile card and application count.
 * Depends on: ApplicationsView (for app count)
 */

const ProfileView = {
    load: function () {
        var tg = window.Telegram.WebApp;
        var user = tg.initDataUnsafe && tg.initDataUnsafe.user;

        this._renderName(user);
        this._renderUsername(user);
        this._renderAvatar(user);
        this._renderStats();
        this._loadManagerLink();
        this._loadProfileFromApi(tg.initData || '');
    },

    _loadProfileFromApi: function (initData) {
        if (!initData) return;
        fetch(TmaConstants.ApiEndpoints.Profile + '?initData=' + encodeURIComponent(initData))
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (data) {
                if (!data) return;
                var user = {
                    firstName: data.firstName,
                    lastName: data.lastName,
                    username: data.username,
                    photoUrl: data.photoUrl
                };
                ProfileView._renderName(user);
                ProfileView._renderUsername(user);
                ProfileView._renderAvatar(user);
            })
            .catch(function () {});
    },

    _renderName: function (user) {
        var el = document.getElementById('profile-name');
        if (!el) return;
        if (user) {
            var fullName = ((user.firstName || '') + ' ' + (user.lastName || '')).trim();
            el.textContent = fullName || I18n.t('profile.guest');
        } else {
            el.textContent = I18n.t('profile.guest');
        }
    },

    _renderUsername: function (user) {
        var el = document.getElementById('profile-username');
        if (!el) return;
        if (user && user.username) {
            el.textContent = '@' + user.username;
            el.style.display = '';
        } else {
            el.style.display = 'none';
        }
    },

    _renderAvatar: function (user) {
        var el = document.getElementById('profile-avatar');
        if (!el) return;
        el.innerHTML = '';
        el.style.backgroundImage = '';

        if (user && user.photoUrl) {
            el.style.backgroundImage = 'url("' + user.photoUrl + '")';
        } else {
            el.innerHTML = '<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" /></svg>';
        }
    },

    _renderStats: function () {
        var el = document.getElementById('stat-applications');
        if (!el) return;
        var apps = ApplicationsView.getCachedApps();
        el.textContent = (apps && apps.length) || 0;
    },

    _loadManagerLink: function () {
        var tg = window.Telegram.WebApp;
        var initData = tg.initData || '';
        fetch(TmaConstants.ApiEndpoints.Manager + '?initData=' + encodeURIComponent(initData))
            .then(function (r) { return r.ok ? r.json() : null; })
            .then(function (data) {
                if (data && data.username) {
                    ProfileView._managerUsername = data.username;
                }
            })
            .catch(function () {});
    },

    _managerUsername: null
};
