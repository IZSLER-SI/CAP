$.template.init();

$('#sorting-PPProdottiP').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});

var mydialogPPProdottiP;
function openRicercaPPProdottiP(urlDialog, owner) {
    var timeout;
    var scrollTop = $(owner).scrollTop();
    var scrollLeft = $(owner).scrollLeft();
    mydialogPPProdottiP = $.modal({
        contentAlign: 'center',
        width: 900,
        height: 550,
        title: 'Scelta Prodotto',
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

    mydialogPPProdottiP.loadModalContent(urlDialog, null);
};

var mydialogPPUDMP;

function openPPUDMP(urlDialog, owner)  
{
    var timeout1; 
    var scrollTop = $(owner).scrollTop();
    var scrollLeft = $(owner).scrollLeft();
    mydialogPPUDMP = $.modal({
        contentAlign: 'center',
        width: 700,
        height: 345,
        title: 'Scelta Unità di Misura',
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
            clearTimeout(timeout1);
            owner.scrollTo(scrollLeft, scrollTop);
        },
        onOpen: function () {
            owner.scrollTo(scrollLeft, scrollTop);
        }
    });

    mydialogPPUDMP.loadModalContent(urlDialog, null);
   

};
function saveUdMRatioP(valpos_id) {
   
        
    var udmRatio = $('#udmRatio').val();
    //udmRatio = udmRatio.replace(",", ".");
    var floatUdmRatio = parseFloat(udmRatio);
    
    //var analisi_Pos_id = $('#Analisi_Pos_id').val();
    var prefisso = "";
    //var ProdUDM = '#' + prefisso + 'AnalisiPos_Prodotto_UDM_ID_' + valpos_id;
    var UdMCoeff = '#' + prefisso + 'UdMCoeff_' + valpos_id;

    var UdMProdot = '#' + prefisso + 'UdMProdot_' + valpos_id;

    $(UdMCoeff).val(udmRatio);

    var emp =
    {
        TipoSalvataggio: "UdMRatio",
        ProdottoPos_id: valpos_id,
        ProdottoPos_CoeffConversioneString: udmRatio
        
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
            else { mydialogPPUDMP.closeModal(); }
        }
    });
   updateCostoTotale(valpos_id);
    
}

function clearDataPopUpProdottiP(valpos_id) {
    var prefisso = '';
    var ProdottoPos_Prodotto_id = "#" + prefisso + "ProdottoPos_Prodotto_id_" + valpos_id;
    var ProdottoPos_Prodotto_Desc = "#" + prefisso + "ProdottoPos_Prodotto_Desc_" + valpos_id;
    var ProdottoPos_Prodotto_UDM_ID = "#" + prefisso + "ProdottoPos_Prodotto_UDM_ID_" + valpos_id;

    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;


    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    //$(Costo_Unitario).text("0,00");
    $(Costo_Unitario).val("0,00");

    var ProdottoPos_Analisi_desc = "#" + prefisso + "ProdottoPos_Analisi_desc_" + valpos_id;
    $(ProdottoPos_Analisi_desc).text(""); // pulizia AnalisiIntermedio
    $(ProdottoPos_Prodotto_UDM_ID).val("");

 
    var UdMCoeff = '#' + prefisso + 'UdMCoeff_' + valpos_id;
    // udmRatio = ""; 
    // + conseguente salvataggio sul DB di UDM RATIO
    var udmRatio = "1";
    $(UdMCoeff).val(udmRatio);


    updateCostoTotale(valpos_id);
    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
  

    var emp =
    {
        TipoSalvataggio: "Prodotto",
        ProdottoPos_id: valpos_id,
        ProdottoPos_Prodotto_id: 0,
        ProdottoPos_QuantitaCosto: 0,
        ProdottoPos_CoeffConversioneString: udmRatio
    };

    var urlSave = ' /Prodotto/SaveValPos';
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
    $(ProdottoPos_Prodotto_Desc).text("");
    $(ProdottoPos_Prodotto_id).val("");
}
function saveDataPPProdottoP(valpos_id, id, valore, costo_totale, udmID, descrizioneUdm) {
    var prefisso = '';
    var ProdottoPos_Prodotto_id = "#" + prefisso + "ProdottoPos_Prodotto_id_" + valpos_id;
    var ProdottoPos_Prodotto_Desc = "#" + prefisso + "ProdottoPos_Prodotto_Desc_" + valpos_id;
    var ProdottoPos_Prodotto_UDM_ID = "#" + prefisso + "ProdottoPos_Prodotto_UDM_ID_" + valpos_id;


    var ProdottoPos_Analisi_desc = "#" + prefisso + "ProdottoPos_Analisi_desc_" + valpos_id;

    var Costo_Unitario = "#" + prefisso + "Costo_Unitario_" + valpos_id;
    var FigProf_id = "#" + prefisso + "FigProf_" + valpos_id;



    $(ProdottoPos_Prodotto_Desc).text(valore);
    $(ProdottoPos_Prodotto_id).val(id);
    $(ProdottoPos_Prodotto_UDM_ID).val(udmID);
    //$(Costo_Unitario).text(costo_totale);
    $(FigProf_id).val('0'); // pulizia LivelloFigura professionale
    $(ProdottoPos_Analisi_desc).text(""); // pulizia AnalisiIntermedio
   
    var UdMCoeff = '#' + prefisso + 'UdMCoeff_' + valpos_id;
    var UdMSel = '#' + prefisso + 'UdM_' + valpos_id;

    var UdmDesc = "#" + prefisso + "UdMProdotDesc_" + valpos_id;
    $(UdmDesc).val(descrizioneUdm);
    var UdmSelVal=$(UdMSel).val();
    // se il AnalisiPos_Prodotto_UDM_ID e' == UdM_ID  --> udmRatio == 1 
    // altrimenti --> udmRatio = ""; 
    // + conseguente salvataggio sul DB di UDM RATIO
    var udmRatio = "1";
    if (udmID.toString() != UdmSelVal)
        udmRatio = "";
    $(UdMCoeff).val(udmRatio); 

    $(Costo_Unitario).val(costo_totale);
    updateCostoTotale(valpos_id);



    var emp =
    {
        TipoSalvataggio: "Prodotto",
        ProdottoPos_id: valpos_id,
        ProdottoPos_Prodotto_id: id,
        ProdottoPos_QuantitaCosto: costo_totale,
        ProdottoPos_CoeffConversioneString: udmRatio
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


    mydialogPPProdottiP.closeModal();
}




