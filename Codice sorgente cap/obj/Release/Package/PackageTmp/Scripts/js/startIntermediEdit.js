// Call template init (optional, but faster if called manually)
$.template.init();




function togglePosSecondarie() {
    loadPonderazione();
}


function loadPonderazione() {
   
    var a = $('input[name=switchPonderazioni]:checked').val();
    if (a == 'on') 
    {
        /*visible*/
        $('#posizioniSecondarie').show();
        $('#btnDown').removeAttr('disabled');
        $('#btnUp').removeAttr('disabled');
        var valCurr = $('#analisi_Peso_Positivo').val();
        if ( valCurr == "" || valCurr ==null)
            $('#analisi_Peso_Positivo').val("0");
    }
    else 
    {
        $('#posizioniSecondarie').hide();
        /*hidden*/
        $('#btnDown').attr('disabled', true);
        $('#btnUp').attr('disabled', true);
        $('#analisi_Peso_Positivo').val("");
    }
  
}

loadPonderazione();




var mydialogPPGruppiReparti;
function openRicercaPPGruppiReparti(urlDialog) {
    var timeout;
    mydialogPPGruppiReparti = $.modal({
        contentAlign: 'center',
        width: 700,
        height: 570,
        title: 'Scelta Gruppo',
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

    mydialogPPGruppiReparti.loadModalContent(urlDialog, null);
};

function cancellaDataPPGruppiIntermedi(flagInsert) 
{
    $('#Gruppo_Desc').val("");
    $('#Gruppo_ID').text("");
    $('#Gruppo_flgReparto').text("");
    

    if (flagInsert == 1) {
        // aggiorno i valori di destinazione e basta
    }
    else 
    {

    }
    
}
function saveDataPPGruppiIntermedi(Grurep_ID, Grurep_Desc, flagReparto, flagInsert) 
{
    $('#Gruppo_Desc').val(Grurep_Desc);
    $('#Gruppo_ID').text(Grurep_ID);
    $('#Gruppo_flgReparto').text(flagReparto);
    if (flagInsert == 1) {
        // aggiorno i valori di destinazione e basta
        
    }
    if (flagInsert == 0) {
        // aggiorno i valori di destinazione e basta e faccio un salvataggio in ajax
    }
    mydialogPPGruppiReparti.closeModal();
}

//funzione utilizzata nel duplica intermedi per utilizzare il popup dei settings
function saveDataGruppiRepartiM(Grurep_ID, Grurep_Desc, flagReparto, flagInsert) {
    $('#Gruppo_Desc').val(Grurep_Desc);
    $('#Gruppo_ID').text(Grurep_ID);
    $('#Gruppo_flgReparto').text(flagReparto);
    if (flagInsert == 1) {
        // aggiorno i valori di destinazione e basta

    }
    if (flagInsert == 0) {
        // aggiorno i valori di destinazione e basta e faccio un salvataggio in ajax
    }
    mydialogPPGruppiReparti.closeModal();
}


function showAlertError(message,titleMsg) {
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


function saveDuplicateIntermedio() {
    var analisi_id = $('#analisi_id').val();
    var analisi_Descrizione = $('#Analisi_Descrizione').val();
    var analisi_CodiceIntermedio = $('#Analisi_CodiceIntermedio').val();
    var analisi_GruppoID = $('#Gruppo_ID').text();
    var analisi_FlagReparto = $('#Gruppo_flgReparto').text();

    if (analisi_GruppoID == "" || analisi_GruppoID == null) {
        showAlertError("Selezionare un Gruppo", "Errore!");
        return;
    }

    var emp = {
        Analisi_id: analisi_id,
        Analisi_CodiceIntermedio: analisi_CodiceIntermedio,
        Analisi_Descrizione: analisi_Descrizione,
        Analisi_Gruppo_id: analisi_GruppoID,
        Analisi_Flag_Reparto: analisi_FlagReparto
    };
    var urlSave = '/Intermedi/SaveDuplicateIntermedio';
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
                $('#Analisi_CodiceIntermedio').val(retdata.infopersonali);
                $.modal.alert("Dati salvati",
                {
                    contentBg: false,
                    contentAlign: 'center',
                    minWidth: 120,
                    width: false,
                    maxWidth: 260,
                    resizable: false,
                    actions: {},
                    buttons:
                    {
                        'Chiudi':
                         {
                             classes: 'blue-gradient glossy big full-width',
                             click: function (modal)
                             { modal.closeModal(); location.href = "/Intermedi/IntermediEdit/" + retdata.id; }
                         }

                    }
                }
                );

            }
        }
    });
}

function saveInsertIntermedio() 
{
    var analisi_Descrizione = $('#Analisi_Descrizione').val();
    var analisi_CodiceIntermedio = $('#Analisi_CodiceIntermedio').val();
    var analisi_GruppoID = $('#Gruppo_ID').text();
    var analisi_FlagReparto = $('#Gruppo_flgReparto').text();
//    if (analisi_CodiceIntermedio == "" || analisi_CodiceIntermedio == null) {
//        showAlertError("Inserire un codice Intermedio", "Errore!");
//        return;
//    }
    if (analisi_GruppoID == "" || analisi_GruppoID == null) {
        showAlertError("Selezionare un Gruppo", "Errore!");
        return;
    }
  
    var emp = {
        Analisi_CodiceIntermedio: analisi_CodiceIntermedio,
        Analisi_Descrizione: analisi_Descrizione,
        Analisi_Gruppo_id:analisi_GruppoID,
        Analisi_Flag_Reparto:analisi_FlagReparto
    };
    var urlSave = '/Intermedi/SaveInsertIntermedio';
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
                $('#Analisi_CodiceIntermedio').val(retdata.infopersonali);
                $.modal.alert("Dati salvati",
                {
                    contentBg: false,
                    contentAlign: 'center',
                    minWidth: 120,
                    width: false,
                    maxWidth: 260,
                    resizable: false,
                    actions: {},
                    buttons:
                    {
                        'Chiudi':
                         {
                             classes: 'blue-gradient glossy big full-width',
                             click: function (modal)
                             { modal.closeModal(); location.href = "/Intermedi/IntermediEdit/" + retdata.id; }
                         }

                    }
                }
                );

            }
        }
    });
}

function saveIntermedio() {
    var analisi_id = $('#analisi_id').val();

    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();

    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();
    var analisi_Descrizione = $('#Analisi_Descrizione').val();
    var analisi_CodiceIntermedio= $('#Analisi_CodiceIntermedio').val();
    var emp = {
        Analisi_id: analisi_id,
        Analisi_CodiceIntermedio:analisi_CodiceIntermedio,
        Analisi_Descrizione:analisi_Descrizione,
        Analisi_Peso_Positivo:analisi_Peso_Positivo
    };
    var urlSave = '/Intermedi/SaveValAnalisiTot';
    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) 
        {
            if (!retdata.ok) 
            {
                showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
            }
            else 
            {
                $.modal.alert("Dati salvati",
                {
                    contentBg:	 false,
                    contentAlign:	'center',
                    minWidth:	 120,
                    width:	 false,
                    maxWidth:	 260,
                    resizable:	 false,
                    actions:	 {},
                    buttons:	 
                    {
                        'Chiudi':
                         {
                                classes:	'blue-gradient glossy big full-width',
                                click: function (modal)
                                { modal.closeModal(); location.href = "/Intermedi/IntermediEdit/" + analisi_id; }
                         }

                     }
                }
                );
                
            }
        }
    });
}


function sendData(messaggioConferma, urlInvio, emp,urlRedirect) 
{
    var conf = $.modal.confirm;
    conf(messaggioConferma,
        function () 
        {
            $.ajax(
            {
                type: "POST",
                url: urlInvio,
                data: JSON.stringify(emp),
                datatype: "JSON",
                contentType: "application/json; charset=utf-8",
                success:
                    function (retdata) {
                        if (retdata.ok)
                        { location.href = urlRedirect; }
                        else
                        { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
                    }
            });
        },
        function () { $.modal.alert('Annullato.'); }
        );
}

function CheckPosizioniIntermedi() {
    var analisi_id = $('#analisi_id').val();
    var emp = { Analisi_id: analisi_id };
    var urlRedirect = "/Intermedi/IntermediEdit/" + analisi_id;
    var urlInvio = '/Intermedi/CheckPosizioniIntermedi';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) { showAlertMessageGeneric('Posizioni valide', 'Controllo Posizioni'); }
            else { showAlertErrorGeneric(retdata.infopersonali, 'Controllo Posizioni'); }
        }
    });
}

function AttualizzaPosizioni() {

    var urlInvio = '/Intermedi/AttualizzaPosizioni';
    var analisi_id = $('#analisi_id').val();
    var urlRedirect = "/Intermedi/IntermediEdit/" + analisi_id;
    var messaggioConferma = 'Aggiornare le posizioni?';

    
    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();
    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();
    var analisi_Descrizione = $('#Analisi_Descrizione').val();
    var analisi_CodiceIntermedio = $('#Analisi_CodiceIntermedio').val();

    var emp = {
        Analisi_id: analisi_id,
        Analisi_CodiceIntermedio: analisi_CodiceIntermedio,
        Analisi_Descrizione: analisi_Descrizione,
        Analisi_Peso_Positivo: analisi_Peso_Positivo
    };

    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}

function SbloccaIntermedio() {
    var analisi_id = $('#analisi_id').val();
    var urlInvio = '/Intermedi/SbloccaIntermedio';
    var urlRedirect = "/Intermedi/IntermediEdit/" + analisi_id;
    var messaggioConferma = 'Sbloccare l\'intermedio?';
    var emp = { Analisi_id: analisi_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}
function BloccaIntermedio() {
    var analisi_id = $('#analisi_id').val();
    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();
    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();
    var analisi_Descrizione = $('#Analisi_Descrizione').val();
    var analisi_CodiceIntermedio = $('#Analisi_CodiceIntermedio').val();


    var urlInvio = '/Intermedi/BloccaIntermedio';
    var urlRedirect = "/Intermedi/IntermediEdit/" + analisi_id;
    var messaggioConferma = 'Bloccare l\'intermedio?';

    var emp = {
        Analisi_id: analisi_id,
        Analisi_CodiceIntermedio: analisi_CodiceIntermedio,
        Analisi_Descrizione: analisi_Descrizione,
        Analisi_Peso_Positivo: analisi_Peso_Positivo
    };

    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}
