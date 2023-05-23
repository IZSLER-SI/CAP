// Call template init (optional, but faster if called manually)
$.template.init();






var mydialogPPGruppiRepartiM;
function openRicercaPPGruppiReparti(urlDialog) {
    var timeout;
    mydialogPPGruppiRepartiM = $.modal({
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

    mydialogPPGruppiRepartiM.loadModalContent(urlDialog, null);
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
function saveDataGruppiRepartiM(Grurep_ID, Grurep_Desc, flagReparto, flagInsert) 
{
    $('#Gruppo_Desc').val(Grurep_Desc);
    $('#Gruppo_ID').text(Grurep_ID);
    $('#Gruppo_flgReparto').text(flagReparto);
    mydialogPPGruppiRepartiM.closeModal();
}



function saveInsertMacchinario() 
{
    var mac_Descrizione = $('#mac_Descrizione').val();
    var mac_Codice = $('#mac_Codice').val();
    var mac_GruppoID = $('#Gruppo_ID').text();
    var mac_Val_Strumentazione = $('#mac_Val_Strum').val();
    var mac_Costo_Manutenzione = $('#mac_Costo_Manutenzione').val();
    var mac_Vita_Utile = $('#mac_Vita_Utile').val();
    var mac_Minuti_Annuali = $('#mac_Minuti_Annuali').val();


    //    var analisi_FlagReparto = $('#Gruppo_flgReparto').text();
    if (mac_Codice == "" || mac_Codice == null) {
        showAlertError("Inserire un codice macchinario", "Errore!");
        return;
    }
    if (mac_Minuti_Annuali == "" || mac_Minuti_Annuali == null) {
        showAlertError("Inserire un valore di minuti annuali di utilizzo", "Errore!");
        return;
    }
    if (mac_Vita_Utile == "" || mac_Vita_Utile == null) {
        showAlertError("Inserire un valore di vita utile", "Errore!");
        return;
    }
    if (mac_Descrizione == "" || mac_Descrizione == null) {
        showAlertError("Inserire una descrizione", "Errore!");
        return;
    }
   /* if (mac_GruppoID == "" || mac_GruppoID == null) {
        showAlertError("Selezionare un Gruppo / Reparto", "Errore!");
        return;
    }*/
  
    var emp = {
        Macchi_Codice: mac_Codice,
        Macchi_Desc: mac_Descrizione,
        Macchi_GruppoID: mac_GruppoID,
        Macchi_Valore_Strumentazione:mac_Val_Strumentazione,
        Macchi_Costo_Manutenzione_Annuo:mac_Costo_Manutenzione,
        Macchi_Vita_Utile_Anni:mac_Vita_Utile,
        Macchi_Minuti_Anno: mac_Minuti_Annuali
    };
    var urlSave = '/Settings/SaveInsertMacchinario';
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
                             { modal.closeModal(); location.href = "/Settings/MacchinarioEdit/" + retdata.id; }
                         }

                    }
                }
                );

            }
        }
    });
}

function saveMacchinario() {
    var mac_id = $('#mac_id').val();
    var mac_Descrizione = $('#mac_Descrizione').val();
    var mac_Codice = $('#mac_Codice').val();
    var mac_GruppoID = $('#Gruppo_ID').text();

    var mac_Val_Strumentazione = $('#mac_Val_Strum').val();
    var mac_Costo_Manutenzione = $('#mac_Costo_Manutenzione').val();
    var mac_Vita_Utile = $('#mac_Vita_Utile').val();
    var mac_Minuti_Annuali = $('#mac_Minuti_Annuali').val();

    //    var analisi_FlagReparto = $('#Gruppo_flgReparto').text();
    if (mac_Codice == "" || mac_Codice == null) {
        showAlertError("Inserire un codice macchinario", "Errore!");
        return;
    }
    if (mac_Minuti_Annuali == "" || mac_Minuti_Annuali == null) {
        showAlertError("Inserire un valore di minuti annuali di utilizzo", "Errore!");
        return;
    }
    if (mac_Vita_Utile == "" || mac_Vita_Utile == null) {
        showAlertError("Inserire un valore di vita utile", "Errore!");
        return;
    }
    if (mac_Descrizione == "" || mac_Descrizione == null) {
        showAlertError("Inserire una descrizione", "Errore!");
        return;
    }
    var emp = {
        Macchi_ID: mac_id,
        Macchi_Codice: mac_Codice,
        Macchi_Desc: mac_Descrizione,
        Macchi_GruppoID: mac_GruppoID,
        Macchi_Valore_Strumentazione:mac_Val_Strumentazione,
        Macchi_Costo_Manutenzione_Annuo:mac_Costo_Manutenzione,
        Macchi_Vita_Utile_Anni:mac_Vita_Utile,
        Macchi_Minuti_Anno:mac_Minuti_Annuali
    };
    var urlSave = '/Settings/SaveMacchinario';
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
                                { modal.closeModal(); location.href = "/Settings/MacchinarioEdit/"  +mac_id; }
                         }

                     }
                }
                );
                
            }
        }
    });
}




function EliminaMacchinario() {
    var mac_id = $('#mac_id').val();
    var urlInvio = '/Settings/EliminaMacchinario';
    var urlRedirect = "/Settings/Macchinari";
    var messaggioConferma = 'Eliminare il macchinario corrente?';
    var emp = { Macchi_ID: mac_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}
function sendData(messaggioConferma, urlInvio, emp, urlRedirect) {
    var conf = $.modal.confirm;
    conf(messaggioConferma,
        function () {
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
                        { showAlertErrorGeneric(retdata.infopersonali,'Errore'); }
                    }
            });
        },
        function () { $.modal.alert('Annullato.'); }
        );
}


