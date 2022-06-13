$(document).ready(function () {
    $('#txtAnagraficaCodFis').blur(function () {


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
});

