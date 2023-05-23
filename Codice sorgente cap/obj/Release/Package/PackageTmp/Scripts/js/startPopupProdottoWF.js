$.template.init();

//$('#sorting-advanced').tablesorter();
// Table sort - DataTables
$('#sorting-trackingProdotto').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [
        { 'bSortable': false, 'aTargets': [0]},
        { 'sType': 'date', 'aTargets': [0]}
        ],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});
//$('#sorting-Utenti').tablesorter();


var mydialogTrkProdotto;
function openProdottoWorkFlow(urlDialog,prodotto_codice) 
{
    var timeout;
    mydialogTrkProdotto = $.modal({
            contentAlign: 'center',
            width: 900,
            height: 500,
            title: 'Workflow Prodotto - ' + prodotto_codice,
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
                    var a = $('#sorting-trackingProdotto_filter label input:text');
                    a.focus();
                }, 2000);
            }
        });

        mydialogTrkProdotto.loadModalContent(urlDialog, null);
};
