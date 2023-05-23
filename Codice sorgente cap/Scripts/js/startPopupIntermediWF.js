$.template.init();

//$('#sorting-advanced').tablesorter();
// Table sort - DataTables
$('#sorting-trackingIntermedi').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': []}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});
//$('#sorting-Utenti').tablesorter();


var mydialogTrkIntermedi;
function openIntermediWorkFlow(urlDialog, analisi_codice) 
{
    var timeout;
    mydialogTrkIntermedi = $.modal({
            contentAlign: 'center',
            width: 900,
            height: 500,
            title: 'Workflow intermedi - ' + analisi_codice,
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

        mydialogTrkIntermedi.loadModalContent(urlDialog, null);
};
