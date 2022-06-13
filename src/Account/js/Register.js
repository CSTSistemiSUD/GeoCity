$(document).ready(function () {

	var confirmButton = $('[data-id="ConfirmButton"]');
	//var password = $('[data-id="PasswordCtrl"]');
	//var passwordConfirm = $('[data-id="ConfirmPasswordCtrl"]');

	var email = $('[data-id="EMailCtrl"]');
	var emailConfirm = $('[data-id="ConfirmEMailCtrl"]');

	$(document).on('focus', '.has-error', function () {
		$(this).closest('.form-group').removeClass('has-error');
	});

	$(document).on('click', '.toggle-input-type', function () {
		var inputId = $(this).data('ref');
		var input = $('[data-id="' + inputId + '"]');

		if (input.attr('type') == 'password') {
			input.attr('type', 'text');
			$(this).html('<span class="glyphicon glyphicon-eye-close"></span>');
		}
		else {
			input.attr('type', 'password');
			$(this).html('<span class="glyphicon glyphicon-eye-open"></span>');
		}

	});

	confirmButton.click(function () {
		var context = $('[data-id="RegistrationForm"]');
		var is_valid = true;
		$('[data-required="true"]', context).each(function () {
			$(this).closest('.form-group').removeClass('has-error');
			if ($(this).val() == '') {
				$(this).closest('.form-group').addClass('has-error');
				is_valid = false;
			}
		});

		//if (is_valid && password.val() != passwordConfirm.val()) {
		//	$(password).closest('.form-group').addClass('has-error');
		//	$(passwordConfirm).closest('.form-group').addClass('has-error');
		//	is_valid = false;
		//}

		if (is_valid && email.val() != emailConfirm.val()) {
			$(email).closest('.form-group').addClass('has-error');
			$(emailConfirm).closest('.form-group').addClass('has-error');
			is_valid = false;
		}

		return is_valid;

	});
});