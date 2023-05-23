$.template.init();

var pageInitial;
var mydialogCopiaDaValorizzazioni;
function openRicercaPPProdotti(urlDialog, owner) {
    pageInitial = owner;
    var timeout;
    mydialogCopiaDaProdotto = $.modal({
        contentAlign: 'center',
        width: 1000,
        height: 610,
        title: 'Elenco Prodotti',
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

    mydialogCopiaDaProdotto.loadModalContent(urlDialog, null);
};

function saveDataPPCopiaDaProdotti(prodot_id, prodot_idMaster) {

    var emp =
    {
        Prodotto_id: prodot_id,
        Prodotto_id_Master: prodot_idMaster
    };

    var urlSave = '/Prodotto/ApplicaValorizzazioneAProdotti';
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

    mydialogCopiaDaProdotto.closeModal();
}