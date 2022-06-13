/*
 * min	0	Minimum value.
max	100	Maximum value.
step	1	Incremental/decremental step on up/down change.
stepinterval	100	Refresh rate of the spinner in milliseconds.
stepintervaldelay	500	Time in milliseconds before the spinner starts to spin.
prefix	""	Text before the input.
postfix	""	Text after the input.
booster	true	If enabled, the the spinner is continually becoming faster as holding the button.
boostat	10	Boost at every nth step.
maxboostedstep	false	Maximum step when boosted.
 */

(function ($) {
    'use strict';

    var _currentSpinnerId = 0;
    var _currentVal = 0;
    function _scopedEventName(name, id) {
        return name + '.touchspin_' + id;
    }

    function _scopeEventNames(names, id) {
        return $.map(names, function (name) {
            return _scopedEventName(name, id);
        });
    }

    $.fn.TouchSpin = function (options, onChage, onStopSpin) {

        if (options === 'destroy') {
            this.each(function () {
                var container = $(this),
                    container_data = container.data();
                $(document).off(_scopeEventNames([
                  'mouseup',
                  'touchend',
                  'touchcancel',
                  'mousemove',
                  'touchmove',
                  'scroll',
                  'scrollstart'], container_data.spinnerid).join(' '));
            });
            return;
        }

        var defaults = {
            min: 0,
            max: 100,
            initval: '',
            replacementval: '',
            step: 1,
            decimals: 0,
            stepinterval: 100,
            forcestepdivisibility: 'round', // none | floor | round | ceil
            stepintervaldelay: 500,
            booster: true,
            boostat: 10,
            maxboostedstep: false,
            mousewheel: true,
            buttondown_class: 'btn btn-default',
            buttonup_class: 'btn btn-default',
            buttondown_txt: '-',
            buttonup_txt: '+'
        };

        var attributeMap = {
            min: 'min',
            max: 'max',
            initval: 'init-val',
            replacementval: 'replacement-val',
            step: 'step',
            decimals: 'decimals',
            stepinterval: 'step-interval',
            forcestepdivisibility: 'force-step-divisibility',
            stepintervaldelay: 'step-interval-delay',
            booster: 'booster',
            boostat: 'boostat',
            maxboostedstep: 'max-boosted-step',
            mousewheel: 'mouse-wheel',
            buttondown_class: 'button-down-class',
            buttonup_class: 'button-up-class',
            buttondown_txt: 'button-down-txt',
            buttonup_txt: 'button-up-txt'
        };

        return this.each(function () {

            var settings,
                container = $(this),
                container_data = container.data(),
                container,
                elements,
                value,
                downSpinTimer,
                upSpinTimer,
                downDelayTimeout,
                upDelayTimeout,
                spincount = 0,
                spinning = false;

            init();


            function init() {
                if (container.data('alreadyinitialized')) {
                    return;
                }

                container.data('alreadyinitialized', true);
                _currentSpinnerId += 1;
                container.data('spinnerid', _currentSpinnerId);


               

                _initSettings();
                _initElements();
                _setInitval();
                _checkValue();
                
                
                _bindEvents();
                _bindEventsInterface();
                
            }

            function _setInitval() {
                if (settings.initval !== '') {
                    _currentVal = settings.initval;
                    fireChangeEvent();
                    
                }
            }

            function changeSettings(newsettings) {
                _updateSettings(newsettings);
                _checkValue();

                
            }

            function _initSettings() {
                settings = $.extend({}, defaults, container_data, _parseAttributes(), options);
            }

            function _parseAttributes() {
                var data = {};
                $.each(attributeMap, function (key, value) {
                    var attrName = 'bts-' + value + '';
                    if (container.is('[data-' + attrName + ']')) {
                        data[key] = container.data(attrName);
                    }
                });
                return data;
            }

            function _updateSettings(newsettings) {
                settings = $.extend({}, settings, newsettings);
            }

            

            function _initElements() {
                elements = {
                    down: $('.bootstrap-touchspin-down', container),
                    up: $('.bootstrap-touchspin-up', container),
                    input: $('input', container)
                };
            }

            

            function _bindEvents() {
                elements.input.on('keydown', function (ev) {
                    var code = ev.keyCode || ev.which;

                    if (code === 38) {
                        if (spinning !== 'up') {
                            upOnce();
                            startUpSpin();
                        }
                        ev.preventDefault();
                    }
                    else if (code === 40) {
                        if (spinning !== 'down') {
                            downOnce();
                            startDownSpin();
                        }
                        ev.preventDefault();
                    }
                });

                elements.input.on('keyup', function (ev) {
                    var code = ev.keyCode || ev.which;

                    if (code === 38) {
                        stopSpin();
                    }
                    else if (code === 40) {
                        stopSpin();
                    }
                });

                elements.input.on('blur', function () {
                    _checkValue();
                });

                elements.down.on('keydown', function (ev) {
                    var code = ev.keyCode || ev.which;

                    if (code === 32 || code === 13) {
                        if (spinning !== 'down') {
                            downOnce();
                            startDownSpin();
                        }
                        ev.preventDefault();
                    }
                });

                elements.down.on('keyup', function (ev) {
                    var code = ev.keyCode || ev.which;

                    if (code === 32 || code === 13) {
                        stopSpin();
                    }
                });

                elements.up.on('keydown', function (ev) {
                    var code = ev.keyCode || ev.which;

                    if (code === 32 || code === 13) {
                        if (spinning !== 'up') {
                            upOnce();
                            startUpSpin();
                        }
                        ev.preventDefault();
                    }
                });

                elements.up.on('keyup', function (ev) {
                    var code = ev.keyCode || ev.which;

                    if (code === 32 || code === 13) {
                        stopSpin();
                    }
                });

                elements.down.on('mousedown.touchspin', function (ev) {
                    elements.down.off('touchstart.touchspin');  // android 4 workaround

                    if (elements.input.is(':disabled')) {
                        return;
                    }

                    downOnce();
                    startDownSpin();

                    ev.preventDefault();
                    ev.stopPropagation();
                });

                elements.down.on('touchstart.touchspin', function (ev) {
                    elements.down.off('mousedown.touchspin');  // android 4 workaround

                    if (elements.input.is(':disabled')) {
                        return;
                    }

                    downOnce();
                    startDownSpin();

                    ev.preventDefault();
                    ev.stopPropagation();
                });

                elements.up.on('mousedown.touchspin', function (ev) {
                    elements.up.off('touchstart.touchspin');  // android 4 workaround

                    if (elements.input.is(':disabled')) {
                        return;
                    }

                    upOnce();
                    startUpSpin();

                    ev.preventDefault();
                    ev.stopPropagation();
                });

                elements.up.on('touchstart.touchspin', function (ev) {
                    elements.up.off('mousedown.touchspin');  // android 4 workaround

                    if (elements.input.is(':disabled')) {
                        return;
                    }

                    upOnce();
                    startUpSpin();

                    ev.preventDefault();
                    ev.stopPropagation();
                });

                elements.up.on('mouseout touchleave touchend touchcancel', function (ev) {
                    if (!spinning) {
                        return;
                    }

                    ev.stopPropagation();
                    stopSpin();
                });

                elements.down.on('mouseout touchleave touchend touchcancel', function (ev) {
                    if (!spinning) {
                        return;
                    }

                    ev.stopPropagation();
                    stopSpin();
                });

                elements.down.on('mousemove touchmove', function (ev) {
                    if (!spinning) {
                        return;
                    }

                    ev.stopPropagation();
                    ev.preventDefault();
                });

                elements.up.on('mousemove touchmove', function (ev) {
                    if (!spinning) {
                        return;
                    }

                    ev.stopPropagation();
                    ev.preventDefault();
                });

                $(document).on(_scopeEventNames(['mouseup', 'touchend', 'touchcancel'], _currentSpinnerId).join(' '), function (ev) {
                    if (!spinning) {
                        return;
                    }

                    ev.preventDefault();
                    stopSpin();
                });

                $(document).on(_scopeEventNames(['mousemove', 'touchmove', 'scroll', 'scrollstart'], _currentSpinnerId).join(' '), function (ev) {
                    if (!spinning) {
                        return;
                    }

                    ev.preventDefault();
                    stopSpin();
                });

                elements.input.on('mousewheel DOMMouseScroll', function (ev) {
                    if (!settings.mousewheel || !elements.input.is(':focus')) {
                        return;
                    }

                    var delta = ev.originalEvent.wheelDelta || -ev.originalEvent.deltaY || -ev.originalEvent.detail;

                    ev.stopPropagation();
                    ev.preventDefault();

                    if (delta < 0) {
                        downOnce();
                    }
                    else {
                        upOnce();
                    }
                });
            }

            function _bindEventsInterface() {
                container.on('touchspin.uponce', function () {
                    stopSpin();
                    upOnce();
                });

                container.on('touchspin.downonce', function () {
                    stopSpin();
                    downOnce();
                });

                container.on('touchspin.startupspin', function () {
                    startUpSpin();
                });

                container.on('touchspin.startdownspin', function () {
                    startDownSpin();
                });

                container.on('touchspin.stopspin', function () {
                    stopSpin();
                });

                container.on('touchspin.updatesettings', function (e, newsettings) {
                    changeSettings(newsettings);
                });

                container.on('touchspin.updateText', function (e) {
                    elements.input.val(e.text);
                });

                container.on('touchspin.updateVal', function (e) {
                    _currentVal = e.value;
                    elements.input.val(e.text);
                });
            }

            function _forcestepdivisibility(value) {
                switch (settings.forcestepdivisibility) {
                    case 'round':
                        return (Math.round(value / settings.step) * settings.step).toFixed(settings.decimals);
                    case 'floor':
                        return (Math.floor(value / settings.step) * settings.step).toFixed(settings.decimals);
                    case 'ceil':
                        return (Math.ceil(value / settings.step) * settings.step).toFixed(settings.decimals);
                    default:
                        return value;
                }
            }

            function _checkValue() {
                var val, parsedval, returnval;

                val = _currentVal;

                if (val === '') {
                    if (settings.replacementval !== '') {
                        _currentVal = settings.replacementval;
                        fireChangeEvent();
                    }
                    return;
                }

                if (settings.decimals > 0 && val === '.') {
                    return;
                }

                parsedval = parseFloat(val);

                if (isNaN(parsedval)) {
                    if (settings.replacementval !== '') {
                        parsedval = settings.replacementval;
                    }
                    else {
                        parsedval = 0;
                    }
                }

                returnval = parsedval;

                if (parsedval.toString() !== val) {
                    returnval = parsedval;
                }

                if (parsedval < settings.min) {
                    returnval = settings.min;
                }

                if (parsedval > settings.max) {
                    returnval = settings.max;
                }

                returnval = _forcestepdivisibility(returnval);

                if (Number(val).toString() !== returnval.toString()) {
                    
                    fireChangeEvent();
                }
            }

            function _getBoostedStep() {
                if (!settings.booster) {
                    return settings.step;
                }
                else {
                    var boosted = Math.pow(2, Math.floor(spincount / settings.boostat)) * settings.step;

                    if (settings.maxboostedstep) {
                        if (boosted > settings.maxboostedstep) {
                            boosted = settings.maxboostedstep;
                            value = Math.round((value / boosted)) * boosted;
                        }
                    }

                    return Math.max(settings.step, boosted);
                }
            }

            function upOnce() {
                _checkValue();

                value = parseFloat(_currentVal);
                if (isNaN(value)) {
                    value = 0;
                }

                var initvalue = value,
                    boostedstep = _getBoostedStep();

                value = value + boostedstep;

                if (value > settings.max) {
                    value = settings.max;
                    container.trigger('touchspin.on.max');
                    stopSpin();
                }

                _currentVal = Number(value).toFixed(settings.decimals);

                if (initvalue !== value) {
                    fireChangeEvent();
                }
            }

            function downOnce() {
                _checkValue();

                value = parseFloat(_currentVal);
                if (isNaN(value)) {
                    value = 0;
                }

                var initvalue = value,
                    boostedstep = _getBoostedStep();

                value = value - boostedstep;

                if (value < settings.min) {
                    value = settings.min;
                    container.trigger('touchspin.on.min');
                    stopSpin();
                }

                _currentVal = value.toFixed(settings.decimals);

                if (initvalue !== value) {
                    fireChangeEvent();
                }
            }

            function startDownSpin() {
                stopSpin();

                spincount = 0;
                spinning = 'down';

                container.trigger('touchspin.on.startspin');
                container.trigger('touchspin.on.startdownspin');

                downDelayTimeout = setTimeout(function () {
                    downSpinTimer = setInterval(function () {
                        spincount++;
                        downOnce();
                    }, settings.stepinterval);
                }, settings.stepintervaldelay);
            }

            function startUpSpin() {
                stopSpin();

                spincount = 0;
                spinning = 'up';

                container.trigger('touchspin.on.startspin');
                container.trigger('touchspin.on.startupspin');

                upDelayTimeout = setTimeout(function () {
                    upSpinTimer = setInterval(function () {
                        spincount++;
                        upOnce();
                    }, settings.stepinterval);
                }, settings.stepintervaldelay);
            }

            function stopSpin() {
                clearTimeout(downDelayTimeout);
                clearTimeout(upDelayTimeout);
                clearInterval(downSpinTimer);
                clearInterval(upSpinTimer);

                switch (spinning) {
                    case 'up':
                        container.trigger('touchspin.on.stopupspin');
                        container.trigger('touchspin.on.stopspin');
                        break;
                    case 'down':
                        container.trigger('touchspin.on.stopdownspin');
                        container.trigger('touchspin.on.stopspin');
                        break;
                }

                spincount = 0;
                spinning = false;
                fireStopSpin();
            }


            function fireChangeEvent() {
                if ($.isFunction(onChage)) {
                    onChage(_currentVal);
                }
                else {
                    elements.input.val(_currentVal);
                };
            }

            function fireStopSpin() {
                if ($.isFunction(onStopSpin)) {
                    setTimeout(function () {
                        if (!spinning) {
                            onStopSpin(_currentVal);
                        }
                    }, 500);
                }
                
            }

           
        });

    };

})(jQuery);