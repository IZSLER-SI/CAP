$.template.init();

// CLEditor
var editorTextareaSbloccoP = $('#cleditorRichSbloccoP');
var editorWrapperSbloccoP = editorTextareaSbloccoP.parent();
var editorSbloccoP = editorTextareaSbloccoP.cleditor({ width: 500, height: 180 })[0];
// Update size when resizing
editorWrapperSbloccoP.sizechange
(function () {
    editorSbloccoP.refresh();
});



var mydialogSbloccoProd;
function RichiediSblocco(Prodot_id) {

    var urlDialog = '/Prodotto/PopUpRichiediSbloccoProdotto/' + Prodot_id
    var timeout;
    mydialogSbloccoProd = $.modal({
        contentAlign: 'center',
        width: 580,
        height: 420,
        title: 'Richiesta sblocco prodotto',
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

    mydialogSbloccoProd.loadModalContent(urlDialog, null);
};
function saveDataPopUpProdottoSblocco(Prodot_id) {

   // var Prodot_id = $('#Prodot_id').val();
    var motivo = $('#cleditorRichSbloccoP').val();
    var priorita_id = $('input[name=button-radio-sblocco]:checked').val();
    var emp = { Prodotto_id: Prodot_id,
        Prodotto_Motivo: motivo,
        Prodotto_T_RICPRI_ID: priorita_id
    };
    var urlRedirect = "/Prodotto/ProdottoEdit/" + Prodot_id;
    var urlInvio = '/Prodotto/RichiediSbloccoProdotto';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) 
            {
                mydialogSbloccoProd.closeModal();
             location.href = urlRedirect; 
            }
         else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
        }
    });
   
}




