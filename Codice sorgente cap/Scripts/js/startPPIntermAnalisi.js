﻿$.template.init();

$('#sorting-PPIntermAnalisi').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});

var mydialogPPIntermAnalisi ;
function openRicercaPPIntermAnalisi(urlDialog,  owner) {
    var timeout;
    var scrollTop = $(owner).scrollTop();
    var scrollLeft = $(owner).scrollLeft();
    mydialogPPIntermAnalisi = $.modal({
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
        onOpen: function () {
            owner.scrollTo(scrollLeft, scrollTop);
        }
    });

    mydialogPPIntermAnalisi.loadModalContent(urlDialog, null);
};

function clearDataPPAnalisi(valpos_id, flgSecondaria) {
    var prefisso = '';
    if (flgSecondaria)
        prefisso = 'S';
    var AnalisiPos_Analisi_id = "#" + prefisso + "AnalisiPos_Analisi_id_" + valpos_id;
    var AnalisiPos_Analisi_desc = "#" + prefisso + "AnalisiPos_Analisi_desc_" + valpos_id;
    var AnalisiPos_Prodotto_Desc = "#" + prefisso + "AnalisiPos_Prodotto_Desc_" + valpos_id;
    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;
    var AnalisiPos_Prodotto_id = "#" + prefisso + "AnalisiPos_Prodotto_id_" + valpos_id;
    var AnalisiPos_Prodotto_UDM_ID = "#" + prefisso + "AnalisiPos_Prodotto_UDM_ID_" + valpos_id;

    var AnalisiPos_Macchinario_id = "#" + prefisso + "AnalisiPos_Macchinario_id_" + valpos_id;
    var AnalisiPos_Macchinario_Desc = "#" + prefisso + "AnalisiPos_Macchinario_Desc_" + valpos_id;
    

    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    //$(Costo_Unitario).text("0,00"); 
    $(Costo_Unitario).val("0,00");

    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
    $(AnalisiPos_Prodotto_Desc).text(""); // pulizia Prodotto
    $(AnalisiPos_Prodotto_UDM_ID).val("");
    $(AnalisiPos_Prodotto_id).val("");

    $(AnalisiPos_Macchinario_id).val("");
    $(AnalisiPos_Macchinario_Desc).text(""); // pulizia Macchinario


    //$(Costo_Unitario).change();
    if (flgSecondaria)
        updateCostoTotaleSec(valpos_id);
    else 
        updateCostoTotale(valpos_id);
    
    
    var emp =
    {
        TipoSalvataggio: "AnalisiIntermedio",
        AnalisiPos_id: valpos_id,
        AnalisiPos_Analisi_id: 0,
        AnalisiPos_QuantitaCosto: 0 
    };

    var urlSave = '/Analisi/SaveValPos';
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
    $(AnalisiPos_Analisi_desc).text("");
    $(AnalisiPos_Analisi_id).val("");
}
function saveDataPPIntermAnalisi(valpos_id, id, valore, costo_totale, flgSecondaria, settore) {
    var prefisso = '';
    if (flgSecondaria)
        prefisso = 'S';
    var AnalisiPos_Analisi_id = "#" + prefisso + "AnalisiPos_Analisi_id_" + valpos_id;
    var AnalisiPos_Analisi_desc = "#" + prefisso + "AnalisiPos_Analisi_desc_" + valpos_id;
    var AnalisiPos_Prodotto_Desc = "#" + prefisso + "AnalisiPos_Prodotto_Desc_" + valpos_id;
    var AnalisiPos_Prodotto_UDM_ID = "#" + prefisso + "AnalisiPos_Prodotto_UDM_ID_" + valpos_id;
    var AnalisiPos_Prodotto_id = "#" + prefisso + "AnalisiPos_Prodotto_id_" + valpos_id;

    var UdM_id = "#" + prefisso + "UdM_" + valpos_id;

    var AnalisiPos_Macchinario_id = "#" + prefisso + "AnalisiPos_Macchinario_id_" + valpos_id;
    var AnalisiPos_Macchinario_Desc = "#" + prefisso + "AnalisiPos_Macchinario_Desc_" + valpos_id;
    


    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;
    


    
    $(AnalisiPos_Analisi_desc).text(valore);
    $(AnalisiPos_Analisi_id).val(id);
    //$(Costo_Unitario).text(costo_totale);
    $(Costo_Unitario).val(costo_totale);
    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
    $(AnalisiPos_Prodotto_Desc).text(""); // pulizia Prodotto
    $(AnalisiPos_Prodotto_UDM_ID).val("");
    $(AnalisiPos_Prodotto_id).val("");
    $(UdM_id).val('25'); // UdM Numero

    $(AnalisiPos_Macchinario_id).val("");
    $(AnalisiPos_Macchinario_Desc).text(""); // pulizia Macchinario

    if (flgSecondaria)
        updateCostoTotaleSec(valpos_id); 
    else 
        updateCostoTotale(valpos_id); // $(Costo_Unitario).change();
    
    var emp =
    {
        TipoSalvataggio: "AnalisiIntermedio",
        AnalisiPos_id: valpos_id,
        AnalisiPos_Analisi_id: id,
        AnalisiPos_QuantitaCosto: costo_totale,
        AnalisiPos_CodSettore : settore
    };
    
    var urlSave = '/Analisi/SaveValPos';
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


    mydialogPPIntermAnalisi.closeModal();
}




