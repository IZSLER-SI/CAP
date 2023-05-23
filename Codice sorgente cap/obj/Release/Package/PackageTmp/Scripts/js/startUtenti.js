
function saveUtenti() {

    var txtUtenteDom = $('#txtUtenteDom').val();
    var txtNome = $('#txtNome').val();
    var txtCognome = $('#txtCognome').val();
    var txtEmail = $('#txtEmail').val();
    var Utente_id = $('#Utente_id').val();
    var Utente_Lock = $('input[name=checkbox_lock]').attr('checked') ? true : false;

    var emp = { Utente_ID: Utente_id,
        Utente_User: txtUtenteDom,
        Utente_Nome: txtNome,
        Utente_Cognome: txtCognome,
        Utente_Email: txtEmail,
        Utente_Lock: Utente_Lock
    }
    if (txtUtenteDom == null || txtUtenteDom == "" || txtNome == null || txtNome == "" ||
        txtCognome == null || txtCognome == "" || txtEmail == null || txtEmail == "") {
        showAlertErrorGeneric("Valorizzare tutti i campi prima di procedere al salvataggio.","Attenzione");
        return;
    }

    var urlSave = '/Settings/saveUtenti';
    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) {

                location.href = "/Settings/UtentiEdit/" + retdata.id;
            }
            else {

                showAlertErrorGeneric(retdata.infopersonali, "Riscontrati i seguenti errori:");
                
            }

        }
    });
}
function EliminaUtenti_profili_gruppi() {
    var checked = [];
    $("input[name='multicheckSelectUtenti']:checked").each(function () {
        checked.push(parseInt($(this).val()));
    });
    var urlSave = '/Settings/EliminaProfiliUtente';
  
    var parms = { ProdottoPosIds: checked };

    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(parms),
        datatype: "json",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (!retdata.ok) {
                showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
            }
            else {

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
        }
    });

}
function NuovoUtenti_profili_gruppi() {

    var Utente_id = $('#SelectUtente_ID').val();
    var emp = { M_Utprgr_Utente_Id: Utente_id}
    var urlSave = '/Settings/saveNuovoUtenti_profili_gruppi';
    $.ajax({
        type: "POST",
        url: urlSave,
        data:  JSON.stringify(emp),
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

//                location.href = "/Settings/Utenti";
            }
            else {

                showAlertErrorGeneric(retdata.infopersonali, "Riscontrati i seguenti errori:");

            }

        }
    });
}


function saveGruppoPrincipale(UtenteGruppoProfilo_ID, M_Utprgr_Profil_Id_OLD) {


   
    var M_Utprgr_Id = UtenteGruppoProfilo_ID;
    var M_Utprgr_Grurep_Id = $('#Grurep_ID_' + UtenteGruppoProfilo_ID).val();
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
                        if (M_Utprgr_Profil_Id_OLD !=undefined) 
                        {
                            $('#Profilo_id_' + UtenteGruppoProfilo_ID).val(M_Utprgr_Profil_Id_OLD);
                        }
                        $('input[name=M_Utprgr_Flg_Principale_' + UtenteGruppoProfilo_ID + ']').attr('checked', false);

                    }

                }
            });
        }

        