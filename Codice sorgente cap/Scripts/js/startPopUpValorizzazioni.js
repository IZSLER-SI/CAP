$.template.init();

var pageInitial;
var mydialogCopiaDaValorizzazioni;
function openRicercaPPValorizzazioni(urlDialog,owner) {
    pageInitial = owner;
    var timeout;
    mydialogCopiaDaValorizzazioni = $.modal({
        contentAlign: 'center',
        width: 1000,
        height: 610,
        title: 'Elenco Valorizzazioni',
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

    mydialogCopiaDaValorizzazioni.loadModalContent(urlDialog, null);
};

function saveDataPPCopiaDaValorizzazioni(valori_id, valori_idMaster) {
   
    var emp =
    {
        Analisi_id: valori_id,
        Analisi_id_Master: valori_idMaster
    };

    var urlSave = '/Analisi/ApplicaValorizzazioneAdAnalisi';
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
            else {
                pageInitial.location.reload();
            }
        }
    });



    mydialogCopiaDaValorizzazioni.closeModal();
}
