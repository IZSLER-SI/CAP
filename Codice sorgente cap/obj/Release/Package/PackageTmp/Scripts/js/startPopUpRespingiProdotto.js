$.template.init();

// CLEditor
var editorTextareaRespP = $('#cleditorRespP');
var editorWrapperRespP = editorTextareaRespP.parent();
var editorRespP = editorTextareaRespP.cleditor({ width: 500, height: 180 })[0];
// Update size when resizing
editorWrapperRespP.sizechange
(function () {
    editorRespP.refresh();
});



var mydialogRespingiProdotto;
function RespingiProdotto(Prodotto_id) {

    var urlDialog = '/Prodotto/PopUpRespingiProdotto/' + Prodotto_id
    var timeout;
    mydialogRespingiProdotto = $.modal({
        contentAlign: 'center',
        width: 600,
        height: 380,
        title: 'Respingi prodotto',
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

    mydialogRespingiProdotto.loadModalContent(urlDialog, null);
};
function saveDataPopUpProdottoResp(prodotto_id) {

    var motivo = $('#cleditorRespP').val();
    var emp = { Prodotto_id: prodotto_id,
        Prodotto_Motivo: motivo
    };
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var urlInvio = '/Prodotto/RespingiProdotto';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) 
            {
                mydialogRespingiProdotto.closeModal();
             location.href = urlRedirect; 
            }
         else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
        }
    });
   
}




