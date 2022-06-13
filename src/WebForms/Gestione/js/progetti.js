$(document).ready(function () {
    $(document).on('click', 'button.btn-save', function () {
        var c = ['PRJDESC', 'PRJGUID', 'PRJDATABASE', 'OUTSIDE', 'INSIDE', 'INSIDE_PLAN', 'ID_1LIV'];
        var o = {};


        $(this).closest('div.form').find('input[type=text]').each(function () {
            var input = $(this);
            for (var i = 0; i < c.length; i++) {
                if (input.hasClass(c[i])) {
                    o[c[i]] = input.val();
                    break;
                }
            }
        });
        
		bsConfirm('Necessaria conferma', 'Salvare le modifiche?', function () {

			callWebMethod(page_Url + '/UpdatePRJ', {
				prjObject: o

			}, function (response) {


				if (response.d !== '') {
					alertDanger(response.d);
				}

			});
		});
	});

	$(document).on('click', 'button.btn-outside', function () {
	
		submitForm($(this).attr('data-viewer'), [
			{ Name: 'codice', Value: $(this).attr('data-codice') },
			{ Name: 'servizio', Value: $(this).attr('data-servizio') },
			{ Name: 'descrizione', Value: $(this).attr('data-descrizione') }
		], 'post');


	});
});