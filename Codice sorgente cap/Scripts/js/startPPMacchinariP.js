$.template.init();

//$('#sorting-PPProdotti').dataTable({
//    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
//    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
//    'sPaginationType': 'full_numbers',
//    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
//    'bFilter': true,
//    'bInfo': true
//});

var mydialogPPMacchinarioP;
function openRicercaPPMacchinarioP(urlDialog,owner) {
    var timeout;
    var scrollTop = $(owner).scrollTop();
    var scrollLeft = $(owner).scrollLeft();
    mydialogPPMacchinarioP = $.modal({
        contentAlign: 'center',
        width: 900,
        height: 550,
        title: 'Scelta Apparecchiatura dedicata',
        content: "",
        buttons: {},
        scrolling: true,
        actions: {
            'Chiudi': {
                color: 'red',
                click: function (win) { win.closeModal(); owner.scrollTo(scrollLeft, scrollTop); }
            }
        },
        onClose: function () {
            // Stop simulated loading if needed
            clearTimeout(timeout);
            owner.scrollTo(scrollLeft, scrollTop);
        },
        onOpen: function () {
            owner.scrollTo(scrollLeft, scrollTop);
        }
    });

    mydialogPPMacchinarioP.loadModalContent(urlDialog, null);
};



function clearDataPopUpMacchinarioP(valpos_id) {
    var prefisso = '';

    var ProdottoPos_Macchinario_id = "#" + prefisso + "ProdottoPos_Macchinario_id_" + valpos_id;
    var ProdottoPos_Macchinario_Desc = "#" + prefisso + "ProdottoPos_Macchinario_Desc_" + valpos_id;


    var ProdottoPos_Analisi_id = "#" + prefisso + "ProdottoPos_Analisi_id_" + valpos_id;
    var ProdottoPos_Analisi_desc = "#" + prefisso + "ProdottoPos_Analisi_desc_" + valpos_id;
    var ProdottoPos_Prodotto_Desc = "#" + prefisso + "ProdottoPos_Prodotto_Desc_" + valpos_id;
    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;
    var ProdottoPos_Prodotto_id = "#" + prefisso + "ProdottoPos_Prodotto_id_" + valpos_id;
    var ProdottoPos_Prodotto_UDM_ID = "#" + prefisso + "ProdottoPos_Prodotto_UDM_ID_" + valpos_id;


    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    //$(Costo_Unitario).text("0,00"); 
    $(Costo_Unitario).val("0,00");

    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
    $(ProdottoPos_Prodotto_Desc).text(""); // pulizia Prodotto
    $(ProdottoPos_Prodotto_UDM_ID).val("");
    $(ProdottoPos_Prodotto_id).val("");

    updateCostoTotale(valpos_id);


    var emp =
    {
        TipoSalvataggio: "Macchinario",
        ProdottoPos_id: valpos_id,
        ProdottoPos_Macchinario_id: 0,
        ProdottoPos_QuantitaCosto: 0
    };

    var urlSave = '/Prodotto/SaveValPos';
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
        }
    });

    $(ProdottoPos_Macchinario_Desc).text("");
    $(ProdottoPos_Macchinario_id).val("");
}
function saveDataPPMacchinarioP(valpos_id, id, valore, costo_totale) {
    var prefisso = '';

    var ProdottoPos_Macchinario_id = "#" + prefisso + "ProdottoPos_Macchinario_id_" + valpos_id;
    var ProdottoPos_Macchianario_Desc = "#" + prefisso + "ProdottoPos_Macchinario_Desc_" + valpos_id;

    var ProdottoPos_Analisi_id = "#" + prefisso + "ProdottoPos_Analisi_id_" + valpos_id;
    var ProdottoPos_Analisi_desc = "#" + prefisso + "ProdottoPos_Analisi_desc_" + valpos_id;

    var ProdottoPos_Prodotto_Desc = "#" + prefisso + "ProdottoPos_Prodotto_Desc_" + valpos_id;
    var ProdottoPos_Prodotto_UDM_ID = "#" + prefisso + "ProdottoPos_Prodotto_UDM_ID_" + valpos_id;
    var ProdottoPos_Prodotto_id = "#" + prefisso + "ProdottoPos_Prodotto_id_" + valpos_id;

    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;
    var UdM_id = "#" + prefisso + "UdM_" + valpos_id;

    $(ProdottoPos_Macchianario_Desc).text(valore);
    $(ProdottoPos_Macchinario_id).val(id);
    //$(Costo_Unitario).text(costo_totale);
    $(Costo_Unitario).val(costo_totale);

    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
    $(ProdottoPos_Prodotto_Desc).text(""); // pulizia Prodotto
    $(ProdottoPos_Prodotto_UDM_ID).val("");
    $(ProdottoPos_Prodotto_id).val("");
    $(ProdottoPos_Analisi_desc).text("");
    $(ProdottoPos_Analisi_id).val("");

    $(UdM_id).val("13"); // Udm Minuti
    
    updateCostoTotale(valpos_id); // $(Costo_Unitario).change();

    var emp =
    {
        TipoSalvataggio: "Macchinario",
        ProdottoPos_id: valpos_id,
        ProdottoPos_Macchinario_id: id,
        ProdottoPos_QuantitaCosto: costo_totale
    };

    var urlSave = '/Prodotto/SaveValPos';
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
        }
    });


    mydialogPPMacchinarioP.closeModal();
}




