
(function ($) {
    /*
    // allownumericwithdecimal      =>  numeri decimali
    // allownumericwithoutdecimal   =>  numeri interi
    // dropdown         =>  datasource: [array di items con value e text]
    // dropdowngrouped  =>  datasource: [array di optGroup con id, label e options che è un array di items con value e text]
    // dropdownnamed    =>  datasource: { options: _ambiente_ao, value: 'AO_ID', text: 'AO_DESC' }
    // dropdowntwoarray =>  datasource: { optgroups: { datasource: _ambiente_uso, value: 'USO_ID', text: 'USO' }, options: _ambiente_tipo, value: 'TIPO_ID', text: 'TIPO', group: 'USO_ID' }
    */

	$.editableTable = function (element, options) {
        var ARROW_LEFT = 37, ARROW_UP = 38, ARROW_RIGHT = 39, ARROW_DOWN = 40, ENTER = 13, ESC = 27, TAB = 9;

        var active, editor, value; // cella attiva, controllo per la modifica, valore corrente

        var defaults = {
            cssClass: 'form-control input-sm',
            fields: [],
            cloneProperties: ['padding', 'padding-top', 'padding-bottom', 'padding-left', 'padding-right',
                'text-align', 'font', 'font-size', 'font-family', 'font-weight',
                'border', 'border-top', 'border-bottom', 'border-left', 'border-right'],
			// if your plugin is event-driven, you may provide callback capabilities
			// for its events. execute these functions before or after events of your 
			// plugin, so that users may customize those particular events without 
			// changing the plugin's code
			onChange: null

		}

		var plugin = this;

		
		plugin.settings = {}

		var $element = $(element), // reference to the jQuery version of DOM element
             element = element;    // reference to the actual DOM element

		// the "constructor" method that gets called when the object is created
		plugin.init = function () {

			// the plugin's final properties are the merged default and 
			// user-provided options (if any)
			plugin.settings = $.extend({}, defaults, options);

			// code goes here
            // console.log('plugin.init');
            $element.on('click dblclick', showEditor)
                .keydown(function (e) {
                    var prevent = true,
                        possibleMove = movement($(e.target), e.which);
                    if (possibleMove.length > 0) {
                        possibleMove.focus();
                    } else if (e.which === ENTER) {
                        showEditor(false);
                    } else if (e.which === 17 || e.which === 91 || e.which === 93) {
                        showEditor(true);
                        prevent = false;
                    } else {
                        prevent = false;
                    }
                    if (prevent) {
                        e.stopPropagation();
                        e.preventDefault();
                    }
                });

            
            $element.on('keypress keyup blur', '.allownumericwithoutdecimal', function (event) {
                $(this).val($(this).val().replace(/[^\d].+/, ""));
                if ((event.which < 48 || event.which > 57)) {
                    event.preventDefault();
                }
            });
            //$element.on('change', 'td', function (evt) {
            //    // Eventuale validazione o formattazione input
            //    var cell = $(this), column = cell.index();

            //    // non valido return false;
            //});

		}

		// private methods
		
        movement = function (element, keycode) {
            if (keycode === ARROW_RIGHT) {
                return element.next('td');
            } else if (keycode === ARROW_LEFT) {
                return element.prev('td');
            } else if (keycode === ARROW_UP) {
                return element.parent().prev().children().eq(element.index());
            } else if (keycode === ARROW_DOWN) {
                return element.parent().next().children().eq(element.index());
            }
            return [];
        };

        showEditor = function (e) {
            
            active = $(e.target);//$element.find('td:focus');
            //console.log('active.length: ' + active.length);
            if (active.length) {
                var originalContent;
                var field = $.grep(plugin.settings.fields, function (f) { return f.id === active.data('fieldId'); })[0];
                if (field) {
                    field.type = field.type || 'text';
                    switch (field.type) {
                        case 'text':
                            editor = $('<input autocomplete="off">').val(active.text());
                            break;
                        case 'allownumericwithdecimal':
                            editor = $('<input autocomplete="off">').on("keypress keyup blur", function (event) {
                                //this.value = this.value.replace(/[^0-9\.]/g,'');
                                $(this).val($(this).val().replace(/[^0-9\.]/g, ''));
                                if ((event.which != 46 || $(this).val().indexOf('.') != -1) && (event.which < 48 || event.which > 57)) {
                                    event.preventDefault();
                                }
                            }).val(active.text());
                            break;
                        case 'allownumericwithoutdecimal':
                            editor = $('<input autocomplete="off">').on("keypress keyup blur", function (event) {
                                $(this).val($(this).val().replace(/[^\d].+/, ""));
                                if ((event.which < 48 || event.which > 57)) {
                                    event.preventDefault();
                                }
                            }).val(active.text());
                            break
                        case 'dropdown':
                            if (field.datasource) {
                                editor = $('<select>');
                                originalContent = active.html();
                                var ds;

                                if ($.isArray(field.datasource)) {
                                    ds = field.datasource;
                                }

                                if ($.isFunction(field.datasource)) {
                                    ds = field.datasource(active);
                                }

                                $.each(ds, function (i, opt) {

                                    editor.append(
                                        $('<option>')
                                            .val(opt.value)
                                            .text(opt.text)
                                            .prop('selected', originalContent == opt.text)
                                    );

                                });
                            }
                            break;
                        case 'dropdownnamed':
                            if (field.datasource) {
                                editor = $('<select>');
                                originalContent = active.html();

                                if ($.isArray(field.datasource.options)) {
                                    $.each(field.datasource.options, function (i, opt) {
                                        
                                        editor.append(
                                            $('<option>')
                                                .val(opt[field.datasource.value])
                                                .text(opt[field.datasource.text])
                                                .prop('selected', originalContent == opt[field.datasource.text])
                                        );
                                        
                                    });

                                    
                                }
                            }
                            break;
                        case 'dropdowngrouped':
                            if (field.datasource) {
                                editor = $('<select>');
                                originalContent = active.html();

                                if ($.isArray(field.datasource)) {
                                    $.each(field.datasource, function (i, optGroup) {
                                        var $optGroup = $('<optgroup>').attr({ 'id': optGroup.id, 'label': optGroup.label });

                                        if ($.isArray(optGroup.options)) {

                                            $.each(optGroup.options, function (i, opt) {

                                                $optGroup.append(
                                                    $('<option>')
                                                        .val(opt.value)
                                                        .text(opt.text)
                                                        .prop('selected', originalContent == opt.text)
                                                );

                                            });


                                        }

                                        editor.append($optGroup);

                                    });


                                }
                            }
                            break;
                        case 'dropdowntwoarray':
                            if (field.datasource) {
                                editor = $('<select>');
                                originalContent = active.html();

                                if ($.isArray(field.datasource.optgroups.datasource)) {
                                    $.each(field.datasource.optgroups.datasource, function (i, optGroup) {
                                        var $optGroup = $('<optgroup>').attr({ 'id': optGroup[field.datasource.optgroups.value], 'label': optGroup[field.datasource.optgroups.text] });

                                        if ($.isArray(field.datasource.options)) {

                                            var options = $.grep(field.datasource.options, function (opt) { return opt[field.datasource.group] === optGroup[field.datasource.optgroups.value]; })

                                            $.each(options, function (i, opt) {

                                                $optGroup.append(
                                                    $('<option>')
                                                        .val(opt[field.datasource.value])
                                                        .text(opt[field.datasource.text])
                                                        .prop('selected', originalContent == opt[field.datasource.text])
                                                )

                                            });


                                        }

                                        editor.append($optGroup);

                                    });


                                }
                            }
                            break;
                            
                            
                    }

                    if (editor) {
                        editor.addClass(plugin.settings.cssClass).css({ 'position': 'absolute' }).hide().appendTo($element.parent());

                        editor
                            .removeClass('error')
                            .show()
                            .offset(active.offset())
                            .css(active.css(plugin.settings.cloneProperties))
                            .width(active.width())
                            .height(active.height())
                            .focus()
                            .blur(function () {
                                setActiveText(field);
                                editor.remove();
                                editor = null;
                            });
                    }
                    
                }
                
            }
        }

        function setActiveText(field) {
            var evt = $.Event('change'), originalContent = active.html(), newContent;
            var valid = true;
            switch (field.type) {

                case 'dropdown':
                case 'dropdownnamed':
                    newContent = $('option:selected', editor);
                    active.html(newContent.text());
                    break;
                case 'dropdowngrouped':
                case 'dropdowntwoarray':
                    var opt = $('option:selected', editor);
                    var optGroup = opt.closest('optgroup');
                    newContent = { optionGroup: optGroup, option: opt };
                    active.html(opt.text());

                    break;
                default:
                    newContent = editor.val();

                    active.html(newContent);
                    break;

            }

           //   Convalida nell'evento onChange
            
            if ($.isFunction(plugin.settings.onChange)) {
                if (plugin.settings.onChange(active, field, newContent) === false) {
                    active.html(originalContent);
                };
            }
            
            
            
        }
        // public methods
        // these methods can be called like:
        // plugin.methodName(arg1, arg2, ... argn) from inside the plugin or
        // element.data('pluginName').publicMethod(arg1, arg2, ... argn) from outside 
        // the plugin, where "element" is the element the plugin is attached to;

        // a public method. for demonstration purposes only - remove it!
        plugin.public_method = function () {

            // code goes here

        }

		// fire up the plugin!
		// call the "constructor" method
		plugin.init();

	}

	// add the plugin to the jQuery.fn object
	$.fn.editableTable = function (options) {

		// iterate through the DOM elements we are attaching the plugin to
		return this.each(function () {

			// if plugin has not already been attached to the element
			if (undefined == $(this).data('editableTable')) {

				// create a new instance of the plugin
				// pass the DOM element and the user-provided options as arguments
				var plugin = new $.editableTable(this, options);

				// in the jQuery version of the element
				// store a reference to the plugin object
				// you can later access the plugin and its methods and properties like
				// element.data('pluginName').publicMethod(arg1, arg2, ... argn) or
				// element.data('pluginName').settings.propertyName
				$(this).data('editableTable', plugin);

			}

		});

	}

})(jQuery);