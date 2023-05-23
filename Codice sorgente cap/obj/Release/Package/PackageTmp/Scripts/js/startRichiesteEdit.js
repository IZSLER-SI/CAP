// Call template init (optional, but faster if called manually)
$.template.init();


//// CLEditor
//var editorTextarea = $('#cleditor');
//var editorWrapper = editorTextarea.parent();
//var editor = editorTextarea.cleditor({width: 650,height: 250})[0];
//// Update size when resizing
//editorWrapper.sizechange
//(   function () {
//        editor.refresh();
//});


/*
$('#table_RicDaLavorare').dataTable({
"oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
'aoColumnDefs': [{ 'bSortable': false, 'bVisible': false, 'aTargets': [0]}],
'aaSorting': [[0, 'desc']],
'sPaginationType': 'full_numbers',
'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
'bFilter': true,
'bInfo': true
});

*/
$('#table_RicDaLavorare').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': true, 'bVisible': true, 'aTargets': [0] }],
    'aaSorting': [[0, 'desc']],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});


$('#table_RicDaInviare').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': true, 'bVisible': true, 'aTargets': [0] }],
    'aaSorting': [[0, 'desc']],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});

$('#table_RicInviate').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [{ 'bSortable': false, 'bVisible': true, 'aTargets': [0]}],
    "aaSorting": [[0, "desc"]],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});


function saveDataConfirm1() {
    $.modal.alert('Me gusta!');
}

function gup(name) {
    name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]");
    var regexS = "[\\?&]" + name + "=([^&#]*)";
    var regex = new RegExp(regexS);
    var results = regex.exec(window.location.href);
    if (results == null)
        return "";
    else
        return results[1];
}


function showAlertError(message, titleMsg) {
    if (titleMsg == "" || titleMsg == null) {
        $.modal.alert(message,
        {
            contentBg: false,
            contentAlign: 'center',
            minWidth: 120,
            width: false,
            maxWidth: 260,
            resizable: false,
            actions: {},
            buttons: {
                'Chiudi': {
                    classes: 'red-gradient glossy big full-width',
                    click: function (modal) { modal.closeModal(); }
                }
            },
            buttonsAlign: 'center',
            buttonsLowPadding: true
        }
       );
    }
    else {
        $.modal.alert(message,
        {
            title: titleMsg,
            contentBg: false,
            contentAlign: 'center',
            minWidth: 120,
            width: false,
            maxWidth: 260,
            resizable: false,

            actions: {},
            buttons: {
                'Chiudi': {
                    classes: 'red-gradient glossy big full-width',
                    click: function (modal) { modal.closeModal(); }
                }
            },
            buttonsAlign: 'center',
            buttonsLowPadding: true
        }
       );
    }
}
function ArchiviaRispostaIntermedio() 
{
    var id = $('#rc_id').val();
    var emp = {
        Richie_id: id
    }
    var urlSave = '/Home/ArchiviaRispostaIntermedio';

    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) {
                location.href = "/Home" ;
            }
            else {

                showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
            }
        }
    });

 }
function InviaRichiesta() 
{
    var id = $('#rc_id').val();
    var titolo = $('#ric_titolo').val();
    var tipoOggetto = $('#tipoOggetto').val();
    var richiedente_utente_id = $('#richiedente_utente_id').val();
    var destinatario_utente_id = $('#ric_utente_id').val();
    if (destinatario_utente_id == "") destinatario_utente_id = null;
    var testo = $('#cleditor').val();
    var valori_id = $('#ric_valid_id').val();
    var staric_desc = $('#ric_stato').val();
    var t_ricpri_id = $('input[name=button-radio]:checked').val();
    if (destinatario_utente_id == null || destinatario_utente_id == '' || destinatario_utente_id == '0')
    {showAlertError('Valorizzare il destinatario della richiesta prima di inviarla.','Impossibile inviare la richiesta');return; }
    var info = gup('origine');
    //Ric#3
    var flag_assegna_gruppo = $('input[name=ric_flag_assegna_gruppo]').attr('checked') ? true : false;

    var emp = {
        Richie_id: id,
        Richie_titolo: titolo,
        Richie_richiedente_utente_id: richiedente_utente_id,
        Richie_destinatario_utente_id: destinatario_utente_id,
        Richie_testo: testo,
        Richie_valori_id: valori_id,
        Richie_t_ricpri_id: t_ricpri_id,
        T_staric_desc: null,
        T_Richie_desc: tipoOggetto,
        //Ric#3
        Richie_flg_assegn_al_gruppo: flag_assegna_gruppo,

        PaginaOrigine: info
    };
    var urlSave = '/Home/InvioRichiesta';

    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) {

                //   $('#field1').val(returndata.id);
                // $('#field2').val(returndata.description);
                //  $.modal.alert('Dati salvati' + "<br/>" + "info personali:" + retdata.infopersonali);
                location.href = "/Home/RichiesteEdit/" + retdata.id;
            }
            else {

                showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
                //$.modal.alert('Errore in fase di salvataggio');
            }

        }
    });


}
function saveRichiesta() {

    

    var id = $('#rc_id').val();
    //     var codice  = $('#').val();
    var titolo = $('#ric_titolo').val();
    var tipoOggetto = $('#tipoOggetto').val();
    //     var data_richiesta  = $('#').val();
    //     var t_richie_id  = $('#').val();
    var richiedente_utente_id = $('#richiedente_utente_id').val();

    //Ric#3
    var flag_assegna_gruppo = $('input[name=Ric_flag_assegna_gruppo]').attr('checked') ? true : false;

    var destinatario_utente_id = $('#ric_utente_id').val();
    if (destinatario_utente_id == "") destinatario_utente_id = null;
    var testo = $('#cleditor').val();
    //     var t_staric_id  = $('#').val();
    var valori_id = $('#ric_valid_id').val();
    //     var t_ricpri_id = $('#').val();
    var staric_desc = $('#ric_stato').val();
    var prodot_id = $('#ric_prodot_id').val();

    var t_ricpri_id = $('input[name=button-radio]:checked').val();
    var info = gup('origine');
    
    ;
                                  
    //var origine =

   
        var emp = {
            Richie_id: id,
            //Richie_codice :   codice ,
            Richie_titolo: titolo,
            //Richie_data_richiesta : data_richiesta   ,
            //Richie_t_richie_id :   t_richie_id ,
            Richie_richiedente_utente_id: richiedente_utente_id,
            Richie_destinatario_utente_id: destinatario_utente_id,
            Richie_testo: testo,
            //Richie_t_staric_id :  t_staric_id  ,
            Richie_valori_id: valori_id,
            Richie_prodot_id:  prodot_id,
            Richie_t_ricpri_id: t_ricpri_id,
            //T_Richie_desc :    null, 
            //T_Richie_color :    null, 
            //Richie_utente_ric_nome : null   ,
            //Richie_utente_ric_cognome :  null  ,
            //Richie_utente_des_nome :    null,
            //Richie_utente_des_cognome :    null, 
            T_staric_desc: null,
            T_Richie_desc: tipoOggetto,

            //Richie_valorizzazione : null   , 
            //T_Ricpri_desc :   null ,
            //T_Ricpri_color :   null

            //Ric#3
            Richie_flg_assegn_al_gruppo: flag_assegna_gruppo,

            PaginaOrigine: info
        };
        var urlSave = '/Home/SaveRichiesta';
        $.ajax({
            type: "POST",
            url: urlSave,
            data: JSON.stringify(emp),
            datatype: "JSON",
            contentType: "application/json; charset=utf-8",
            success: function (retdata) {
                if (retdata.ok) {

                    //   $('#field1').val(returndata.id);
                    // $('#field2').val(returndata.description);
                    //  $.modal.alert('Dati salvati' + "<br/>" + "info personali:" + retdata.infopersonali);
                    location.href = "/Home/RichiesteEdit/" + retdata.id;
                }
                else {

                    showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
                    //$.modal.alert('Errore in fase di salvataggio');
                }

            }
        });

    
}

function deleteRichiesta() {

    var id = $('#rc_id').val();
    var titolo = $('#ric_titolo').val();
    var richiedente_utente_id = $('#richiedente_utente_id').val();
    var destinatario_utente_id = $('#ric_utente_id').val();
    var testo = $('#cleditor').val();
    var staric_desc = $('#ric_stato').val();
    var t_ricpri_id = $('input[name=button-radio]:checked').val();
    var info = gup('origine');

   var emp = {
        Richie_id: id,
        Richie_titolo: titolo,
        Richie_richiedente_utente_id: richiedente_utente_id,
        Richie_destinatario_utente_id: destinatario_utente_id,
        Richie_testo: testo,
        Richie_t_ricpri_id: t_ricpri_id,
        T_staric_desc: null,
        PaginaOrigine: info
    };
        var urlDelete = '/Home/DeleteRichiesta';
        var conf = $.modal.confirm;
      //  conf.settings.confirmText = 'Si';
        conf('Sicuro di voler procedere?',
            function () 
            {
             $.ajax({
                    type: "POST",
                    url: urlDelete,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) {location.href = "/Home/Index/"}
                        else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
                        }
                    });
            },
            function () { $.modal.alert('Annullato.'); }
         );
}


function showAlertError(message, titleMsg) {
    if (titleMsg == "" || titleMsg == null) {
        $.modal.alert(message,
        {
            contentBg: false,
            contentAlign: 'center',
            minWidth: 120,
            width: false,
            maxWidth: 260,
            resizable: false,
            actions: {},
            buttons: {
                'Chiudi': {
                    classes: 'red-gradient glossy big full-width',
                    click: function (modal) { modal.closeModal(); }
                }
            },
            buttonsAlign: 'center',
            buttonsLowPadding: true
        }
       );
    }
    else {
        $.modal.alert(message,
        {
            title: titleMsg,
            contentBg: false,
            contentAlign: 'center',
            minWidth: 120,
            width: false,
            maxWidth: 260,
            resizable: false,

            actions: {},
            buttons: {
                'Chiudi': {
                    classes: 'red-gradient glossy big full-width',
                    click: function (modal) { modal.closeModal(); }
                }
            },
            buttonsAlign: 'center',
            buttonsLowPadding: true
        }
       );
    }
}

function InvValRichiesta() {


    var id = $('#rc_id').val();
    var titolo = $('#ric_titolo').val();
    var richiedente_utente_id = $('#richiedente_utente_id').val();
    var destinatario_utente_id = $('#ric_utente_id').val();
    var testo = $('#cleditor').val();
    var valori_id = $('#ric_valid_id').val();
    var prodot_id = $('#ric_prodot_id').val();

    var staric_desc = $('#ric_stato').val();
    var t_ricpri_id = $('input[name=button-radio]:checked').val();
    var info = gup('origine');

    //Ric#3
    var flag_assegna_gruppo = $('input[name=ric_flag_assegna_gruppo]').attr('checked') ? true : false;

    if (destinatario_utente_id == null || destinatario_utente_id == 0)
    {
        showAlertError("Il destinatario è un campo obblibatorio per l'invio", "Attenzione");
        return;
    }
    if (typeof valori_id != "undefined") 
    {

        if (valori_id == null || valori_id == 0) 
        {
            showAlertError("L'analisi è un campo obblibatorio per l'invio", "Attenzione");
            return;
        }
    }
    if (typeof prodot_id != "undefined") {

        if (prodot_id == null || prodot_id == 0) {
            showAlertError("Il prodotto è un campo obblibatorio per l'invio", "Attenzione");
            return;
        }
    }
    
    var emp = {
        Richie_id: id,
        Richie_titolo: titolo,
        Richie_richiedente_utente_id: richiedente_utente_id,
        Richie_destinatario_utente_id: destinatario_utente_id,
        Richie_testo: testo,
        Richie_t_ricpri_id: t_ricpri_id,
        Richie_valori_id: valori_id,
        Richie_prodot_id: prodot_id,
        T_staric_desc: null,
        //Ric#3
        Richie_flg_assegn_al_gruppo: flag_assegna_gruppo,
        PaginaOrigine: info
    };
    var urlInvio = '/Home/InvValRichiesta';
    var conf = $.modal.confirm;
    //  conf.settings.confirmText = 'Si';
    conf('Inviare la richiesta?',
            function () {
                $.ajax({
                    type: "POST",
                    url: urlInvio,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) { location.href = "/Home/RichiesteEdit/" + retdata.id; }
                        else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
                    }
                });
            },
            function () { $.modal.alert('Annullato.'); }
            );
    
}


