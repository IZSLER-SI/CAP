$.template.init();

$('#sorting-ProdottoPopUp').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});

var mydialogProdotto;
function openRicercaProdotto(urlDialog) {
    var timeout;
    mydialogProdotto = $.modal({
        contentAlign: 'center',
        width: 900,
        height: 250,
        title: 'Scelta chiave',
        content: "",
        buttons: {},
        scrolling: true,
        actions: {
            'Chiudi': {
                color: 'red',
                click: function (win) { win.closeModal(); }
            }
        },
        onClose: function () {
            // Stop simulated loading if needed
            clearTimeout(timeout);
        }
    });

    mydialogProdotto.loadModalContent(urlDialog, null);
};

function clearDataPopUpProdotto() {
    $('#ric_prodot_desc').val("");
    $('#ric_prodot_id').val("");
}
function saveDataPopUpProdotto(id, valore) {
    $('#ric_prodot_desc').val(valore);
    $('#ric_prodot_id').val(id);
    mydialogProdotto.closeModal();
}

