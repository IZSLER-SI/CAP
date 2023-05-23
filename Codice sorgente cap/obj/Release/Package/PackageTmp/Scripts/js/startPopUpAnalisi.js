$.template.init();
/*
$('#sorting-AnalisiPopUp').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});
*/
var mydialogAnalisi ;
function openRicercaAnalisi(urlDialog) {
    var timeout;
    mydialogAnalisi = $.modal({
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

    mydialogAnalisi.loadModalContent(urlDialog, null);
};

function clearDataPopUpAnalisi() {
    $('#ric_valid_desc').val("");
    $('#ric_valid_id').val("");
}
function saveDataPopUpAnalisi(id,valore) {
    $('#ric_valid_desc').val(valore);
    $('#ric_valid_id').val(id);
    mydialogAnalisi.closeModal();
}




