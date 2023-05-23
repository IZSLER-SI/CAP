$.template.init();

// CLEditor
var editorTextareaResp = $('#cleditorResp');
var editorWrapperResp = editorTextareaResp.parent();
var editorResp = editorTextareaResp.cleditor({ width: 500, height: 180 })[0];
// Update size when resizing
editorWrapperResp.sizechange
(function () {
    editorResp.refresh();
});



var mydialogRespingiAnalisi;
function RespingiAnalisi(Analisi_id) {

    var urlDialog = '/Analisi/PopUpRespingiAnalisi/' + Analisi_id
    var timeout;
    mydialogRespingiAnalisi = $.modal({
        contentAlign: 'center',
        width: 600,
        height: 380,
        title: 'Respingi analisi',
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

    mydialogRespingiAnalisi.loadModalContent(urlDialog, null);
};
function saveDataPopUpAnalisiResp(Analisi_id) {

    var analisi_id = $('#analisi_id').val();
    var motivo = $('#cleditorResp').val();
    var emp = { Analisi_id: analisi_id,
        Analisi_Motivo: motivo
    };
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var urlInvio = '/Analisi/RespingiAnalisi';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) 
            {
             mydialogRespingiAnalisi.closeModal();
             location.href = urlRedirect; 
            }
         else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
        }
    });
   
}




