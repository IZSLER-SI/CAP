$.template.init();

//$('#sorting-advanced').tablesorter();
// Table sort - DataTables
$('#sorting-trackingAnalisi').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [
        { 'bSortable': false, 'aTargets': [0] },
        { 'sType': 'date', 'aTargets': [0] }
        ],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});
//$('#sorting-Utenti').tablesorter();


var mydialogTrkAnalisi;
function openAnalisiWorkFlow(urlDialog,analisi_codice) 
{
    var timeout;
    mydialogTrkAnalisi = $.modal({
            contentAlign: 'center',
            width: 900,
            height: 500,
            title: 'Workflow analisi - ' + analisi_codice,
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

        mydialogTrkAnalisi.loadModalContent(urlDialog, null);
};
