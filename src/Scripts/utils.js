
try {
	if (!String.prototype.trim) {
		String.prototype.trim = function () {
			return this.replace(/^[\s\uFEFF\xA0]+|[\s\uFEFF\xA0]+$/g, '');
		};
	}
} catch (e) { }

var _dateTimeFormat = {
    DATA: { MASK: '99/99/9999', FORMAT: 'DD/MM/YYYY' },
    DATA_ORA_MIN: { MASK: '99/99/9999 99:99', FORMAT: 'DD/MM/YYYY HH:mm' },
    DATA_ORA_MIN_SEC: { MASK: '99/99/9999 99:99:99', FORMAT: 'DD/MM/YYYY HH:mm:ss' }
};

var NumberGroupSeparator = '.';
var NumberDecimalSeparator = ',';

var stringEmpty = '';
// For todays date;
Date.prototype.today = function () {
    return ((this.getDate() < 10) ? "0" : "") + this.getDate() + "/" + (((this.getMonth() + 1) < 10) ? "0" : "") + (this.getMonth() + 1) + "/" + this.getFullYear();
}

// For the time now
Date.prototype.timeNow = function () {
    return ((this.getHours() < 10) ? "0" : "") + this.getHours() + ":" + ((this.getMinutes() < 10) ? "0" : "") + this.getMinutes() + ":" + ((this.getSeconds() < 10) ? "0" : "") + this.getSeconds();
}

$(document).ready(function () {
    try {
        $.views.helpers(
            {
                jsFormatBoolean: function (val) {
                    return formatBoolean(val, 'SI', 'NO');
                },
                jsFormatDouble: function (arg) { return formatDouble(arg); },
                jsConvertISODate: convertISODate,
                jsIsEmpty: isEmpty
            }
        );
    }
    catch (e) {
		//
    }

     // Prevent the backspace key from navigating back.
    $(document).unbind('keydown').bind('keydown', function (event) {
        var doPrevent = false;
        if (event.keyCode === 8) {
            var d = event.srcElement || event.target;
            if (
                d.tagName.toUpperCase() === 'INPUT' &&
                d.type.toUpperCase() === 'TEXT' ||
                d.type.toUpperCase() === 'PASSWORD' ||
                d.type.toUpperCase() === 'FILE' ||
                d.type.toUpperCase() === 'EMAIL' ||
                d.tagName.toUpperCase() === 'TEXTAREA') {
                doPrevent = d.readOnly || d.disabled;
            }

        }

        if (doPrevent) {
            event.preventDefault();
        }
    });

    $(document).on('click', ':submit', function () {
		var $btn = $(this);
		$btn.hide(0, function () {
			var wait = $('<a href="#">Attendere...</span>').attr({ 'class': $btn.attr('class') }).addClass('disabled');
			$(wait).insertBefore($btn);
		});
    });


    $(document).on('input', '[required]', function () {
        $(this).closest('.form-group').removeClass('has-error');
    });

    
    $('input').each(function () {
        switch ($(this).data('type')) {
            case 'email':
            case 'codice-fiscale':
            case 'partita-iva':
            case 'data-ora':
                $(this).validInputChecker();
                break;
        }
    });

	$(document).on(
		{
			click: function () { $(this).select(); },
			keydown: onkeydown_OnlyNumber,
			focus: function () {
				if ($(this).hasClass('decimal')) {

					this.value = unformatNumber(this.value);
				}
			},
			blur: function () {
				if ($(this).hasClass('decimal')) {

					if (NumberDecimalSeparator == ',') {
						this.value = this.value.replace(/,/g, '.');
					}

					$(this).val(formatNumber(this.value));
				}
			}
		}, 'input.numeric');

    //$(document).on('blur', 'input[type=text]', function () {
    //    this.value = this.value.toUpperCase();
    //});
});

// NUOVE FUNZIONI DI FORMATTAZIONE DEI NUMERI
function formatNumber(num, precision) {
	if (precision === undefined) precision = 2;

	return accounting.formatNumber(num, precision, NumberGroupSeparator, NumberDecimalSeparator)
}

function unformatNumber(text) {
	var num = accounting.unformat(text, NumberDecimalSeparator);
	return num.toString().replace('.', NumberDecimalSeparator);
}
//
function onkeydown_OnlyNumber(e) {
	var allowDecimal = $(this).hasClass('decimal');
	var allowNegative = $(this).hasClass('allow-negative');
	// allow function keys and decimal separators
	if (
		// backspace, delete, tab, escape, enter
		$.inArray(e.keyCode, [46, 8, 9, 27, 13]) !== -1 ||
		// comma and dot
		$.inArray(e.keyCode, [110, 188, 190]) !== -1 && allowDecimal && $(this).val().indexOf(NumberDecimalSeparator) === -1 ||
		// minus
		e.keyCode === 45 !== -1 && allowNegative && e.target.selectionStart == 0 && $(this).val().indexOf('-') === -1 ||
		// Ctrl/cmd+A, Ctrl/cmd+C, Ctrl/cmd+X
		$.inArray(e.keyCode, [65, 67, 88]) !== -1 && (e.ctrlKey === true || e.metaKey === true) ||
		// home, end, left, right
		e.keyCode >= 35 && e.keyCode <= 39) {

        /*
        // optional: replace commas with dots in real-time (for en-US locals)
        
		*/
		if (allowDecimal && NumberDecimalSeparator === '.') {
			if (e.keyCode === 188) {
				e.preventDefault();
				$(this).val($(this).val() + NumberDecimalSeparator);
			}
		}
		if (allowDecimal && NumberDecimalSeparator === ',') {

			// replace decimal points (num pad) and dots with commas in real-time (for EU locals)
			if (e.keyCode === 110 || e.keyCode === 190) {
				e.preventDefault();
				$(this).val($(this).val() + NumberDecimalSeparator);
			}
		}

		//if (e.keyCode === 46) {
		//    if ($(this).val().indexOf('.') === -1) {
		//        return;
		//    } else {
		//        e.preventDefault();
		//    }
		//}

		return;
	}
	// block any non-number
	if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
		e.preventDefault();
	}
}

(function () {

	if (typeof window.CustomEvent === "function") return false;

	function CustomEvent(event, params) {
		params = params || { bubbles: false, cancelable: false, detail: undefined };
		var evt = document.createEvent('CustomEvent');
		evt.initCustomEvent(event, params.bubbles, params.cancelable, params.detail);
		return evt;
	}

	CustomEvent.prototype = window.Event.prototype;

	window.CustomEvent = CustomEvent;
})();


function callWebMethod(methodUrl, postData, onSuccess, onError, onComplete) {
    $.ajax({
        type: "POST",
        url: methodUrl,
        data: JSON.stringify(postData),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if ($.isFunction(onSuccess)) {
                onSuccess(response);
            }

        },
        error: function (xhr, ajaxOptions, thrownError) {
            var msg = '';
            try {
                msg = $.parseJSON(xhr.responseText).Message;
            }
            catch (e) {
                msg = thrownError;
            }

            if ($.isFunction(onError)) {
                onError(msg);
            }
            else {
                alert(methodUrl + '\n' + msg);
            }
        },
        complete: function (response) {
            if ($.isFunction(onComplete)) {
                onComplete(response);
            }
        }
    });
}

function syncAjax(methodUrl, postData, onSuccess){
    $.ajax({
        type: "POST",
        url: methodUrl,
        data: JSON.stringify(postData),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: false,
        success: function (response) {
            if ($.isFunction(onSuccess)) {
                onSuccess(response);
            }

        },
        error: function (xhr, ajaxOptions, thrownError) {
            var msg = '';
            try {
                msg = $.parseJSON(xhr.responseText).Message;
            }
            catch (e) {
                msg = thrownError;
            }

            alert(methodUrl + '\n' + msg);
            
        },
        complete: function (response) {
            
        }
    });
}


function getPropValue(obj, prop) {
    var val = '';
    for (var p in obj) {
        if (obj.hasOwnProperty(p) && prop.toLowerCase() == (p).toLowerCase()) {
            val = obj[p]
            break;
        }
    }
    return val;
}

function setPropValue(obj, prop, val) {

}

function notify(dialogRef, msg, className) {
	var $p = $('<p>')
		.addClass(className)
		.html(msg);

	dialogRef.getModalBody().append($p);
	$p.css({
		'position': 'absolute',
		'left': '0',
		'top': '0',
		'width': '100%',
		'height': 'auto',
		'padding': '15px',
		'z-index': '10'
	});

	if (className == 'bg-danger') {
		$p.append(
			$('<button>').attr({ 'type': 'button' }).addClass('close').html('x').click(function () {
				$(this).closest('p').fadeOut('slow', function () { $(this).remove(); });
			})
		);

	}
	else {
		$p.show().delay(2000).fadeOut('slow', function () { $(this).remove(); });
	}
}

function alertWarning(msg) {
    bsAlert('ATTENZIONE', BootstrapDialog.TYPE_WARNING, msg);
}

function alertDanger(msg) {
    bsAlert('ERRORE', BootstrapDialog.TYPE_DANGER, msg);
}



function bsAlert(title, type, message) {
    BootstrapDialog.alert({
        title: title,
        message: message,
        type: type,
        closable: true,
        draggable: false,
        buttonLabel: 'Ok!',
        callback: function (result) {
            // result will be true if button was click, while it will be false if users close the dialog directly.

        }
    });
}

function bsConfirm(title, message, fn_Ok, fn_Cancel) {
    BootstrapDialog.confirm({
        title: title,
        message: message,
        type: BootstrapDialog.TYPE_WARNING, // <-- Default value is BootstrapDialog.TYPE_PRIMARY
        closable: true, // <-- Default value is false
        draggable: true, // <-- Default value is false
        btnCancelLabel: 'Annulla', // <-- Default value is 'Cancel',
        btnOKLabel: 'Conferma', // <-- Default value is 'OK',
        btnOKClass: 'btn-warning', // <-- If you didn't specify it, dialog type will be used,
        callback: function (result) {
            // result will be true if button was click, while it will be false if users close the dialog directly.
            if (result) {
                if ($.isFunction(fn_Ok)) {
                    fn_Ok();
                }
            } else {
                if ($.isFunction(fn_Cancel)) {
                    fn_Cancel();
                }
            }
        }
    });
}

function isEmpty(obj) {
    if (typeof obj == 'undefined' || obj === null || obj === '') return true;
    if (typeof obj == 'number' && isNaN(obj)) return true;
    if (obj instanceof Date && isNaN(Number(obj))) return true;
    return false;
}

function notEmpty(currentValue, defaultIfEmpty) {
	defaultIfEmpty = isEmpty(defaultIfEmpty) ? ' ' : defaultIfEmpty;
	if (isEmpty(currentValue)) {

		return defaultIfEmpty;
	}
	else {
		return currentValue;
	}

}

function convertISODate(isoDate) {
    if (moment(isoDate, 'YYYY-MM-DDTHH:mm:SSZ').isValid()) {
        return moment(isoDate, 'YYYY-MM-DDTHH:mm:SSZ').format(_dateTimeFormat.DATA.FORMAT);
    }
    else {
        
        
        return '';
        
        
    }
}

function parseDouble(num) {
    if (isEmpty(num)) {
        return 0;
    }
    else {
        if (nfi.NumberDecimalSeparator == ',') {
            num = num.toString().replace(/,([^,]*)$/, ".$1");
        }
        return num;
    }

    
}

function formatDouble(num, precision) {
    if (isEmpty(precision)) {
        precision = 2;
    }

    var n = accounting.formatNumber(parseDouble(num), precision, nfi.NumberGroupSeparator, nfi.NumberDecimalSeparator);

    if (precision > 2) {
        var z = 0;
        var p = n.indexOf('.') + 2;
        var i = n.length - 1; //or 10
        while (i > p) {

            if (n.charAt(i) != '0') {
                break;
            }
            else {
                z += 1;
            }
            i--;
        }

        n = n.substring(0, n.length - z);

    }

    return n;
}

function unformatDouble(num) {

    //console.log('unformatDouble: ' + num);
    var n = accounting.unformat(num, nfi.NumberDecimalSeparator);
    //console.log('unformatDouble: ' + n);
    return parseFloat(n);
}

function roundNumber(number, precision) {
    if (isEmpty(precision)) {
        precision = 2;
    }
    precision = Math.abs(parseInt(precision)) || 0;
    var multiplier = Math.pow(10, precision);
    return (Math.round(number * multiplier) / multiplier);
}

function parseBoolean(b) {
    return b == true || b == 'True' || b == 'true' || b == 1 || b == '1';
}

function formatBoolean(b, trueValue, falseValue) {
    if (isEmpty(b)) { b = false; }
    
    return parseBoolean(b) ? trueValue : falseValue;
    
}

function getUrlParameter(name) {
    name = name.replace(/[\[]/, '\\[').replace(/[\]]/, '\\]');
    var regex = new RegExp('[\\?&]' + name + '=([^&#]*)');
    var results = regex.exec(location.search);
    return results === null ? '' : decodeURIComponent(results[1].replace(/\+/g, ' '));
};


function checkRequired(context) {
    var is_valid = true;
    $('[required]', context).each(function () {
        $(this).closest('.form-group').removeClass('has-error');
        if ($(this).val() == '') {
            $(this).closest('.form-group').addClass('has-error');
            is_valid = false;
        }
    });
    return is_valid;
}

function checkValidInput(context) {
    var is_valid = true;
    $('input.validInputChecker', context).each(function () {
        if (!$(this).data('validInputChecker').validate()) {
            is_valid = false;
        }
    });
    return is_valid;
}

function copyVal(srcId, dstId) {
    $(dstId).val($(srcId).val());
}

function setErrorState() {

}
/*
bindWithDelay jQuery plugin
Author: Brian Grinstead
MIT license: http://www.opensource.org/licenses/mit-license.php
http://github.com/bgrins/bindWithDelay
http://briangrinstead.com/files/bindWithDelay
Usage:
    See http://api.jquery.com/bind/
    .bindWithDelay( eventType, [ eventData ], handler(eventObject), timeout, throttle )
Examples:
    $("#foo").bindWithDelay("click", function(e) { }, 100);
    $(window).bindWithDelay("resize", { optional: "eventData" }, callback, 1000);
    $(window).bindWithDelay("resize", callback, 1000, true);
*/

(function ($) {

  $.fn.bindWithDelay = function (type, data, fn, timeout, throttle) {

        if ($.isFunction(data)) {
            throttle = timeout;
            timeout = fn;
            fn = data;
            data = undefined;
        }

        // Allow delayed function to be removed with fn in unbind function
        fn.guid = fn.guid || ($.guid && $.guid++);

        // Bind each separately so that each element has its own delay
        return this.each(function () {

            var wait = null;

            function cb() {
                var e = $.extend(true, {}, arguments[0]);
                var ctx = this;
                var throttler = function () {
                    wait = null;
                    fn.apply(ctx, [e]);
                };

                if (!throttle) { clearTimeout(wait); wait = null; }
                if (!wait) { wait = setTimeout(throttler, timeout); }
            }

            cb.guid = fn.guid;

            $(this).bind(type, data, cb);
        });
    };

})(jQuery);



//table-sortable ---
//$(document).on('click', 'th.sortable', function () {
//    var $this = $(this);
//    var $tr = $this.closest('tr');
//    var asc = $this.hasClass('asc');
//    var desc = $this.hasClass('desc');
//    $tr.find('th.sortable').removeClass('asc').removeClass('desc');
//    if (desc || (!asc && !desc)) {
//        $this.addClass('asc');
//    } else {
//        $this.addClass('desc');
//    }
//});
// Stupid jQuery table plugin.
// https://github.com/joequery/Stupid-Table-Plugin
(function ($) {
    $.fn.tablesortable = function (sortFns) {
        return this.each(function () {
            var $table = $(this);
            sortFns = sortFns || {};
            sortFns = $.extend({}, $.fn.tablesortable.default_sort_fns, sortFns);
            $table.data('sortFns', sortFns);

            $table.on("click", "thead th", function () {
                $(this).tablesort();
            });
        });
    };


    // Expects $("#mytable").tablesortable() to have already been called.
    // Call on a table header.
    $.fn.tablesort = function (force_direction) {
        var $this_th = $(this);
        var th_index = 0; // we'll increment this soon
        var dir = $.fn.tablesortable.dir;
        var $table = $this_th.closest("table");
        var datatype = $this_th.data("sort") || null;

        // No datatype? Nothing to do.
        if (datatype === null) {
            return;
        }

        // Account for colspans
        $this_th.parents("tr").find("th").slice(0, $(this).index()).each(function () {
            var cols = $(this).attr("colspan") || 1;
            th_index += parseInt(cols, 10);
        });

        var sort_dir;
        if (arguments.length == 1) {
            sort_dir = force_direction;
        }
        else {
            sort_dir = force_direction || $this_th.data("sort-default") || dir.ASC;
            if ($this_th.data("sort-dir"))
                sort_dir = $this_th.data("sort-dir") === dir.ASC ? dir.DESC : dir.ASC;
        }

        // Bail if already sorted in this direction
        if ($this_th.data("sort-dir") === sort_dir) {
            return;
        }
        // Go ahead and set sort-dir.  If immediately subsequent calls have same sort-dir they will bail
        $this_th.data("sort-dir", sort_dir);

        $table.trigger("beforetablesort", { column: th_index, direction: sort_dir });

        // More reliable method of forcing a redraw
        $table.css("display");

        // Run sorting asynchronously on a timout to force browser redraw after
        // `beforetablesort` callback. Also avoids locking up the browser too much.
        setTimeout(function () {
            // Gather the elements for this column
            var column = [];
            var sortFns = $table.data('sortFns');
            var sortMethod = sortFns[datatype];
            var trs = $table.children("tbody").children("tr");

            // Extract the data for the column that needs to be sorted and pair it up
            // with the TR itself into a tuple. This way sorting the values will
            // incidentally sort the trs.
            trs.each(function (index, tr) {
                var $e = $(tr).children().eq(th_index);
                var sort_val = $e.data("sort-value");

                // Store and read from the .data cache for display text only sorts
                // instead of looking through the DOM every time
                if (typeof (sort_val) === "undefined") {
                    var txt = $e.text();
                    $e.data('sort-value', txt);
                    sort_val = txt;
                }
                column.push([sort_val, tr]);
            });

            // Sort by the data-order-by value
            column.sort(function (a, b) { return sortMethod(a[0], b[0]); });
            if (sort_dir != dir.ASC)
                column.reverse();

            // Replace the content of tbody with the sorted rows. Strangely
            // enough, .append accomplishes this for us.
            trs = $.map(column, function (kv) { return kv[1]; });
            $table.children("tbody").append(trs);

            // Reset siblings
            $table.find("th").data("sort-dir", null).removeClass("sorting-desc sorting-asc");
            $this_th.data("sort-dir", sort_dir).addClass("sorting-" + sort_dir);

            // ---
                
                var $tr = $this_th.closest('tr');
                $tr.find('th').removeClass('asc').removeClass('desc');
                var arrow = sort_dir === dir.ASC ? "asc" : "desc";
                $this_th.addClass(arrow);
                
            // ---

            $table.trigger("aftertablesort", { column: th_index, direction: sort_dir });
            $table.css("display");
        }, 10);

        return $this_th;
    };

    // Call on a sortable td to update its value in the sort. This should be the
    // only mechanism used to update a cell's sort value. If your display value is
    // different from your sort value, use jQuery's .text() or .html() to update
    // the td contents, Assumes tablesortable has already been called for the table.
    $.fn.updateSortVal = function (new_sort_val) {
        var $this_td = $(this);
        if ($this_td.is('[data-sort-value]')) {
            // For visual consistency with the .data cache
            $this_td.attr('data-sort-value', new_sort_val);
        }
        $this_td.data("sort-value", new_sort_val);
        return $this_td;
    };

    // ------------------------------------------------------------------
    // Default settings
    // ------------------------------------------------------------------
    $.fn.tablesortable.dir = { ASC: "asc", DESC: "desc" };
    $.fn.tablesortable.default_sort_fns = {
        "int": function (a, b) {
            return parseInt(a, 10) - parseInt(b, 10);
        },
        "float": function (a, b) {
            return parseFloat(a) - parseFloat(b);
        },
        "string": function (a, b) {
            return a.toString().localeCompare(b.toString());
        },
        "string-ins": function (a, b) {
            a = a.toString().toLocaleLowerCase();
            b = b.toString().toLocaleLowerCase();
            return a.localeCompare(b);
        },
        "datetime": function (a, b) {
            
            return moment(a, _dateTimeFormat.DATA.FORMAT).isAfter(moment(b, _dateTimeFormat.DATA.FORMAT));
        },
        "currency": function (a, b) {
            return unformatDouble(a) - unformatDouble(b);
        }
    };
})(jQuery);
//---



//---- bsDialog Plugin
$(document).ready(function () {

    $(document).on({
        'show.bs.modal': function () {
            var zIndex = 1040 + (10 * $('.modal:visible').length);
            $(this).css('z-index', zIndex);
            setTimeout(function () {
                $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
            }, 0);
        },
        'hidden.bs.modal': function () {
            if ($('.modal:visible').length > 0) {
                // restore the modal-open class to the body element, so that scrolling works
                // properly after de-stacking a modal.
                setTimeout(function () {
                    $(document.body).addClass('modal-open');
                }, 0);
            }
        }
    }, '.modal');
});
; (function ($, window, document, undefined) {

    "use strict";

    // undefined is used here as the undefined global variable in ECMAScript 3 is
    // mutable (ie. it can be changed by someone else). undefined isn't really being
    // passed in so we can ensure the value of it is truly undefined. In ES5, undefined
    // can no longer be modified.

    // window and document are passed through as local variable rather than global
    // as this (slightly) quickens the resolution process and can be more efficiently
    // minified (especially when both are regularly referenced in your plugin).

    // Create the defaults once
    var pluginName = 'bsDialog',
        defaults = {
            title: '',
            size: '',
            buttons: []
        };

    // The actual plugin constructor
    function Plugin(element, options) {
        this.element = element;

        // jQuery has an extend method which merges the contents of two or
        // more objects, storing the result in the first object. The first object
        // is generally empty as we don't want to alter the default options for
        // future instances of the plugin
        this.settings = $.extend({}, defaults, options);
        this._defaults = defaults;
        this._name = pluginName;
        this.init();
    }

    // Avoid Plugin.prototype conflicts
    $.extend(Plugin.prototype, {
        init: function () {

            // Place initialization logic here
            // You already have access to the DOM element and
            // the options via the instance, e.g. this.element
            // and this.settings
            // you can add more functions like the one below and
            // call them like the example bellow
            //this.yourOtherFunction("jQuery Boilerplate");

            var $this = $(this.element);
            $this.addClass('modal fade').attr('role', 'dialog').removeClass('hidden');


            var $modalDialog = $('<div class="modal-dialog ' + this.settings.size + '" role="document">');
            var $modalContent = $('<div class="modal-content">');
            var $modalHeader = $('<div class="modal-header">');

            $modalHeader.html(
                '<button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>' +
                '<h4 class="modal-title">' + this.settings.title + '</h4>');

            var $modalFooter = $('<div class="modal-footer">');

            $.each(this.settings.buttons, function (i, btn) {
                btn.class = btn.class || 'btn-default';
                var $btn = $('<button type="button" class="btn ' + btn.class + '">' + btn.label + '</button>').click(function () {
                    if ($.isFunction(btn.action)) {
                        btn.action($this);
                    }
                });
                $modalFooter.append($btn);
            });

            $modalContent.append($modalHeader).append($this.find('.modal-body')).append($modalFooter);

            $modalDialog.append($modalContent);

            $this.append($modalDialog);



        }
    });

    // A really lightweight plugin wrapper around the constructor,
    // preventing against multiple instantiations
    $.fn[pluginName] = function (options) {
        return this.each(function () {
            if (!$.data(this, "plugin_" + pluginName)) {
                $.data(this, "plugin_" +
                    pluginName, new Plugin(this, options));
            }
        });
    };

})(jQuery, window, document);
//----

function submitForm(pageUrl, pageParams) {

    var $htmlForm = $('<form>').attr({ 'id': 'temp_form', 'method': 'POST', 'action': pageUrl });
    

    $.each(pageParams, function (i, p) {
        $htmlForm.append(
            $('<input>').attr({ 'id': p.Name, 'name': p.Name, 'type': 'hidden' }).val(p.Value)
        );
    });

    //Submit the form
    $htmlForm.appendTo("body").submit();
}

function setTableFilter(tblId, filterValue) {

	var rows = '#' + tblId + ' tr';
	var rex = new RegExp(filterValue, 'i');
	$(rows).hide();
	$(rows).filter(function () {
		return rex.test($(this).text());
	}).show();
}