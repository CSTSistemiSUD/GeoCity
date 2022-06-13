var dateRangePicker_locale = {
    "format": "DD/MM/YYYY HH:mm",
    "separator": " - ",
    "applyLabel": "Applica",
    "cancelLabel": "Annulla",
    "fromLabel": "Dal",
    "toLabel": "Al",
    "customRangeLabel": "Personalizza",
    "weekLabel": "W",
    "daysOfWeek": [
        "Do",
        "Lu",
        "Ma",
        "Me",
        "Gi",
        "Ve",
        "Sa"
    ],
    "monthNames": [
        "Gennaio",
        "Febbraio",
        "Marzo",
        "Aprile",
        "Maggio",
        "Giugno",
        "Luglio",
        "Agosto",
        "Settembre",
        "Ottobre",
        "Novembre",
        "Dicembre"
    ],
    "firstDay": 1
};

function dateRangePicker_ranges() {
    return {
        'Oggi': [moment().startOf('day'), moment()],
        'Ieri': [moment().subtract(1, 'days').startOf('day'), moment().subtract(1, 'days').endOf("day")],
        'Ultimi 7 giorni': [moment().subtract(6, 'days'), moment()],
        'Ultimi 30 giorni': [moment().subtract(29, 'days'), moment()],
        'Questo Mese': [moment().startOf('month'), moment().endOf('month')],
        'Mese Scorso': [moment().subtract(1, 'month').startOf('month'), moment().subtract(1, 'month').endOf('month')]
    };
}