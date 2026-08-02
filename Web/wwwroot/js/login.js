function initLogin(returnUrl) {
    const statusEl = document.getElementById('status');
    const spinnerEl = document.getElementById('spinner');

    const urlParams = new URLSearchParams(window.location.search);
    const chatId = urlParams.get('chatId');

    function fail(message) {
        if (spinnerEl) spinnerEl.style.display = 'none';
        if (statusEl) statusEl.textContent = message;
    }

    const tg = window.Telegram && window.Telegram.WebApp;

    if (chatId) {
        if (statusEl) statusEl.textContent = 'Отладка: Авторизация через ID ' + chatId + '...';
        fetch('/api/admin/auth-debug?chatId=' + chatId, {
            method: 'GET'
        }).then(function (response) {
            if (response.ok) {
                window.location.href = returnUrl;
            } else if (response.status === 403) {
                fail('Debug: Доступ запрещён. Пользователь не является администратором.');
            } else {
                fail('Debug: Не удалось авторизоваться через ID.');
            }
        }).catch(function () {
            fail('Debug: Ошибка соединения при отладочной авторизации.');
        });
    } else if (!tg || !tg.initData) {
        fail('Откройте панель управления через кнопку в Telegram-боте или используйте ?chatId=... для отладки.');
    } else {
        tg.ready();
        fetch('/api/admin/auth', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ initData: tg.initData })
        }).then(function (response) {
            if (response.ok) {
                window.location.href = returnUrl;
            } else if (response.status === 403) {
                fail('Доступ запрещён. У вас нет прав администратора.');
            } else {
                fail('Не удалось авторизоваться. Попробуйте снова через /start в боте.');
            }
        }).catch(function () {
            fail('Ошибка соединения с сервером.');
        });
    }
}
