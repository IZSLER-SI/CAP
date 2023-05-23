$.template.init();


var editorTextareaSollIns = $('#cleditorSollIns');
var editorWrapperSollIns = editorTextareaSollIns.parent();
var editorSollIns = editorTextareaSollIns.cleditor({ width: 650, height: 250 })[0];

            // Update size when resizing
editorWrapperSollIns.sizechange
(
    function () 
    {
       // editor.refresh();
    }
 );
    editorWrapperSollIns.show
 (
    10, '', function () {
        //editorSollIns.disable(true);
        
    }
 );



var mydialog;
function openSollecitoInsert(urlDialog) {
    
    var timeout;
     mydialog= $.modal({
            contentAlign: 'center',
            width: 900,
            height: 470,
            title: 'Inserimento Sollecito',
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

    mydialog.loadModalContent(urlDialog, null);
};

function SalvaSollecito() {
   
    var soll_Utente_id = $('#soll_Utente_id').val();
    var soll_Richie_id = $('#Soll_ric_id').val();
    var soll_Messaggio = $('#cleditorSollIns').val();
    var soll_Datascadenza = $('#soll_data_scadenza').val();

    var urlDelete = '/Shared/SalvaSollecito';
    var conf = $.modal.confirm;
    var emp = { Sollec_Utente_id: soll_Utente_id,
                Sollec_Richie_id: soll_Richie_id,
                Sollec_Messaggio: soll_Messaggio,
                Sollec_Datascadenza: soll_Datascadenza};
    
    conf('Sicuro di voler inviare il sollecito?',
        function () 
        {
            $.ajax({
                type: "POST",
                url: urlDelete,
                data: JSON.stringify(emp),
                datatype: "JSON",
                contentType: "application/json; charset=utf-8",
                success: function (retdata) 
                    {
                        if (retdata.ok) 
                        {   
                            mydialog.closeModal();
                            window.location.reload(true);
                            $.modal.alert("Il solecito è stato inviato correttamente");
                        }
                        else 
                        {
                            showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
                        }
                }
            });

    }, 
    function () {})
}



try
{

    $('#soll_data_scadenza').glDatePicker({ zIndex: 100 });
}
catch(e)
{} 