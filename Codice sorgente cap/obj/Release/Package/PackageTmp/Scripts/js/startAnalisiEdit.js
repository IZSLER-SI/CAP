// Call template init (optional, but faster if called manually)
$.template.init();

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

function checkLottoQualitaAnalisi() 
{
    var lotto = $('#Analisi_Dim_Lotto').val();
    var qualita = $('#Analisi_nr_Campioni').val();
    var intLotto = parseInt(lotto, 10);

    var intQualita = parseInt(qualita, 10);
    if (intQualita > intLotto) {
        showAlertError("La 'Dimensione lotto' deve essere sempre maggiore al 'Nr. Campioni Qualità'.", "Attenzione");
        $('#Analisi_nr_Campioni').val(lotto);
    }
}

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



function saveAnalisi() {
    var analisi_id = $('#analisi_id').val();

    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();
    var analisi_nr_Campioni = $('#Analisi_nr_Campioni').val();

    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();
   // var analisi_Codice_Descrizione = $('#Analisi_Codice_Descrizione').val();
    
    //Ric#3
    var analisi_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var analisi_flag_assegna_gruppo = $('input[name=Analisi_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (analisi_utente_id == "" || analisi_utente_id == null || analisi_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    if (analisi_Dim_Lotto == "" || analisi_Dim_Lotto == null || analisi_Dim_Lotto<=0) {
        showAlertErrorGeneric("Inserire una Dimesione Lotto", "Errore!");
        return;
    }

    var emp = {
        Analisi_id: analisi_id,
        Analisi_Dim_Lotto: analisi_Dim_Lotto,
        Analisi_nr_Campioni:analisi_nr_Campioni,
        Analisi_Matrice: analisi_Matrice,
        Analisi_Peso_Positivo: analisi_Peso_Positivo,
        Analisi_utente_id: analisi_utente_id, //Ric#3
        Analisi_flg_assegn_al_gruppo: analisi_flag_assegna_gruppo //Ric#3

        //,Analisi_Codice_Descrizione: analisi_Codice_Descrizione
    };
    var urlSave = '/Analisi/SaveValAnalisiTot';
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
                        'Chiudi' :
                         {
                                classes:	'blue-gradient glossy big full-width',
                                click: function (modal) 
                                { modal.closeModal();location.href = "/Analisi/AnalisiEdit/" + analisi_id; }
                         }

                     }
                }
                );
                
            }
        }
    });
}

//Ric#3
function changeAnalisiUtenteAss() {
    var analisi_id = $('#analisi_id').val();

    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();
    var analisi_nr_Campioni = $('#Analisi_nr_Campioni').val();

    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();
    // var analisi_Codice_Descrizione = $('#Analisi_Codice_Descrizione').val();

    //Ric#3
    var analisi_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var analisi_flag_assegna_gruppo = $('input[name=Analisi_flag_assegna_gruppo]').attr('checked') ? true : false;

    if (analisi_utente_id == "" || analisi_utente_id == null || analisi_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    var emp = {
        Analisi_id: analisi_id,
        Analisi_Dim_Lotto: analisi_Dim_Lotto,
        Analisi_nr_Campioni: analisi_nr_Campioni,
        Analisi_Matrice: analisi_Matrice,
        Analisi_Peso_Positivo: analisi_Peso_Positivo,
        Analisi_utente_id: analisi_utente_id, //Ric#3
        Analisi_flg_assegn_al_gruppo: analisi_flag_assegna_gruppo //Ric#3

        //,Analisi_Codice_Descrizione: analisi_Codice_Descrizione
    };

    var urlSave = '/Analisi/ChangeAnalisiUtenteAss';
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
                             { modal.closeModal(); location.href = "/Analisi/AnalisiEdit/" + analisi_id; }
                         }

                    }
                }
                );

            }
        }
    });
}

function revisioneFormale() {
    var analisi_id = $('#analisi_id').val();

    var checked = [];
    $("input[name='multicheckSelect']").each(function () {
        checked.push(parseInt($(this).val()));
    });
     var Schecked = [];
    $("input[name='SmulticheckSelect']").each(function () {
        Schecked.push(parseInt($(this).val()));
    }); 

    var emp = {
        Analisi_id: analisi_id,
        AnalisiPosIds: checked,
        AnalisiPosSIds:Schecked
    };
    var urlSave = '/Analisi/RevisioneFormale';
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
                             { modal.closeModal(); location.href = "/Analisi/AnalisiEdit/" + analisi_id; }
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

function ApprovaEdInviaCdGAnalisi() {
    var analisi_id = $('#analisi_id').val();
    var urlInvio = '/Analisi/ApprovaEdInviaCdGAnalisi';
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var messaggioConferma = 'Approvare l\'analisi corrente?';
    var emp = { Analisi_id: analisi_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}
function InvCDGAnalisi(){ alert('InvCDGAnalisi'); }
function DeliberaAnalisi() {
    var analisi_id = $('#analisi_id').val();
    var urlInvio = '/Analisi/DeliberaAnalisi';
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var messaggioConferma = 'Deliberare l\'analisi corrente?';
    var emp = { Analisi_id: analisi_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect) }


function SbloccaAnalisi() 
{
    var analisi_id = $('#analisi_id').val();
    var urlInvio = '/Analisi/SbloccaAnalisi';
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var messaggioConferma = 'Sbloccare l\'analisi?';
    var emp = {Analisi_id: analisi_id}
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}

function AttualizzaPosizioni() {
    var analisi_id = $('#analisi_id').val();
    var urlInvio = '/Analisi/AttualizzaPosizioni';
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var messaggioConferma = 'Aggiornare le posizioni?';
    //      var emp = { Analisi_id: analisi_id }

    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();
    var analisi_nr_Campioni = $('#Analisi_nr_Campioni').val();

    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();

    //Ric#3
    var analisi_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var analisi_flag_assegna_gruppo = $('input[name=Analisi_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (analisi_utente_id == "" || analisi_utente_id == null || analisi_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }
    
    if (analisi_Dim_Lotto == "" || analisi_Dim_Lotto == null || analisi_Dim_Lotto <= 0) {
        showAlertErrorGeneric("Inserire una Dimesione Lotto", "Errore!");
        return;
    }

    var emp = {
        Analisi_id: analisi_id,
        Analisi_Dim_Lotto: analisi_Dim_Lotto,
        Analisi_nr_Campioni: analisi_nr_Campioni,
        Analisi_Matrice: analisi_Matrice,
        Analisi_Peso_Positivo: analisi_Peso_Positivo,
        Analisi_utente_id: analisi_utente_id, //Ric#3
        Analisi_flg_assegn_al_gruppo: analisi_flag_assegna_gruppo //Ric#3
        //,Analisi_Codice_Descrizione: analisi_Codice_Descrizione
    };
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}


function InvValAnalisi() 
{
    var analisi_id = $('#analisi_id').val();

    var analisi_Dim_Lotto = $('#Analisi_Dim_Lotto').val();
    var analisi_nr_Campioni = $('#Analisi_nr_Campioni').val();

    var analisi_Matrice = $('#Analisi_Matrice').val();
    var analisi_Peso_Positivo = $('#analisi_Peso_Positivo').val();
    // var analisi_Codice_Descrizione = $('#Analisi_Codice_Descrizione').val();

    //Ric#3
    var analisi_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var analisi_flag_assegna_gruppo = $('input[name=Analisi_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (analisi_utente_id == "" || analisi_utente_id == null || analisi_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    if (analisi_Dim_Lotto == "" || analisi_Dim_Lotto == null || analisi_Dim_Lotto <= 0) {
        showAlertErrorGeneric("Inserire una Dimesione Lotto", "Errore!");
        return;
    }



    var checked = [];
    $("input[name='multicheckSelect']").each(function () {
        checked.push(parseInt($(this).val()));
    });
    var Schecked = [];
    $("input[name='SmulticheckSelect']").each(function () {
        Schecked.push(parseInt($(this).val()));
    });


    var emp = {
        Analisi_id: analisi_id,
        Analisi_Dim_Lotto: analisi_Dim_Lotto,
        Analisi_nr_Campioni: analisi_nr_Campioni,
        Analisi_Matrice: analisi_Matrice,
        Analisi_Peso_Positivo: analisi_Peso_Positivo,
        AnalisiPosIds: checked, 
        AnalisiPosSIds: Schecked,
        Analisi_utente_id: analisi_utente_id, //Ric#3
        Analisi_flg_assegn_al_gruppo: analisi_flag_assegna_gruppo //Ric#3
        //,Analisi_Codice_Descrizione: analisi_Codice_Descrizione
    };
    
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var urlInvio = '/Analisi/InviaValidatoreAnalisi';

    var conf = $.modal.confirm;
    //  conf.settings.confirmText = 'Si';
    conf(   
            'Inviare l\'analisi al validatore?',
            function () 
            {
                $.ajax({
                    type: "POST",
                    url: urlInvio,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) { location.href = "/Analisi/AnalisiEdit/" + analisi_id; }
                        else { showAlertErrorGeneric(retdata.infopersonali,'Attenzione'); }
                    }
                });
             },
             function () { $.modal.alert('Annullato.'); }
    );
 }

function CheckPosizioniAnalisi() 
{
    var analisi_id = $('#analisi_id').val();

    var checked = [];
    $("input[name='multicheckSelect']").each(function () {
        checked.push(parseInt($(this).val()));
    });
    var Schecked = [];
    $("input[name='SmulticheckSelect']").each(function () {
        Schecked.push(parseInt($(this).val()));
    });

    var emp = { Analisi_id: analisi_id,
                AnalisiPosIds:checked,
                AnalisiPosSIds:Schecked
                };
    var urlRedirect = "/Analisi/AnalisiEdit/" + analisi_id;
    var urlInvio = '/Analisi/CheckPosizioniAnalisi';
    $.ajax({
        type: "POST",
        url: urlInvio,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) { showAlertMessageGeneric('Posizioni valide','Controllo Posizioni'); }
            else { //location.href = "/Analisi/AnalisiEdit/" + analisi_id;
            showAlertErrorGeneric(retdata.infopersonali, 'Controllo Posizioni'); }
        }
    });
}



function Ricarica(jsonDataChar, optionsChart) {
    var analisi_idVal = $('#analisi_id').val();
            $.getJSON
            (
                "/Analisi/GetElencoChart",
                    { analisi_id: analisi_idVal },
                    function (data) {
                        var len = data.length;
                        var multiArray = new Array(len);
                        var pos = 0;
                        var faseTitolo = 0;
                        var faseRossa = 0;
                        var faseBlu = 0;
                        var maxDelta = 0;
                        $.each(data, function (index, itemData) 
                        {
                            multiArray[pos] = new Array(3);
                            if (pos == 0) 
                            {
                                multiArray[pos][0] = itemData.Titolo;
                                multiArray[pos][1] = itemData.Val1;
                                multiArray[pos][2] = itemData.Val2;
                            }
                            else 
                            {
                                multiArray[pos][0] = itemData.Titolo;
                                multiArray[pos][1] = parseFloat(itemData.Val1);
                                multiArray[pos][2] = parseFloat(itemData.Val2);
                                if (Math.abs(maxDelta) < Math.abs(parseFloat(itemData.Val2) - parseFloat(itemData.Val1))) 
                                {
                                    maxDelta = parseFloat(itemData.Val2) - parseFloat(itemData.Val1);
                                    faseTitolo = itemData.Titolo;
                                    faseRossa = parseFloat(itemData.Val2);
                                    faseBlu = parseFloat(itemData.Val1);
                                }
                            }
                            pos = pos + 1;
                        });

                        jsonDataChar = multiArray;
                        var ldata = google.visualization.arrayToDataTable(jsonDataChar);
                        var chart = new google.visualization.ColumnChart(document.getElementById('demo-chart'));
                        optionsChart = loadOptionsChart();
                        chart.draw(ldata, optionsChart);
                        var res = 0;
                        if (faseRossa > 0) {
                            res = (faseRossa - faseBlu) * 100 / faseRossa;
                        }
                        res = roundNumber(res, 2);
                        var strTot = "";
                        strTot = res.toFixed(2).toString();
                        strTot = strTot.replace('.', ',');
                        $('#Analisi_ScostamentoFase').text(strTot + "%");
                    }
            );
}


