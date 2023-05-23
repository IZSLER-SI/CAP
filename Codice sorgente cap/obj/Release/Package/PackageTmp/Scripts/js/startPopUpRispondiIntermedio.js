$.template.init();

// CLEditor
var editorTextareaRespInter = $('#cleditorRespInter');
var editorWrapperRespInter = editorTextareaRespInter.parent();
var editorRespInter = editorTextareaRespInter.cleditor({ width: 500, height: 180 })[0];
// Update size when resizing
editorWrapperRespInter.sizechange
(function () {
    editorRespInter.refresh();
});



var mydialogRespInter;
function RispondiIntermedio(richie_id) {

    var urlDialog = '/Home/PopUpRispondiIntermedio/' + richie_id
    var timeout;
    mydialogRespInter = $.modal({
        contentAlign: 'center',
        width: 600,
        height: 380,
        title: 'Rispondi richiesta Intermedio',
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

    mydialogRespInter.loadModalContent(urlDialog, null);
};
function saveDataPopUpRispondiIntermedio(ric_id) {

    var motivo = $('#cleditorRespInter').val();
    var priorita_id = $('input[name=button-radio-sblocco]:checked').val();
    var emp = { Richie_id: ric_id,
        Richie_testo: motivo,
        Richie_t_ricpri_id: priorita_id
    };
    var urlRedirect = "/Home";
    var urlInvio = '/Home/RispostaRichiestaIntermedio';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) 
            {
                mydialogRespInter.closeModal();
             location.href = urlRedirect; 
            }
         else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
        }
    });
   
}




