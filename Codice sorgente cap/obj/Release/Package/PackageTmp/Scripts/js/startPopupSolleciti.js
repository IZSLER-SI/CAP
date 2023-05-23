$.template.init();

var editorTextareaSoll = $('#cleditorSolleciti');
var editorWrapperSoll = editorTextareaSoll.parent();
var editorSoll = editorTextareaSoll.cleditor({ width: 650, height: 150 })[0];

            // Update size when resizing
editorWrapperSoll.sizechange
(
    function () 
    {
       // editor.refresh();
    }
 );
    editorWrapperSoll.show
 (
    10, '', function () {
        //editorSoll.disable(true);
        
    }
 );



var mydialog;
function openSollecito(urlDialog) 
{
    var timeout;
     mydialog= $.modal({
            contentAlign: 'center',
            width: 900,
            height: 470,
            title: 'Sollecito',
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
function chiudiSollecito(sollec_id) {

    var urlDelete = '/Shared/ChiudiSollecito';
    var conf = $.modal.confirm;
    var emp = { Sollec_id: sollec_id }
    
    conf('Sicuro di voler archiviare il sollecito?',
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