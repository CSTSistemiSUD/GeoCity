(function ($) {

    $.validInputChecker = function (element, options) {

        var defaults = {
            country: {
                "CODICE_NAZIONE": "IT",
                "DESCRIZIONE": "ITALIA",
                "CHECK_PIVA": true,
                "CHECK_CF": true,
                "CCVATCode": "IT"
            },
            isValid: false
        };

        var plugin = this;

        plugin.settings = {};

        var $element = $(element), $container = $element.parent();

        var elementType = $element.data('type');
        var elementRequired = $element.prop('required');
        var format = $element.data('format');

        if (!_dateTimeFormat[format]) {
            format = 'DATA';
        }

        plugin.init = function () {
            plugin.settings = $.extend({}, defaults, options);
            // code goes here
            $element.addClass('validInputChecker'); // classe usata per trovare tutte le istanze

            switch (elementType) {
                case 'email':
                    $container.append($('<span class="help-block" style="font-size: 8pt;">Indirizzo email non valido</span >').hide());
                    break;
                case 'codice-fiscale':
                    $container.append($('<span class="help-block" style="font-size: 8pt;">Codice Fiscale non valido</span >').hide());
                    break;
                case 'partita-iva':
                    $div = $('<div class="input-group">');
                    $div
                        .append('<span class="input-group-addon" style="width:50px;">' + plugin.settings.country.CODICE_NAZIONE + '</span>')
                        .append($element);


                    $container.append($div).append($('<span class="help-block" style="font-size: 8pt;">Partita IVA non valida.</span >').hide());
                    break;
                case 'data-ora':
                    $container.append($('<span class="help-block" style="font-size: 8pt;">Data non valida</span >').hide());

                    $element.mask(_dateTimeFormat[format].MASK, {
                        placeholder: _dateTimeFormat[format].FORMAT,
                        completed: function () {
                            plugin.validate();
                        }
                    });
                    break;
            }





            $element.blur(function () {
                if ($element.val() != '') {
                    plugin.validate();
                }
            });

        };

        plugin.setCountry = function (country) {
            plugin.settings.country = country;

            if (elementType == 'partita-iva') {
                $element.prev('span').html(plugin.settings.country.CODICE_NAZIONE);
            }

            plugin.validate();

        };

        plugin.validate = function () {

            switch (elementType) {
                case 'email':
                    if (/^\w+([\.-]?\w+)*@\w+([\.-]?\w+)*(\.\w{2,3})+$/.test($element.val())) {
                        plugin.settings.isValid = true;
                    }
                    else {
                        plugin.settings.isValid = false;
                    }
                    break;
                case 'codice-fiscale':
                    if (plugin.settings.country.CHECK_CF) {
                        if (/^(?:[B-DF-HJ-NP-TV-Z](?:[AEIOU]{2}|[AEIOU]X)|[AEIOU]{2}X|[B-DF-HJ-NP-TV-Z]{2}[A-Z]){2}[\dLMNP-V]{2}(?:[A-EHLMPR-T](?:[04LQ][1-9MNP-V]|[1256LMRS][\dLMNP-V])|[DHPS][37PT][0L]|[ACELMRT][37PT][01LM])(?:[A-MZ][1-9MNP-V][\dLMNP-V]{2}|[A-M][0L](?:[\dLMNP-V][1-9MNP-V]|[1-9MNP-V][0L]))[A-Z]$/i.test($element.val())) {
                            plugin.settings.isValid = true;
                            //callWebMethod(ws_Util + '/CodFis_Inverso', {
                            //    codice_fiscale: $element.val()
                            //}, function (response) {
                                
                            //    $element.data({
                            //        sogNascita: {
                            //            NAZIONE: 'IT',
                            //            DATA: response.d.DATA,
                            //            COMUNE: response.d.COMUNE,
                            //            SIGLA_PROVINCIA: response.d.SIGLA_PROVINCIA,
                            //            PROVINCIA: response.d.PROVINCIA
                            //        }
                            //    });
                            //});
                        }
                        else {
                            plugin.settings.isValid = false;
                        }
                    }
                    else {
                        plugin.settings.isValid = $element.val() != '';
                    }

                    break;
                case 'partita-iva':
                    if (plugin.settings.country.CHECK_PIVA) {
                        plugin.settings.isValid = checkVATNumber(plugin.settings.country.CODICE_NAZIONE + $element.val());
                    }
                    else {
                        plugin.settings.isValid = $element.val() != '';
                    }
                    break;
                case 'data-ora':
                    plugin.settings.isValid = moment($element.val(), _dateTimeFormat[format].FORMAT, 'it', true).isValid();
                    if (!plugin.settings.isValid && elementRequired) {
                        $element.val(moment().format(_dateTimeFormat[format].FORMAT));
                    }

                    break;
            }

            setValidState();
            if (elementRequired) {
                return plugin.settings.isValid;
            }
            else {
                return true;
            }
            

        };


        var setValidState = function () {
            if (plugin.settings.isValid) {
                $container.find('span.help-block').hide();
            }
            else {
                $container.find('span.help-block').show();
            }
        };
        plugin.init();

    };

    $.fn.validInputChecker = function (options) {

        return this.each(function () {
            if (undefined == $(this).data('validInputChecker')) {
                var plugin = new $.validInputChecker(this, options);
                $(this).data('validInputChecker', plugin);
            }
        });

    };

})(jQuery);


/*
    La funzione di validazione email definitiva

 * Girando per internet sono riuscito a trovarla! Craig Cockburn modificando la funzione di Sandeep V. Tamhankar!,
 * ha creato una funzione che segue alla lettera le specifiche riguardanti le email, così da non dover aver più nessun problema in futuro!
 * (almeno finché non cambiano le specifiche 😀 )

 * Original author:  Sandeep V. Tamhankar (stamhankar@hotmail.com)
 * old Source on http://www.jsmadeeasy.com/javascripts/Forms/Email%20Address%20Validation/template.htm
 * The above address bounces and no current valid address
 * can be found. This version has changes by Craig Cockburn
 * to accommodate top level domains .museum and .name
 * plus various other minor corrections and changes
 *
 * Italian translation by Giulio Chalda Bettega
*  https://blog.chalda.it/?p=11
 */

function emailCheck(emailStr) {
    var emailPat = /^(.+)@(.+)$/;
    var specialChars = "\\(\\)<>@,;:\\\\\\\"\\.\\[\\]";
    var validChars = "[^\\s" + specialChars + "]";
    var quotedUser = "(\"[^\"]*\")";
    var ipDomainPat = /^\[(\d{1,3})\.(\d{1,3})\.(\d{1,3})\.(\d{1,3})\]$/;
    var atom = validChars + "+";
    var word = "(" + atom + "|" + quotedUser + ")";
    var userPat = new RegExp("^" + word + "(\\." + word + ")*$");
    var domainPat = new RegExp("^" + atom + "(\\." + atom + ")*$");
    var matchArray = emailStr.match(emailPat);
    if (matchArray == null) {
        alert("L'email sembra essere sbagliata: (controlla @ e .)");
        return false;
    }
    var user = matchArray[1];
    var domain = matchArray[2];
    if (user.match(userPat) == null) {
        alert("La parte dell'email prima di '@' non sembra essere valida!");
        return false;
    }
    var IPArray = domain.match(ipDomainPat);
    if (IPArray != null) {
        for (var i = 1; i <= 4; i++) {
            if (IPArray[i] > 255) {
                alert("L'IP di destinazione non è valido!");
                return false;
            }
        }
        return true;
    }
    var domainArray = domain.match(domainPat);
    if (domainArray == null) {
        alert("La parte dell'email dopo '@' non sembra essere valida!");
        return false;
    }
    var atomPat = new RegExp(atom, "g");
    var domArr = domain.match(atomPat);
    var len = domArr.length;
    if (domArr[domArr.length - 1].length < 2 ||
        domArr[domArr.length - 1].length > 6) {
        alert("Il dominio di primo livello (es: .com e .it) non sembra essere valido!");
        return false;
    }
    if (len < 2) {
        var errStr = "L'indirizzo manca del dominio!";
        alert(errStr);
        return false;
    }
    return true;
}
//  End -->

function IBANChk(b) {
    if (b.length < 5) { alert("La lunghezza è minore di 5 caratteri"); return false; }
    s = b.substring(4) + b.substring(0, 4);
    for (i = 0, r = 0; i < s.length; i++) {
        c = s.charCodeAt(i);
        if (48 <= c && c <= 57) {
            if (i == s.length - 4 || i == s.length - 3) { alert("Posizioni 1 e 2 non possono contenere cifre"); return false; }
            k = c - 48;
        }
        else if (65 <= c && c <= 90) {
            if (i == s.length - 2 || i == s.length - 1) { alert("Posizioni 3 e 4 non possono contenere lettere"); return false; }
            k = c - 55;
        }
        else { alert("Sono ammesse solo cifre e lettere maiuscole"); return false; }
        if (k > 9)
            r = (100 * r + k) % 97;
        else
            r = (10 * r + k) % 97;
    }
    if (r != 1) { alert("Il codice di controllo è errato"); return false; }
    alert("L'IBAN risulta corretto");
    return true;
} 
