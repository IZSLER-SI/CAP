$.template.init();

$('#sorting-PPIntermProdotto').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});

var mydialogPPIntermProdotto ;
function openRicercaPPIntermProdotto(urlDialog,owner) {
    var timeout;
    var scrollTop = $(owner).scrollTop();
    var scrollLeft = $(owner).scrollLeft();
    mydialogPPIntermProdotto = $.modal({
        contentAlign: 'center',
        width: 900,
        height: 550,
        title: 'Scelta Intermedio / Analisi',
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
        onOpen:function()
        {
            owner.scrollTo(scrollLeft, scrollTop);
        }
    });

    mydialogPPIntermProdotto.loadModalContent(urlDialog, null);
};

function clearDataPPProdotto(valpos_id) {
    var prefisso = '';
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
        TipoSalvataggio: "AnalisiIntermedio",
        ProdottoPos_id: valpos_id,
        ProdottoPos_Analisi_id: 0,
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
    $(ProdottoPos_Analisi_desc).text("");
    $(ProdottoPos_Analisi_id).val("");
}

function saveDataPPIntermAnalisiP(valpos_id, id, valore, costo_totale, settore) {
    var prefisso = '';
    var ProdottoPos_Analisi_id = "#" + prefisso + "ProdottoPos_Analisi_id_" + valpos_id;
    var ProdottoPos_Analisi_desc = "#" + prefisso + "ProdottoPos_Analisi_desc_" + valpos_id;
    var ProdottoPos_Prodotto_Desc = "#" + prefisso + "ProdottoPos_Prodotto_Desc_" + valpos_id;
    var ProdottoPos_Prodotto_UDM_ID = "#" + prefisso + "ProdottoPos_Prodotto_UDM_ID_" + valpos_id;
    var ProdottoPos_Prodotto_id = "#" + prefisso + "ProdottoPos_Prodotto_id_" + valpos_id;

    var ProdottoPos_Macchinario_id = "#" + prefisso + "ProdottoPos_Macchinario_id_" + valpos_id;
    var ProdottoPos_Macchinario_Desc = "#" + prefisso + "ProdottoPos_Macchinario_Desc_" + valpos_id;

    var UdM_id = "#" + prefisso + "UdM_" + valpos_id;

    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;




    $(ProdottoPos_Analisi_desc).text(valore);
    $(ProdottoPos_Analisi_id).val(id);
    //$(Costo_Unitario).text(costo_totale);
    $(Costo_Unitario).val(costo_totale);
    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
    $(ProdottoPos_Prodotto_Desc).text(""); // pulizia Prodotto
    $(ProdottoPos_Prodotto_UDM_ID).val("");
    $(ProdottoPos_Prodotto_id).val("");
    $(ProdottoPos_Macchinario_id).val("");
    $(ProdottoPos_Macchinario_Desc).text(""); // pulizia Macchinario
    $(UdM_id).val('25'); // UdM Numero
    


    updateCostoTotale(valpos_id); // $(Costo_Unitario).change();
    
    var emp =
    {
        TipoSalvataggio: "AnalisiIntermedio",
        ProdottoPos_id: valpos_id,
        ProdottoPos_Analisi_id: id,
        ProdottoPos_QuantitaCosto: costo_totale, 
        ProdottoPos_CodSettore: settore
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


    mydialogPPIntermProdotto.closeModal();
}




