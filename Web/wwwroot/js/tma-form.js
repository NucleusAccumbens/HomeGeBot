/* ===== TMA Application Form =====
 * Manages the multi-step rental application wizard.
 * Single responsibility: form state, step navigation, validation, and submission.
 * Depends on: TmaConstants, TmaUtils, ModalController
 */

const ApplicationForm = {
    _currentStep: 1,
    _prefilledData: null,

    /* --- Field value getters --- */

    getCountryDisplay: function () {
        var val = parseInt(document.getElementById('country').value);
        if (val === TmaConstants.CountryOtherValue) {
            var other = document.getElementById('countryOther').value.trim();
            return other || I18n.t('summary.other');
        }
        return TmaConstants.CountryNames[val] || '';
    },

    getTermDisplay: function () {
        var val = parseInt(document.getElementById('term').value);
        if (val === TmaConstants.TermOtherValue) {
            var other = document.getElementById('termOther').value.trim();
            return other || I18n.t('summary.other');
        }
        return TmaConstants.TermNames[val] || '';
    },

    getProfessionDisplay: function () {
        return document.getElementById('profession').value.trim() || '—';
    },

    getHasPetsDisplay: function () {
        return document.getElementById('hasPets').value === 'true' ? I18n.t('form.pets.yes') : I18n.t('form.pets.no');
    },

    /* --- Conditional field toggling --- */

    toggleOther: function (selectId, wrapId) {
        var sel = document.getElementById(selectId);
        var wrap = document.getElementById(wrapId);
        if (!sel || !wrap) return;

        if (selectId === 'country') {
            wrap.style.display = sel.value === String(TmaConstants.CountryOtherValue) ? 'block' : 'none';
        } else if (selectId === 'term') {
            wrap.style.display = sel.value === String(TmaConstants.TermOtherValue) ? 'block' : 'none';
        }
    },

    /* --- Step navigation --- */

    _totalSteps: 4,

    showStep: function (step) {
        var view = document.getElementById('view-application');
        if (!view) return;
        view.querySelectorAll('.step').forEach(function (s) { s.classList.remove('active'); });
        var el = document.getElementById('step' + step) || document.getElementById(step);
        if (el) el.classList.add('active');
        this._currentStep = step;
        this._updateCounter(step);
        if (typeof step === 'number') this._renderSummary(step);
        if (typeof NavigationController !== 'undefined') {
            NavigationController._showNavStep(step);
        }
    },

    _updateCounter: function (step) {
        var counter = document.getElementById('step-counter');
        if (!counter) return;
        if (typeof step === 'number' && step >= 1 && step <= this._totalSteps) {
            counter.textContent = step + '/' + this._totalSteps;
            counter.style.display = 'block';
        } else {
            counter.style.display = 'none';
        }
    },

    nextStep: function (step) {
        this.showStep(step);
    },

    prevStep: function (step) {
        this.showStep(step);
    },

    /* --- Summary rendering --- */

    _renderSummary: function (step) {
        var parts = [];
        if (step > 1) parts.push('🌍 ' + this.getCountryDisplay());
        if (step > 2) parts.push('💼 ' + this.getProfessionDisplay());
        if (step > 3) parts.push('🐾 ' + this.getHasPetsDisplay());
        var el = document.getElementById('summary' + step);
        if (el) {
            el.innerHTML = parts.length > 0
                ? '<div class="summary-list">' + parts.map(function (p) {
                    return '<div class="summary-item">' + p + '</div>';
                }).join('') + '</div>'
                : '';
        }
    },

    _renderReviewSummary: function (data) {
        var parts = [
            '🌍 ' + (data.country === TmaConstants.CountryOtherValue && data.countryOther ? data.countryOther : TmaConstants.CountryNames[data.country]),
            '💼 ' + data.profession,
            '🐾 ' + (data.hasPets ? I18n.t('form.pets.yes') : I18n.t('form.pets.no')),
            '🗓️ ' + (data.term === TmaConstants.TermOtherValue && data.termOther ? data.termOther : TmaConstants.TermNames[data.term])
        ];
        var el = document.getElementById('review-summary');
        if (el) {
            el.innerHTML = '<div class="summary-list">'
                + parts.map(function (p) { return '<div class="summary-item">' + TmaUtils.escapeHtml(p) + '</div>'; }).join('')
                + '</div>';
        }
    },

    /* --- Prefill from existing data --- */

    prefillFromData: function (data) {
        if (!data) return;
        this._prefilledData = data;
        if (data.country != null) {
            document.getElementById('country').value = data.country;
            this.toggleOther('country', 'countryOtherWrap');
        }
        if (data.countryOther) document.getElementById('countryOther').value = data.countryOther.substring(0, 100);
        if (data.profession) document.getElementById('profession').value = data.profession.substring(0, 100);
        if (data.hasPets != null) document.getElementById('hasPets').value = data.hasPets ? 'true' : 'false';
        if (data.term != null) {
            document.getElementById('term').value = data.term;
            this.toggleOther('term', 'termOtherWrap');
        }
        if (data.termOther) document.getElementById('termOther').value = data.termOther.substring(0, 100);
    },

    resetToStep1: function () {
        this.prefillFromData(this._prefilledData);
        this.showStep(1);
    },

    /* --- Validation --- */

    _validate: function () {
        var countryVal = parseInt(document.getElementById('country').value);
        var termVal = parseInt(document.getElementById('term').value);
        var tg = window.Telegram.WebApp;

        if (countryVal === TmaConstants.CountryOtherValue && !document.getElementById('countryOther').value.trim()) {
            tg.showAlert(I18n.t('validation.country'));
            return false;
        }

        var professionVal = document.getElementById('profession').value.trim();
        if (!professionVal) {
            tg.showAlert(I18n.t('validation.profession'));
            return false;
        }

        if (termVal === TmaConstants.TermOtherValue && !document.getElementById('termOther').value.trim()) {
            tg.showAlert(I18n.t('validation.term'));
            return false;
        }

        return true;
    },

    /* --- Submission --- */

    _setButtonLoading: function (btn, loading) {
        if (!btn) return;
        if (loading) {
            btn.disabled = true;
            btn._originalHTML = btn.innerHTML;
            btn.innerHTML = '<span class="btn-spinner"></span>';
        } else {
            btn.disabled = false;
            if (btn._originalHTML) btn.innerHTML = btn._originalHTML;
        }
    },

    submit: async function () {
        if (!this._validate()) return;

        var submitBtn = document.querySelector('.nav-buttons-set[data-nav-step="4"] .btn-primary');
        this._setButtonLoading(submitBtn, true);

        var countryVal = parseInt(document.getElementById('country').value);
        var termVal = parseInt(document.getElementById('term').value);
        var tg = window.Telegram.WebApp;

        var data = {
            country: countryVal,
            countryOther: countryVal === TmaConstants.CountryOtherValue ? document.getElementById('countryOther').value.trim() : null,
            profession: document.getElementById('profession').value.trim(),
            hasPets: document.getElementById('hasPets').value === 'true',
            term: termVal,
            termOther: termVal === TmaConstants.TermOtherValue ? document.getElementById('termOther').value.trim() : null,
            initData: tg.initData
        };

        try {
            var response = await fetch(TmaConstants.ApiEndpoints.SubmitApplication, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                this._setButtonLoading(submitBtn, false);
                tg.close();
            } else {
                var error = await response.text();
                tg.showAlert(I18n.t('error.prefix') + error);
            }
        } catch (e) {
            tg.showAlert(I18n.t('error.submit'));
        } finally {
            this._setButtonLoading(submitBtn, false);
        }
    }
};

/* Expose for inline onclick handlers */
window.toggleOther = function (selectId, wrapId) { ApplicationForm.toggleOther(selectId, wrapId); };
window.blockNewline = function (e) { TmaUtils.blockNewline(e); };
window.stripNewlines = function (el) { TmaUtils.stripNewlines(el); };
window.nextStep = function (step) { ApplicationForm.nextStep(step); };
window.prevStep = function (step) { ApplicationForm.prevStep(step); };
window.submitForm = function () { ApplicationForm.submit(); };
window.doneCloseApp = function () {
    var btn = document.querySelector('.nav-buttons-set[data-nav-step="review"] .btn-primary');
    ApplicationForm._setButtonLoading(btn, true);
    setTimeout(function () {
        var tg = window.Telegram.WebApp;
        tg.close();
    }, 500);
};
