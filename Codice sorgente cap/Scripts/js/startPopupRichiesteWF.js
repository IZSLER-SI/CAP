$.template.init();

//$('#sorting-advanced').tablesorter();
// Table sort - DataTables
$('#sorting-trackingRichiesta').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': []}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});
//$('#sorting-Utenti').tablesorter();


var mydialog;
function openRichiestaWorkFlow(urlDialog,richie_codice) 
{
    var timeout;
     mydialog= $.modal({
            contentAlign: 'center',
            width: 900,
            height: 500,
            title: 'Workflow richiesta - ' + richie_codice,
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
            }, 
            onOpen: function () {

                setTimeout(function () {
                    //alert('11');
                    var a = $('#sorting-trackingRichiesta_filter label input:text');
                    a.focus();
                }, 2000);
            }
        });

    mydialog.loadModalContent(urlDialog, null);
};
