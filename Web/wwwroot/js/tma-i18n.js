/* ===== TMA Internationalization (i18n) =====
 * Manages language switching and translation of user-facing strings.
 * Single responsibility: provide translated strings and apply them to the DOM.
 * Open/closed: new languages are added by extending the _translations dictionary.
 * Available languages: Russian (ru), English (en), Georgian (ka).
 */

const I18n = {
    _currentLang: 'ru',
    _storageKey: 'tma_lang',

    /* --- Supported languages --- */
    languages: [
        { code: 'ru', label: 'Русский', flag: '🇷🇺', short: 'RU' },
        { code: 'en', label: 'English', flag: '🇬🇧', short: 'EN' },
        { code: 'ka', label: 'ქართული', flag: '🇬🇪', short: 'KA' }
    ],

    /* --- Translation dictionaries --- */
    _translations: {
        ru: {
            /* Tab bar */
            'tab.application': 'Заявка',
            'tab.applications': 'Мои заявки',
            'tab.profile': 'Моя страница',

            /* Application form — loading */
            'form.loading': 'Загрузка...',

            /* Application form — step 1 */
            'form.step1.title': 'Откуда вы?',
            'form.country.russia': 'Россия',
            'form.country.belarus': 'Беларусь',
            'form.country.ukraine': 'Украина',
            'form.country.other': 'Другое',
            'form.countryOther.placeholder': 'Укажите страну',

            /* Application form — step 2 */
            'form.step2.title': 'Чем занимаетесь?',
            'form.profession.placeholder': 'Напр. IT-специалист',

            /* Application form — step 3 */
            'form.step3.title': 'Есть ли у вас домашние животные?',
            'form.pets.yes': 'Да',
            'form.pets.no': 'Нет',

            /* Application form — step 4 */
            'form.step4.title': 'На какой срок вы планируете снять жилье?',
            'form.term.6months': '6 месяцев',
            'form.term.1year': '1 год',
            'form.term.other': 'Другое',
            'form.termOther.placeholder': 'Укажите срок',

            /* Application form — review */
            'form.review.title': 'Заявка отправлена!',
            'form.review.instructions': 'Чтобы завершить заявку, перешлите пост с квартирой из канала {channel} в чат с ботом.',

            /* Navigation buttons */
            'nav.next': 'Далее',
            'nav.back': 'Назад',
            'nav.submit': 'Отправить',
            'nav.done': 'Готово',

            /* Applications list */
            'apps.title': 'Мои заявки',
            'apps.empty': 'У вас пока нет заявок',
            'apps.contactManager': 'Написать менеджеру',

            /* Profile */
            'profile.title': 'Моя страница',
            'profile.stat.applications': 'Заявок',
            'profile.help.title': 'Нужна помощь?',
            'profile.help.text': 'Свяжитесь с менеджером или откройте канал {channel} с объявлениями',
            'profile.support': 'Поддержка',
            'profile.guest': 'Гость',

            /* Modal */
            'modal.cancel': 'Отмена',
            'modal.confirm': 'Открыть',

            /* Confirmation messages */
            'confirm.openChannel': 'Открыть канал {channel} в Telegram? Мини-приложение будет закрыто.',
            'confirm.contactManager': 'Написать в поддержку @{username} в Telegram?',
            'confirm.contactManagerBtn': 'Написать',
            'confirm.openChannelBtn': 'Открыть',

            /* Validation messages */
            'validation.country': 'Пожалуйста, укажите страну',
            'validation.profession': 'Пожалуйста, укажите вашу деятельность',
            'validation.term': 'Пожалуйста, укажите срок',

            /* Error messages */
            'error.submit': 'Произошла ошибка при отправке заявки',
            'error.prefix': 'Ошибка: ',

            /* Summary labels */
            'summary.hasPets.yes': 'Есть животные',
            'summary.hasPets.no': 'Нет животных',
            'summary.other': 'Другое',

            /* Language switcher */
            'lang.title': 'Язык'
        },

        en: {
            /* Tab bar */
            'tab.application': 'Application',
            'tab.applications': 'My Applications',
            'tab.profile': 'My Page',

            /* Application form — loading */
            'form.loading': 'Loading...',

            /* Application form — step 1 */
            'form.step1.title': 'Where are you from?',
            'form.country.russia': 'Russia',
            'form.country.belarus': 'Belarus',
            'form.country.ukraine': 'Ukraine',
            'form.country.other': 'Other',
            'form.countryOther.placeholder': 'Specify country',

            /* Application form — step 2 */
            'form.step2.title': 'What is your profession?',
            'form.profession.placeholder': 'e.g. IT specialist',

            /* Application form — step 3 */
            'form.step3.title': 'Do you have pets?',
            'form.pets.yes': 'Yes',
            'form.pets.no': 'No',

            /* Application form — step 4 */
            'form.step4.title': 'How long do you plan to rent?',
            'form.term.6months': '6 months',
            'form.term.1year': '1 year',
            'form.term.other': 'Other',
            'form.termOther.placeholder': 'Specify term',

            /* Application form — review */
            'form.review.title': 'Application sent!',
            'form.review.instructions': 'To complete your application, forward the apartment post from the {channel} channel to the bot chat.',

            /* Navigation buttons */
            'nav.next': 'Next',
            'nav.back': 'Back',
            'nav.submit': 'Submit',
            'nav.done': 'Done',

            /* Applications list */
            'apps.title': 'My Applications',
            'apps.empty': 'You have no applications yet',
            'apps.contactManager': 'Message manager',

            /* Profile */
            'profile.title': 'My Page',
            'profile.stat.applications': 'Applications',
            'profile.help.title': 'Need help?',
            'profile.help.text': 'Contact the manager or open the {channel} channel with listings',
            'profile.support': 'Support',
            'profile.guest': 'Guest',

            /* Modal */
            'modal.cancel': 'Cancel',
            'modal.confirm': 'Open',

            /* Confirmation messages */
            'confirm.openChannel': 'Open the {channel} channel in Telegram? The mini-app will be closed.',
            'confirm.contactManager': 'Message support @{username} on Telegram?',
            'confirm.contactManagerBtn': 'Message',
            'confirm.openChannelBtn': 'Open',

            /* Validation messages */
            'validation.country': 'Please specify your country',
            'validation.profession': 'Please specify your profession',
            'validation.term': 'Please specify the rental term',

            /* Error messages */
            'error.submit': 'An error occurred while submitting the application',
            'error.prefix': 'Error: ',

            /* Summary labels */
            'summary.hasPets.yes': 'Has pets',
            'summary.hasPets.no': 'No pets',
            'summary.other': 'Other',

            /* Language switcher */
            'lang.title': 'Language'
        },

        ka: {
            /* Tab bar */
            'tab.application': 'განაცხადი',
            'tab.applications': 'ჩემი განაცხადები',
            'tab.profile': 'ჩემი გვერდი',

            /* Application form — loading */
            'form.loading': 'იტვირთება...',

            /* Application form — step 1 */
            'form.step1.title': 'საიდან ხართ?',
            'form.country.russia': 'რუსეთი',
            'form.country.belarus': 'ბელარუსი',
            'form.country.ukraine': 'უკრაინა',
            'form.country.other': 'სხვა',
            'form.countryOther.placeholder': 'მიუთითეთ ქვეყანა',

            /* Application form — step 2 */
            'form.step2.title': 'რას აკეთებთ?',
            'form.profession.placeholder': 'მაგ. IT სპეციალისტი',

            /* Application form — step 3 */
            'form.step3.title': 'გაქვთ შინაური ცხოველები?',
            'form.pets.yes': 'დიახ',
            'form.pets.no': 'არა',

            /* Application form — step 4 */
            'form.step4.title': 'რა ვადით გინდათ ქირაობა?',
            'form.term.6months': '6 თვე',
            'form.term.1year': '1 წელი',
            'form.term.other': 'სხვა',
            'form.termOther.placeholder': 'მიუთითეთ ვადა',

            /* Application form — review */
            'form.review.title': 'განაცხადი გაგზავნილია!',
            'form.review.instructions': 'განაცხადის დასასრულებლად, გადააგზავნეთ ბინის პოსტი {channel} არხიდან ბოტის ჩატში.',

            /* Navigation buttons */
            'nav.next': 'შემდეგი',
            'nav.back': 'უკან',
            'nav.submit': 'გაგზავნა',
            'nav.done': 'მზადაა',

            /* Applications list */
            'apps.title': 'ჩემი განაცხადები',
            'apps.empty': 'ჯერ განაცხადები არ გაქვთ',
            'apps.contactManager': 'მენეჯერთან მიწერა',

            /* Profile */
            'profile.title': 'ჩემი გვერდი',
            'profile.stat.applications': 'განაცხადები',
            'profile.help.title': 'დახმარება გჭირდებათ?',
            'profile.help.text': 'დაუკავშირდით მენეჯერს ან გახსენით {channel} არხი განცხადებებით',
            'profile.support': 'მხარდაჭერა',
            'profile.guest': 'სტუმარი',

            /* Modal */
            'modal.cancel': 'გაუქმება',
            'modal.confirm': 'გახსნა',

            /* Confirmation messages */
            'confirm.openChannel': 'გავხსნა {channel} არხი Telegram-ში? მინი-აპი დაიხურება.',
            'confirm.contactManager': 'მივწერო მხარდაჭერას @{username} Telegram-ში?',
            'confirm.contactManagerBtn': 'მიწერა',
            'confirm.openChannelBtn': 'გახსნა',

            /* Validation messages */
            'validation.country': 'გთხოვთ მიუთითოთ ქვეყანა',
            'validation.profession': 'გთხოვთ მიუთითოთ თქვენი საქმიანობა',
            'validation.term': 'გთხოვთ მიუთითოთ ვადა',

            /* Error messages */
            'error.submit': 'განაცხადის გაგზავნისას მოხდა შეცდომა',
            'error.prefix': 'შეცდომა: ',

            /* Summary labels */
            'summary.hasPets.yes': 'აქვს ცხოველები',
            'summary.hasPets.no': 'არ აქვს ცხოველები',
            'summary.other': 'სხვა',

            /* Language switcher */
            'lang.title': 'ენა'
        }
    },

    /* --- Public API --- */

    t: function (key, params) {
        var dict = this._translations[this._currentLang] || this._translations.ru;
        var str = dict[key] || this._translations.ru[key] || key;
        if (params) {
            Object.keys(params).forEach(function (k) {
                str = str.replace('{' + k + '}', params[k]);
            });
        }
        return str;
    },

    getLang: function () {
        return this._currentLang;
    },

    getLangInfo: function () {
        var self = this;
        return this.languages.find(function (l) { return l.code === self._currentLang; }) || this.languages[0];
    },

    setLang: function (lang) {
        if (!this._translations[lang]) return;
        this._currentLang = lang;
        try { localStorage.setItem(this._storageKey, lang); } catch (e) {}
        this.applyTranslations();
        document.documentElement.lang = lang;
        this._updateDropdownToggle();
        this._syncToServer(lang);
    },

    init: function () {
        var saved = null;
        try { saved = localStorage.getItem(this._storageKey); } catch (e) {}
        if (saved && this._translations[saved]) {
            this._currentLang = saved;
        }
        document.documentElement.lang = this._currentLang;
        this.applyTranslations();
        this._updateDropdownToggle();
    },

    _updateDropdownToggle: function () {
        var info = this.getLangInfo();
        var flagEl = document.getElementById('lang-current-flag');
        var labelEl = document.getElementById('lang-current-label');
        if (flagEl) flagEl.textContent = info.flag;
        if (labelEl) labelEl.textContent = info.short;
    },

    /* --- DOM translation: elements with data-i18n get their textContent set --- */
    applyTranslations: function () {
        var self = this;
        document.querySelectorAll('[data-i18n]').forEach(function (el) {
            var key = el.getAttribute('data-i18n');
            el.textContent = self.t(key);
        });
        document.querySelectorAll('[data-i18n-placeholder]').forEach(function (el) {
            var key = el.getAttribute('data-i18n-placeholder');
            el.setAttribute('placeholder', self.t(key));
        });
        var event = new CustomEvent('i18n:changed', { detail: { lang: this._currentLang } });
        document.dispatchEvent(event);
    },

    /* --- Sync language to server so bot messages use the correct locale --- */
    _syncToServer: function (lang) {
        var tg = window.Telegram && window.Telegram.WebApp;
        if (!tg || !tg.initData) return;
        try {
            fetch(TmaConstants.ApiEndpoints.SetLanguage, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ initData: tg.initData, language: lang })
            }).catch(function () {});
        } catch (e) {}
    }
};

/* Expose for inline onclick handlers */
window.I18n = I18n;
