$(document).ready(function () {
    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        if ($("#CheckRedirect").prop('checked')) {
            setTimeout(function () { window.location.replace($('#UrlRedirect').val()); }, 2000);
        }
    });
    

    var language = [];
    language["wordMinLength"] = "La tua password è troppo corta";
    language["wordMaxLength"] = "La tua password è troppo lunga";
    language["wordInvalidChar"] = "La tua password contiene un carattere non valido";
    language["wordNotEmail"] = "Non usare la tua e-mail come password";
    language["wordSimilarToUsername"] = "La tua password non può contenere il tuo nome";
    language["wordTwoCharacterClasses"] = "Usa classi di caratteri diversi";
    language["wordRepetitions"] = "Troppe ripetizioni";
    language["wordSequences"] = "La tua password contiene sequenze";
    language["errorList"] = "Errori:";
    language["veryWeak"] = "Molto debole";
    language["weak"] = "Debole";
    language["normal"] = "Normale";
    language["medium"] = "Media";
    language["strong"] = "Forte";
    language["veryStrong"] = "Molto forte";
    

    var options = {};
    options.ui = {
        container: "#pwd-container",
        showVerdictsInsideProgressBar: true,
        viewports: {
            progress: "#pwstrength_viewport_progress"
        }
    };
    options.i18n = {
        t: function (key) {
            var result = language[key]; // Do your magic here

            return result === key ? '' : result; // This assumes you return the
            // key if no translation was found, adapt as necessary
        }
    };

    $('#NewPassword').pwstrength(options);

    $('#UpdatePassword').click(function () {
        var isValid = checkRequired('#pwd-container') && checkValidInput('#pwd-container');

        var hasEqual = $('#NewPassword').val() == $('#ConfirmPassword').val();
        if (!hasEqual) {
            $('#NewPassword').closest('.form-group').addClass('has-error');
            $('#ConfirmPassword').closest('.form-group').addClass('has-error');
            alertWarning("Le Password non corrispondono!");
        }
        

        return (isValid && hasEqual);
    })
});