// US-10: "nothing is lost by navigating backwards".
//
// Forms marked data-save-on-leave (the capacity and rates steps) are saved before a step-bar
// link is followed: the form is posted to its Leave handler with the step to go to, and the
// server saves, validates and redirects — or keeps the custodian where they are with the
// reason, exactly as "Save and go back" does. Forms marked data-warn-on-leave (adding a cost
// or funding line, the platform details) have no single thing to save, so the browser asks
// before a half-typed entry is thrown away.
(function () {
    'use strict';

    var forms = Array.prototype.slice.call(document.querySelectorAll('form[data-save-on-leave], form[data-warn-on-leave]'));
    if (forms.length === 0) {
        return;
    }

    var changed = [];
    var leaving = false;

    function markChanged(form) {
        if (changed.indexOf(form) < 0) {
            changed.push(form);
        }
    }

    forms.forEach(function (form) {
        form.addEventListener('input', function () { markChanged(form); });
        form.addEventListener('change', function () { markChanged(form); });
        form.addEventListener('submit', function () { leaving = true; });
    });

    document.querySelectorAll('a.ric-step-link').forEach(function (link) {
        link.addEventListener('click', function (event) {
            var saver = changed.filter(function (form) { return form.hasAttribute('data-save-on-leave'); })[0];
            if (!saver) {
                return;
            }

            event.preventDefault();
            var action = new URL(saver.getAttribute('action') || window.location.href, window.location.href);
            action.searchParams.set('handler', 'Leave');
            action.searchParams.set('to', new URL(link.href, window.location.href).pathname);
            saver.setAttribute('action', action.pathname + action.search);
            leaving = true;
            saver.submit();
        });
    });

    window.addEventListener('beforeunload', function (event) {
        if (!leaving && changed.length > 0) {
            event.preventDefault();
            event.returnValue = '';
        }
    });
})();
