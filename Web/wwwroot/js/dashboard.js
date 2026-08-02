function openTab(evt, tabName) {
    var i, tabcontent, tablinks;
    tabcontent = document.getElementsByClassName("tab-content");
    for (i = 0; i < tabcontent.length; i++) {
        tabcontent[i].classList.remove('active');
    }
    tablinks = document.getElementsByClassName("tab-btn");
    for (i = 0; i < tablinks.length; i++) {
        tablinks[i].classList.remove('active');
    }
    document.getElementById(tabName).classList.add('active');
    evt.currentTarget.classList.add('active');
}

function setDefaultTab() {
    var tabBar = document.querySelector('.tab-bar');
    var requestedTab = tabBar ? tabBar.getAttribute('data-active-tab') : null;
    if (requestedTab) {
        var btn = document.querySelector('.tab-btn[data-tab="' + requestedTab + '"]');
        var content = document.getElementById(requestedTab);
        if (btn && content) {
            document.querySelectorAll('.tab-btn').forEach(function(b) { b.classList.remove('active'); });
            document.querySelectorAll('.tab-content').forEach(function(c) { c.classList.remove('active'); });
            btn.classList.add('active');
            content.classList.add('active');
            return;
        }
    }
    var defaultOpen = document.getElementById("defaultOpen");
    if (defaultOpen) {
        defaultOpen.click();
    }
}

window.onload = setDefaultTab;

function loadManagerProfilePhoto() {
    var avatar = document.getElementById('manager-profile-avatar');
    if (!avatar) return;
    var chatId = avatar.getAttribute('data-chat-id');
    if (!chatId || chatId === '0') return;

    fetch('/api/dashboard/user-photo?chatId=' + chatId)
        .then(function (r) { return r.ok ? r.json() : null; })
        .then(function (data) {
            if (data && data.photoUrl) {
                avatar.innerHTML = '';
                avatar.style.backgroundImage = 'url("' + data.photoUrl + '")';
            }
        })
        .catch(function () {});
}

$(document).ready(function () {
    $('a').attr('target', '_blank');
    loadManagerProfilePhoto();

    /* Keyboard detection: hide tab bar when keyboard opens */
    var viewportHeight = window.innerHeight;
    window.addEventListener('resize', function () {
        var newHeight = window.innerHeight;
        var keyboardOpen = newHeight < viewportHeight * 0.75;
        document.body.classList.toggle('keyboard-open', keyboardOpen);
    });

    /* Dismiss keyboard when tapping outside input fields */
    document.addEventListener('touchstart', function (e) {
        var el = document.activeElement;
        if (el && (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') && !el.contains(e.target)) {
            el.blur();
        }
    }, { passive: true });

    $('#user-search-input').on('input', function () {
        var query = $(this).val().toLowerCase().trim();
        var cards = $('#user-card-list .item-card');
        var visibleCount = 0;

        cards.each(function () {
            var searchData = $(this).attr('data-search') || "";
            var matches = searchData.indexOf(query) !== -1;
            $(this).toggle(matches);
            if (matches) visibleCount++;
        });

        $('#user-search-empty').toggle(visibleCount === 0);
    });

    $('form[data-confirm]').on('submit', function (e) {
        var form = this;
        var message = $(form).attr('data-confirm');
        if (!message) return true;
        if ($(form).data('confirmed')) return true;
        e.preventDefault();
        showModal(message, 'Подтвердить', function () {
            $(form).data('confirmed', true);
            showLoading();
            form.submit();
            $(form).data('confirmed', false);
        });
        return false;
    });
});
