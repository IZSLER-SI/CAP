$.template.init();

$('#sorting-ProdottoPopUp').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'aTargets': [0]}],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});

var mydialogProdotto;
function openRicercaGruppoReparto(urlDialog) {
    var timeout;
    mydialogProdotto = $.modal({
        contentAlign: 'center',
        width: 950,
        height: 550,
        title: 'Scelta chiave',
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

    mydialogProdotto.loadModalContent(urlDialog, null);
};

function clearDataGruppoReparto(UtenteGruppoProfilo_ID) {

    if (UtenteGruppoProfilo_ID > 0) {
        $('#Grurep_desc_' + UtenteGruppoProfilo_ID).val("");
        $('#Grurep_ID_' + UtenteGruppoProfilo_ID).text("");

        var M_Utprgr_Id = UtenteGruppoProfilo_ID;

        var Utente_id = $('#SelectUtente_ID').val();
        var M_Utprgr_Profil_Id = $('#Profilo_id_' + UtenteGruppoProfilo_ID).val();
        var M_Utprgr_Flg_Principale = $('input[name=M_Utprgr_Flg_Principale_' + UtenteGruppoProfilo_ID + ']').attr('checked') ? true : false;


        var emp =
            {
                M_Utprgr_Id: M_Utprgr_Id,
                M_Utprgr_Grurep_Id: 0,
                M_Utprgr_Profil_Id: M_Utprgr_Profil_Id,
                M_Utprgr_Utente_Id: Utente_id,
                M_Utprgr_Flg_Principale: M_Utprgr_Flg_Principale
            }

        var urlSave = '/Settings/saveUtenti_Profili_Gruppi';
        $.ajax({
            type: "POST",
            url: urlSave,
            data: JSON.stringify(emp),
            datatype: "JSON",
            contentType: "application/json; charset=utf-8",
            success: function (retdata) {
                if (retdata.ok) {

                    var pageNum = parseInt($(".paginate_active_UP").html());
                    var NumEntities = $('#NumEntities_UP').val();
                    var table_search = $('#table_search_UP').val();

                    var pageNum_Down = parseInt($(".paginate_active_Down").html());
                    var NumEntities_Down = $('#NumEntities_Down').val();
                    var table_search_Down = $('#table_search_Down').val();

                    var idR = $('#SelectUtente_ID').val();

                    var emp =
                        {
                            id: idR,
                            NumEntities: NumEntities,
                            SearchDescription: table_search,
                            CurrentPage: pageNum,
                            NumEntities_Down: NumEntities_Down,
                            SearchDescription_Down: table_search_Down,
                            CurrentPage_Down: pageNum_Down
                        }
                    var urlSave = '/Settings/Utenti';
                    callAjax(urlSave, emp);
                }
                else {

                    showAlertErrorGeneric(retdata.infopersonali, "Riscontrati i seguenti errori:");

                }

            }
        });
    }
    else {
        $('#Grurep_desc').val("");
        $('#Grurep_ID').val("");
    }

}
function saveDataGruppoReparto(id, valore, UtenteGruppoProfilo_ID) {


    if (UtenteGruppoProfilo_ID > 0) {

        

        var M_Utprgr_Id = UtenteGruppoProfilo_ID; // $('#M_Utprgr_Id').val();
        var M_Utprgr_Grurep_Id = id;
        var Utente_id = $('#SelectUtente_ID').val();
        var M_Utprgr_Profil_Id = $('#Profilo_id_' + UtenteGruppoProfilo_ID).val();
        var M_Utprgr_Flg_Principale = $('input[name=M_Utprgr_Flg_Principale_'+UtenteGruppoProfilo_ID+']').attr('checked') ? true : false;


        var emp =
            {
                M_Utprgr_Id: M_Utprgr_Id,
                M_Utprgr_Grurep_Id: M_Utprgr_Grurep_Id,
                M_Utprgr_Profil_Id: M_Utprgr_Profil_Id,
                M_Utprgr_Utente_Id: Utente_id,
                M_Utprgr_Flg_Principale: M_Utprgr_Flg_Principale
            }

        var urlSave = '/Settings/saveUtenti_Profili_Gruppi';
        $.ajax({
            type: "POST",
            url: urlSave,
            data: JSON.stringify(emp),
            datatype: "JSON",
            contentType: "application/json; charset=utf-8",
            success: function (retdata) {
                if (retdata.ok) {

                    $('#Grurep_desc_' + UtenteGruppoProfilo_ID).val(valore);
                    $('#Grurep_ID_' + UtenteGruppoProfilo_ID).text(id);

                    var pageNum = parseInt($(".paginate_active_UP").html());
                    var NumEntities = $('#NumEntities_UP').val();
                    var table_search = $('#table_search_UP').val();

                    var pageNum_Down = parseInt($(".paginate_active_Down").html());
                    var NumEntities_Down = $('#NumEntities_Down').val();
                    var table_search_Down = $('#table_search_Down').val();

                    var idR = $('#SelectUtente_ID').val();

                    var emp =
                        {
                            id: idR,
                            NumEntities: NumEntities,
                            SearchDescription: table_search,
                            CurrentPage: pageNum,
                            NumEntities_Down: NumEntities_Down,
                            SearchDescription_Down: table_search_Down,
                            CurrentPage_Down: pageNum_Down
                        }
                    var urlSave = '/Settings/Utenti';
                    callAjax(urlSave, emp);
                }
                else {

                    showAlertErrorGeneric(retdata.infopersonali, "Riscontrati i seguenti errori:");

                }

            }
        });
    }
    else {
        $('#Grurep_desc').val(valore);
        $('#Grurep_ID').val(id);
    }
    mydialogProdotto.closeModal();
}




