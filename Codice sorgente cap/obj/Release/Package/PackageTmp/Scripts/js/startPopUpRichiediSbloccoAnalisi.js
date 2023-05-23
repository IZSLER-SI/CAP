$.template.init();

// CLEditor
var editorTextareaSblocco = $('#cleditorRichSblocco');
var editorWrapperSblocco = editorTextareaSblocco.parent();
var editorSblocco = editorTextareaSblocco.cleditor({ width: 500, height: 180 })[0];
// Update size when resizing
editorWrapperSblocco.sizechange
(function () {
    editorSblocco.refresh();
});



var mydialogSbloccoAnalisi;
function RichiediSblocco(Analisi_id) {

    var urlDialog = '/Analisi/PopUpRichiediSbloccoAnalisi/' + Analisi_id
    var timeout;
    mydialogSbloccoAnalisi = $.modal({
        contentAlign: 'center',
        width: 580,
        height: 420,
        title: 'Richiesta sblocco analisi',
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

    mydialogSbloccoAnalisi.loadModalContent(urlDialog, null);
};
function saveDataPopUpAnalisiSblocco(Analisi_id) {

    var analisi_id = $('#analisi_id').val();
    var motivo = $('#cleditorRichSblocco').val();
    var priorita_id = $('input[name=button-radio-sblocco]:checked').val();
    var emp = { Analisi_id: analisi_id,
        Analisi_Motivo: motivo,
        Analisi_T_RICPRI_ID: priorita_id
    };
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var urlInvio = '/Analisi/RichiediSbloccoAnalisi';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) 
            {
                mydialogSbloccoAnalisi.closeModal();
             location.href = urlRedirect; 
            }
         else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
        }
    });
   
}




