function AddressSelector(ucId) {

    var $titolo = $('#' + ucId + '_Titolo');
    var $nazione = $('#' + ucId + '_Nazione');
    var $provincia = $('#' + ucId + '_Provincia');
    var $sigla = $('#' + ucId + '_Sigla');
    var $comune = $('#' + ucId + '_Comune');
    var $cap = $('#' + ucId + '_Cap');
    var $indirizzo = $('#' + ucId + '_Indirizzo');
    var $civico = $('#' + ucId + '_Civico');
    var $data = $('#' + ucId + '_Data');
    var $titoloData = $('#' + ucId + '_TitoloData');
    //var _nazioni = null;
    var _onCountryChangeCallback = null;

    //$nazione.empty();
    //callWebMethod(ws_Util + '/ElencaNazioni', {
        
    //}, function (response) {
    //    var jsonResult = $.parseJSON(response.d);
    //    if (jsonResult.ErrorMessage == '') {
    //        _nazioni = jsonResult.DataTable;
    //        $.each(_nazioni, function (i, n) {
    //            $nazione.append($('<option value="' + n.CODICE_NAZIONE + '">' + n.CODICE_NAZIONE + ' - ' + n.DESCRIZIONE + '</option>'));
    //        });

    //        elencaProvince();
    //    }
    //    else {
    //        alertDanger(jsonResult.ErrorMessage);
    //    }
    //});
    elencaProvince();
    $nazione.change(function () {
        $provincia.val(stringEmpty);
        $sigla.val(stringEmpty);
        $comune.val(stringEmpty);
        $cap.val(stringEmpty);
        $indirizzo.val(stringEmpty);
        $civico.val(stringEmpty);

        if ($.isFunction(_onCountryChangeCallback)) {
            _onCountryChangeCallback(_nazioni.DataTable[$nazione.prop('selectedIndex')]);
        }

        elencaProvince();
    });

    function elencaProvince() {
        $provincia.typeahead('destroy');
        callWebMethod(ws_Util + '/ElencaProvince', {
            cod_nazione: $nazione.val()
        }, function (response) {
            var jsonResult = $.parseJSON(response.d);
            if (jsonResult.ErrorMessage == '') {
                $provincia.typeahead({
                        source: jsonResult.DataTable,
                        autoSelect: false,
                        afterSelect: function (item) {
                            $sigla.val(item.id);
                            $comune.val('');
                            $cap.val('');
                            elencaComuni(item.id);
                        }
                });
            }
            else {
                alertDanger(jsonResult.ErrorMessage);
            }
        });
    }

    function elencaComuni(sigla_provincia) {
        $comune.typeahead('destroy');
        callWebMethod(ws_Util + '/ElencaComuni', {
            sigla_provincia: sigla_provincia
        }, function (response) {
            var jsonResult = $.parseJSON(response.d);
            if (jsonResult.ErrorMessage == '') {
                $comune.typeahead({
                    source: jsonResult.DataTable,
                    autoSelect: false,
                    afterSelect: function (item) {
                        elencaCap(sigla_provincia, item.name);
                    }
                });
            }
            else {
                alertDanger(jsonResult.ErrorMessage);
            }
        });
    }
    
    function elencaCap(sigla_provincia, comune) {
        $cap.typeahead('destroy');
        callWebMethod(ws_Util + '/ElencaCap', {
            sigla_provincia: sigla_provincia, comune: comune
        }, function (response) {
            var jsonResult = $.parseJSON(response.d);
            if (jsonResult.ErrorMessage == '') {
                $cap.typeahead({
                    source: jsonResult.DataTable,
                    autoSelect: true
                });

                if (jsonResult.DataTable.length > 0) {
                    $cap.val(jsonResult.DataTable[0].name);
                }
            }
            else {
                alertDanger(jsonResult.ErrorMessage);
            }
        });
    }

    


    this.get = function () {
        return {
            NAZIONE: $nazione.val(),
            SIGLA_PROVINCIA: $sigla.val(),
            PROVINCIA: $provincia.val(),
            COMUNE: $comune.val(),
            CAP: $cap.val(),
            INDIRIZZO: $indirizzo.val(),
            CIVICO: $civico.val() != '' ? $civico.val() : 'S.N.C.',
            DATA: $data.val()
        };
        
    };

    this.set = function (address) {

        $nazione.val(address.NAZIONE);
        $sigla.val(address.SIGLA_PROVINCIA);
        $provincia.val(address.PROVINCIA);
        $comune.val(address.COMUNE);
        $cap.val(address.CAP);
        $indirizzo.val(address.INDIRIZZO);
        $civico.val(address.CIVICO);
        $data.val(address.DATA);

        elencaProvince();
        elencaComuni(address.SIGLA_PROVINCIA);
    };

    this.getCountry = function () {
        return _nazioni.DataTable[$nazione.prop('selectedIndex')];

    };

    this.setCaption = function (caption) {
        $titolo.html(caption);
    };
    this.getCaption = function () {
        return $titolo.html();
    };
    this.setDataTitle = function (dataTitle) {
        $titoloData.html(dataTitle);
    };


    this.setOnCountryChange = function (callback) {
        _onCountryChangeCallback = callback;
    };


}