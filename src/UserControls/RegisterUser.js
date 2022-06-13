function RegisterUser(ucId) {
    //alert(ucId);
    var $cognome = $('#' + ucId + '_COGNOME');
    var $nome = $('#' + ucId + '_NOME');
    var $cf = $('#' + ucId + '_CODICE_FISCALE');
    var $userName = $('#' + ucId + '_USERNAME');
    var $email = $('#' + ucId + '_EMAIL');
    
    var $dlg = $('#' + ucId + '_RegisterUserDialog');


    $cf.blur(function () {


        callWebMethod(ws_Util + '/CodFis_Inverso', {
            codice_fiscale: $cf.val()
        }, function (response) {

            userBirthday_js.set({
                NAZIONE: 'IT',
                DATA: response.d.DATA,
                COMUNE: response.d.COMUNE,
                SIGLA_PROVINCIA: response.d.SIGLA_PROVINCIA,
                PROVINCIA: response.d.PROVINCIA
            });
            
        });
    });

    $dlg.bsDialog(
        {
            title: "REGISTRAZIONE UTENTE",
            size: 'modal-lg',
            buttons: [
                {
                    label: 'Ok',
                    class: 'btn-primary',
                    action: function (dlgItSelf) {

                        var is_valid = checkRequired($dlg) && checkValidInput($dlg);
                        if (!is_valid) return;
                        var sogAnagrafica = {};
                        sogAnagrafica.SOGGETTO_ID = -1;
                        sogAnagrafica.COGNOME = $cognome.val();
                        sogAnagrafica.NOME = $nome.val();
                        sogAnagrafica.CF = $cf.val();
                        sogAnagrafica.EMAIL = $email.val();
                        var sogRecapito = userAddress_js.get();
                        var sogNascita = userBirthday_js.get();
                        callWebMethod(page_Url + '/RegisterUser', {
                            userName: $userName.val(),
                            tipoUtente: $dlg.data('tipoUtente'),
                            sogAnagrafica: sogAnagrafica,
                            sogRecapito: sogRecapito,
                            sogNascita: sogNascita
                        }, function (response) {
                            dlgItSelf.modal('hide');
                            if (response.d == '') {
                                BootstrapDialog.show({
                                    message: '<div class="alert alert-success alert-white rounded" role="alert">' +
                                    '   <div class="icon" style="float: left;"><i class="fa fa-check"></i></div>' +
                                    '   <p>Grazie per aver effettuato la registrazione.</p>' +
                                    '   <p>Per ragioni di sicurezza viene richiesta la convalida dell\'indirizzo email specificato.</p>' +
                                    '   <p>Entro i prossimi 10 minuti (solitamente è istantaneo) riceverà una email con le istruzioni per attivare la Sua utenza.</p>' +
                                    '</div >'
                                });
                               

                            }
                            else {
                                alertDanger(response.d);
                                


                            }
                        });
                    }
                },
                {
                    label: 'Chiudi',
                    action: function (dlgItSelf) {
                        dlgItSelf.modal('hide');
                    }
                }
            ]
        });

    this.showDialog = function (tipoUtente) {
        $dlg.data({ 'tipoUtente': tipoUtente }).modal('show');
    }
    //this.get = function () {
    //    return {
    //        NAZIONE: $nazione.val(),
    //        SIGLA_PROVINCIA: $sigla.val(),
    //        PROVINCIA: $provincia.val(),
    //        COMUNE: $comune.val(),
    //        CAP: $cap.val(),
    //        INDIRIZZO: $indirizzo.val(),
    //        CIVICO: $civico.val() != '' ? $civico.val() : 'S.N.C.',
    //        DATA: $data.val()
    //    };

    //};

    //this.set = function (address) {

    //    $nazione.val(address.NAZIONE);
    //    $sigla.val(address.SIGLA_PROVINCIA);
    //    $provincia.val(address.PROVINCIA);
    //    $comune.val(address.COMUNE);
    //    $cap.val(address.CAP);
    //    $indirizzo.val(address.INDIRIZZO);
    //    $civico.val(address.CIVICO);
    //    $data.val(address.DATA);

    //    elencaProvince();
    //    elencaComuni(address.SIGLA_PROVINCIA);
    //};

    //this.getCountry = function () {
    //    return _nazioni.DataTable[$nazione.prop('selectedIndex')];

    //};

    //this.setCaption = function (caption) {
    //    $titolo.html(caption);
    //};
    //this.getCaption = function () {
    //    return $titolo.html();
    //};
    //this.setDataTitle = function (dataTitle) {
    //    $titoloData.html(dataTitle);
    //};


    //this.setOnCountryChange = function (callback) {
    //    _onCountryChangeCallback = callback;
    //};


}