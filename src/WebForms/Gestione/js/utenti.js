$(document).ready(function () {
    $(document).on('click', '.btn-delete', function () {
        var userId = $(this).data('id');
        bsConfirm('Necessaria conferma', 'Sei sicuro di voler eliminare questo utente?', function () {
            
            callWebMethod(page_Url + '/DeleteUser', {
                UserId: userId

            }, function (response) {

                
                if (response.d != '') {
                    alertDanger(response.d);
                }
                else {
                    loadUsers();


                }
            });
        })
    });

    $(document).on('click', '.btn-attach', function () {
        var userId = $(this).data('id');
        callWebMethod(page_Url + '/LoadPrjForUser', {
            UserId: userId

        }, function (response) {


            var jsonResult = $.parseJSON(response.d);
            if (jsonResult.ErrorMessage != '') {
                alertDanger(jsonResult.ErrorMessage);
            }
            else {
                BootstrapDialog.show({
                    title: 'ASSEGNAZIONE PROGETTI',
                    message: function () {

                        var $msg = $('<div>');
                        $.each(jsonResult.DataTable, function (i, row) {
							$msg.append('<p><input type="checkbox" class="prj" data-codice="' + row.CODICE + '" data-userid="' + userId + '" ' + (row.UserId != null ? 'checked' : '') + '> ' + row.DESCRIZIONE + '</p>');
                        });
                        return $msg;
                    },
                    onshown: function (dlg) {
                           

                        $(dlg.getModalDialog()).on('change', 'input.prj', function () {
                            var prj = $(this).data('codice');
                            var usr = $(this).data('userid');
                            var chk = $(this).prop('checked');

                            callWebMethod(page_Url + '/AssignPrjToUser', {
                                CODICE: prj,
                                UserId: usr,
                                Assign: chk

                            }, function (response) {


                                if (response.d != '') {
                                    alertDanger(response.d);
                                }
                                
                            });

                        });
                    },
                    draggable: true,
                    buttons: [
                        {
                            label: 'Chiudi',
                            action: function (dialog) {
                                dialog.close();
                            }
                        }]
                });
            }
        });
    });

    $('#btnCreateFakeUser').click(function () {
        BootstrapDialog.show({
            title: 'CREAZIONE UTENTE FAKE',
            size: BootstrapDialog.SIZE_SMALL,
            message: function () {
                return $('<div class="row">' +
                    '<div class="form-group form-group-sm col-sm-12">' +
                    '   <label>USER NAME</label><input type="text" id="FakeUserName" class="form-control"/>' +
                    '</div>' +
                    '</div>' +
                    '<div class="row">' +
                    '<div class="form-group form-group-sm col-sm-12">' +
                    '   <label>EMAIL</label><input type="text" id="FakeUserEmail"  class="form-control"/>' +
                    '</div>' +
                    '</div >' +
                    '<div class="row">' +
                    '<div class="form-group form-group-sm col-sm-12">' +
                    '   <label>PASSWORD</label><input type="text" id="FakeUserPassword"  class="form-control"/>' +
                    '</div>' +
                    '</div >');
            },
            buttons: [
                {
                    label: 'Crea',
                    cssClass: 'btn-primary',
                    action: function (dlgItSelf) {
                        dlgItSelf.close();

                        callWebMethod(page_Url + '/RegisterFakeUser', {
                            userName: $('#FakeUserName').val(),
                            userPwd: $('#FakeUserPassword').val(),
                            userEmail: $('#FakeUserEmail').val()
                        }, function (response) {
                            
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
                    action: function (dlg) {
                        dlg.close();
                    }
                }
            ]
        });

    });

    function loadUsers() {
        callWebMethod(page_Url + '/LoadUsers', {
            

        }, function (response) {

            var jsonResult = $.parseJSON(response.d);
            if (jsonResult.ErrorMessage != '') {
                alertDanger(jsonResult.ErrorMessage);
            }
            else {
                $('#tbl_Users').empty().append($('#tmpl_User').render(jsonResult.DataTable));

              
            }





        });
    }

    loadUsers();
});