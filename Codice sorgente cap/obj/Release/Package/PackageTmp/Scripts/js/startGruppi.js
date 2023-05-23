$.template.init();

// Table sort - DataTables
$('#sorting-gruppi').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});



//var mydialog;
//function openRicercaUtenti(urlDialog) {
//    var timeout;
// mydialog= $.modal({
//        contentAlign: 'center',
//        width: 500,
//        height: 250,
//        title: 'Scelta utenti',
//        content: "",
//        buttons: {},
//        scrolling: true,
//        actions: {
//            'Chiudi': {
//                color: 'red',
//                click: function (win) { win.closeModal(); }
//            }
//        },
//        onClose: function () {
//            // Stop simulated loading if needed
//            clearTimeout(timeout);
//        }
//    });

//    mydialog.loadModalContent(urlDialog, null);
//};

//function clearData() {
//    $('#ric_utente_desc').val("");
//    $('#ric_utente_id').val("");
//}
//function saveData(id,valore) {

//    $('#ric_utente_desc').val(valore);
//    $('#ric_utente_id').val(id);
//    mydialog.closeModal();
//}
