$.template.init();

var mydialogPPCopiaModello;
function openRicercaPPCopiaModello(urlDialog) {
    var timeout;
    mydialogPPCopiaModello = $.modal({
        contentAlign: 'center',
        width: 900,
        height: 550,
        title: 'Scelta Modello',
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

    mydialogPPCopiaModello.loadModalContent(urlDialog, null);
};



function saveDataPopUpModello(id,master_id, flagSec, flagProdotto) {

    var emp =
        {
            MasterID: master_id,
            Modello_ID: id,
            FlagSec: flagSec,
            FlagProdotto: flagProdotto
        };
    var urlSave = "";
    var urlRedirect = '';
    if (flagProdotto) 
    { // Prodotto
        urlSave = '/Prodotto/ApplicaModelloAProdotto';
        urlRedirect = '/Prodotto/ProdottoEdit/' + master_id;
    }
    else 
    { // Analisi
        urlSave = '/Analisi/ApplicaModelloAAnalisi';
        urlRedirect = '/Analisi/AnalisiEdit/'+master_id;
    }
    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (!retdata.ok) {
                showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
            }
            else 
            {
                location.href = urlRedirect;
            }
        }
    });
    mydialogPPCopiaModello.closeModal();
}


