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

function checkLottoQualitaProdotto() {
    var lotto = $('#Prodotto_Dim_Lotto').val();
    var qualita = $('#Prodotto_nr_Campioni').val();
    var intLotto = parseInt(lotto, 10);

    var intQualita = parseInt(qualita, 10);
    if (intQualita > intLotto) {
        showAlertError("La 'Dimensione lotto' deve essere sempre maggiore al 'Nr. Campioni Qualità'.", "Attenzione");
        $('#Prodotto_nr_Campioni').val(lotto);
    }
}

function getIsChecked(obj) 
{
    try 
    {
        var info = obj[0].parentElement.className;
        if (info.indexOf('checked') >= 0)
            return false;
        return true;
    }
    catch (e) { }
    return false;
}
function getIsCheckedBool(objBool) {
    if (objBool)
        return false;
    return true;
}

function saveProdotto() {
    var prodotto_id = $('#prodotto_id').val();
    var prodotto_Codice_Desc = $('#Prodot_Codice_Desc').val(); 
    var prodotto_Dim_Lotto = $('#Prodotto_Dim_Lotto').val();
    var prodotto_nr_Campioni = $('#Prodotto_nr_Campioni').val();
    var prodotto_Flag_InternoObj = $('#prod_FlagInterno').is(':checked');
    
    var prodotto_UdM_ID = $('#Prodotto_UdM_ID').val();
    var prodotto_Coeff_Conversione = $('#Prodotto_Coeff_Conversione').val();
    var prodotto_Stima_Prod_Anno=$('#Prodotto_Stima_Prod_Anno').val();
    var prodotto_perc_Scarto = $('#Prodotto_perc_Scarto').val();
    var prodotto_Tariffa_Proposta = $('#Prodotto_Tariffa_Proposta').val();
    var prodotto_UdM_ID_Sec = $('#Prodotto_UdM_ID_SEC').val();

    //Ric#3
    var prodotto_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var prodotto_flag_assegna_gruppo = $('input[name=Prodotto_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (prodotto_utente_id == "" || prodotto_utente_id == null || prodotto_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    //Ric#10
    if (prodotto_UdM_ID_Sec == prodotto_UdM_ID & prodotto_Coeff_Conversione != 1 & prodotto_Coeff_Conversione != 0 & prodotto_Coeff_Conversione != null) {
        showAlertErrorGeneric("Se l'unità di misura di magazzino e l'unità di misura lotto produzione sono uguali il coefficiente di conversione deve essere 1 ", "Errore!");
        return;
    }
   
    //Ric#10
    if (prodotto_UdM_ID_Sec != prodotto_UdM_ID && (prodotto_Coeff_Conversione == 0 || prodotto_Coeff_Conversione == "" || prodotto_Coeff_Conversione == null)) {
        showAlertErrorGeneric("E' necessario inserire il coefficiente di conversione", "Errore!");
        return;
    }
   
    //Ric#9 (sostituito da Ric#10)
    //if ((prodotto_Coeff_Conversione == 0 || prodotto_Coeff_Conversione == "" || prodotto_Coeff_Conversione == null) 
    //&& (prodotto_UdM_ID_Sec != 0 && prodotto_UdM_ID_Sec != "" && prodotto_UdM_ID_Sec != null)) {
    //    showAlertErrorGeneric("E' necessario inserire anche il coefficiente di conversione", "Errore!");
    //    return;
    //}

    //Ric#9 (sostituito da Ric#10)
    //if ((prodotto_UdM_ID_Sec == 0 || prodotto_UdM_ID_Sec == "" || prodotto_UdM_ID_Sec == null) 
    //&& (prodotto_Coeff_Conversione != 0 && prodotto_Coeff_Conversione != "" && prodotto_Coeff_Conversione != null)) {
    //    showAlertErrorGeneric("E' necessario inserire anche l'unità di misura convertita", "Errore!");
    //    return;
    //}
    

    //var infoCheck = getIsChecked(prodotto_Flag_InternoObj);
    var emp = {
        Prodotto_id: prodotto_id,
        Prodotto_Dim_Lotto: prodotto_Dim_Lotto,
        Prodotto_nr_Campioni :prodotto_nr_Campioni ,
        Prodotto_Flag_Interno: getIsCheckedBool(prodotto_Flag_InternoObj),
        Prodotto_UdM_ID :prodotto_UdM_ID,
        Prodotto_Coeff_Conversione: prodotto_Coeff_Conversione,
        Prodotto_Codice_Desc: prodotto_Codice_Desc,
        Prodotto_Stima_Prod_Anno:prodotto_Stima_Prod_Anno,
        Prodotto_perc_Scarto: prodotto_perc_Scarto,
        Prodotto_Tariffa_Proposta:prodotto_Tariffa_Proposta,
        Prodotto_UdM_ID_Sec: prodotto_UdM_ID_Sec,
        Prodotto_utente_id: prodotto_utente_id, //Ric#3
        Prodotto_flg_assegn_al_gruppo: prodotto_flag_assegna_gruppo, //Ric#3

    };
    var urlSave = '/Prodotto/SaveValProdottoTot';
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
                                { modal.closeModal();location.href = "/Prodotto/ProdottoEdit/" + prodotto_id; }
                         }

                     }
                }
                );
                
            }
        }
    });
}

//Ric#3
function changeProdottoUtenteAss() {
    var prodotto_id = $('#prodotto_id').val();
    var prodotto_Codice_Desc = $('#Prodot_Codice_Desc').val(); 
    var prodotto_Dim_Lotto = $('#Prodotto_Dim_Lotto').val();
    var prodotto_nr_Campioni = $('#Prodotto_nr_Campioni').val();
    var prodotto_Flag_InternoObj = $('#prod_FlagInterno').is(':checked');
    
    var prodotto_UdM_ID = $('#Prodotto_UdM_ID').val();
    var prodotto_Coeff_Conversione = $('#Prodotto_Coeff_Conversione').val();
    var prodotto_Stima_Prod_Anno=$('#Prodotto_Stima_Prod_Anno').val();
    var prodotto_perc_Scarto = $('#Prodotto_perc_Scarto').val();
    var prodotto_Tariffa_Proposta = $('#Prodotto_Tariffa_Proposta').val();
    var prodotto_UdM_ID_Sec = $('#Prodotto_UdM_ID_SEC').val();

    //Ric#3
    var prodotto_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var prodotto_flag_assegna_gruppo = $('input[name=Prodotto_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (prodotto_utente_id == "" || prodotto_utente_id == null || prodotto_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    //var infoCheck = getIsChecked(prodotto_Flag_InternoObj);
    var emp = {
        Prodotto_id: prodotto_id,
        Prodotto_Dim_Lotto: prodotto_Dim_Lotto,
        Prodotto_nr_Campioni :prodotto_nr_Campioni ,
        Prodotto_Flag_Interno: getIsCheckedBool(prodotto_Flag_InternoObj),
        Prodotto_UdM_ID :prodotto_UdM_ID,
        Prodotto_Coeff_Conversione: prodotto_Coeff_Conversione,
        Prodotto_Codice_Desc: prodotto_Codice_Desc,
        Prodotto_Stima_Prod_Anno:prodotto_Stima_Prod_Anno,
        Prodotto_perc_Scarto: prodotto_perc_Scarto,
        Prodotto_Tariffa_Proposta:prodotto_Tariffa_Proposta,
        Prodotto_UdM_ID_Sec: prodotto_UdM_ID_Sec,
        Prodotto_utente_id: prodotto_utente_id, //Ric#3
        Prodotto_flg_assegn_al_gruppo: prodotto_flag_assegna_gruppo, //Ric#3

    };
    var urlSave = '/Prodotto/ChangeProdottoUtenteAss';
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
                                { modal.closeModal();location.href = "/Prodotto/ProdottoEdit/" + prodotto_id; }
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
function ApprovaEdInviaCdGProdotto() {
    var prodotto_id = $('#prodotto_id').val();
    
    var prodotto_UdM_ID = $('#Prodotto_UdM_ID').val();                          //Ric#10
    var prodotto_UdM_ID_Sec = $('#Prodotto_UdM_ID_SEC').val();                  //Ric#10
    var prodotto_Coeff_Conversione = $('#Prodotto_Coeff_Conversione').val();    //Ric#10

    //Ric#10
    if (prodotto_UdM_ID_Sec != prodotto_UdM_ID && (prodotto_Coeff_Conversione == 0 || prodotto_Coeff_Conversione == "" || prodotto_Coeff_Conversione == null)) {
        showAlertErrorGeneric("E' necessario inserire il coefficiente di conversione", "Errore!");
        return;
    } 

    var urlInvio = '/Prodotto/ApprovaEdInviaCdGProdotto';
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var messaggioConferma = 'Approvare il prodotto corrente?';
    var emp = { Prodotto_id: prodotto_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}
function ApprovaEdInviaMagazzinoProdotto() {
    var prodotto_id = $('#prodotto_id').val();

    var prodotto_UdM_ID = $('#Prodotto_UdM_ID').val();                          //Ric#10
    var prodotto_UdM_ID_Sec = $('#Prodotto_UdM_ID_SEC').val();                  //Ric#10
    var prodotto_Coeff_Conversione = $('#Prodotto_Coeff_Conversione').val();    //Ric#10

    //Ric#10
    if (prodotto_UdM_ID_Sec != prodotto_UdM_ID && (prodotto_Coeff_Conversione == 0 || prodotto_Coeff_Conversione == "" || prodotto_Coeff_Conversione == null)) {
        showAlertErrorGeneric("E' necessario inserire il coefficiente di conversione", "Errore!");
        return;
    } 

    var urlInvio = '/Prodotto/ApprovaEdInviaMagazzinoProdotto';
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var messaggioConferma = 'Approvare il prodotto corrente?';
    var emp = { Prodotto_id: prodotto_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}

function InvCDGAnalisi(){ alert('InvCDGAnalisi'); }
function DeliberaAnalisi() {
    var prodotto_id = $('#prodotto_id').val();
    var urlInvio = '/Prodotto/DeliberaAnalisi';
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var messaggioConferma = 'Deliberare il prodotto corrente?';
    var emp = { Prodotto_id: prodotto_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect) }


function SbloccaProdotto() 
{
    var prodotto_id = $('#prodotto_id').val();
    var urlInvio = '/Prodotto/SbloccaProdotto';
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var messaggioConferma = 'Sbloccare il prodotto?';
    var emp = { Prodotto_id: prodotto_id }
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}

function AttualizzaPosizioni() {

    var prodotto_id = $('#prodotto_id').val();
    var prodotto_Codice_Desc = $('#Prodot_Codice_Desc').val();
    var prodotto_Dim_Lotto = $('#Prodotto_Dim_Lotto').val();
    var prodotto_nr_Campioni = $('#Prodotto_nr_Campioni').val();
    var prodotto_Flag_InternoObj = $('#prod_FlagInterno').is(':checked');

    var prodotto_UdM_ID = $('#Prodotto_UdM_ID').val();
    var prodotto_Coeff_Conversione = $('#Prodotto_Coeff_Conversione').val();
    var prodotto_Stima_Prod_Anno = $('#Prodotto_Stima_Prod_Anno').val();
    var prodotto_perc_Scarto = $('#Prodotto_perc_Scarto').val();
    var prodotto_Tariffa_Proposta = $('#Prodotto_Tariffa_Proposta').val();
    var prodotto_UdM_ID_Sec = $('#Prodotto_UdM_ID_SEC').val();

    //Ric#3
    var prodotto_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var prodotto_flag_assegna_gruppo = $('input[name=Prodotto_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (prodotto_utente_id == "" || prodotto_utente_id == null || prodotto_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    var emp = {
        Prodotto_id: prodotto_id,
        Prodotto_Dim_Lotto: prodotto_Dim_Lotto,
        Prodotto_nr_Campioni: prodotto_nr_Campioni,
        Prodotto_Flag_Interno: getIsCheckedBool(prodotto_Flag_InternoObj),
        Prodotto_UdM_ID: prodotto_UdM_ID,
        Prodotto_Coeff_Conversione: prodotto_Coeff_Conversione,
        Prodotto_Codice_Desc: prodotto_Codice_Desc,
        Prodotto_Stima_Prod_Anno: prodotto_Stima_Prod_Anno,
        Prodotto_perc_Scarto: prodotto_perc_Scarto,
        Prodotto_Tariffa_Proposta: prodotto_Tariffa_Proposta,
        Prodotto_UdM_ID_Sec: prodotto_UdM_ID_Sec,
        Prodotto_utente_id: prodotto_utente_id, //Ric#3
        Prodotto_flg_assegn_al_gruppo: prodotto_flag_assegna_gruppo //Ric#3
    };

    var urlInvio = '/Prodotto/AttualizzaPosizioni';
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var messaggioConferma = 'Aggiornare le posizioni?';
    
    sendData(messaggioConferma, urlInvio, emp, urlRedirect);
}


function InvValProdotto() 
{
    var prodotto_id = $('#prodotto_id').val();
    var prodotto_Codice_Desc = $('#Prodot_Codice_Desc').val();
    var prodotto_Dim_Lotto = $('#Prodotto_Dim_Lotto').val();
    var prodotto_nr_Campioni = $('#Prodotto_nr_Campioni').val();
    var prodotto_Flag_InternoObj = $('#prod_FlagInterno').is(':checked');

    var prodotto_UdM_ID = $('#Prodotto_UdM_ID').val();
    var prodotto_Coeff_Conversione = $('#Prodotto_Coeff_Conversione').val();
    var prodotto_Stima_Prod_Anno = $('#Prodotto_Stima_Prod_Anno').val();
    var prodotto_perc_Scarto = $('#Prodotto_perc_Scarto').val();
    var prodotto_Tariffa_Proposta = $('#Prodotto_Tariffa_Proposta').val();
    var prodotto_UdM_ID_Sec = $('#Prodotto_UdM_ID_SEC').val();

    //Ric#3
    var prodotto_utente_id = $('#ric_utente_id').val();
    //Ric#3
    var prodotto_flag_assegna_gruppo = $('input[name=Prodotto_flag_assegna_gruppo]').attr('checked') ? true : false;

    //Ric#3
    if (prodotto_utente_id == "" || prodotto_utente_id == null || prodotto_utente_id <= 0) {
        showAlertErrorGeneric("La valorizzazione deve avere un utente associato", "Errore!");
        return;
    }

    //Ric#10
    if (prodotto_UdM_ID_Sec == prodotto_UdM_ID & prodotto_Coeff_Conversione != 1 & prodotto_Coeff_Conversione != 0 & prodotto_Coeff_Conversione != null) {
        showAlertErrorGeneric("Se l'unità di misura di magazzino e l'unità di misura lotto produzione sono uguali il coefficiente di conversione deve essere 1 ", "Errore!");
        return;
    }

    //Ric#10
    if (prodotto_UdM_ID_Sec != prodotto_UdM_ID && (prodotto_Coeff_Conversione == 0 || prodotto_Coeff_Conversione == "" || prodotto_Coeff_Conversione == null)) {
        showAlertErrorGeneric("E' necessario inserire il coefficiente di conversione", "Errore!");
        return;
    }

    var checked = [];
    $("input[name='multicheckSelect']").each(function () {
        checked.push(parseInt($(this).val()));
    });

    var emp = {
        Prodotto_id: prodotto_id,
        Prodotto_Dim_Lotto: prodotto_Dim_Lotto,
        Prodotto_nr_Campioni: prodotto_nr_Campioni,
        Prodotto_Flag_Interno: getIsCheckedBool(prodotto_Flag_InternoObj),
        Prodotto_UdM_ID: prodotto_UdM_ID,
        Prodotto_Coeff_Conversione: prodotto_Coeff_Conversione,
        Prodotto_Codice_Desc: prodotto_Codice_Desc,
        Prodotto_Stima_Prod_Anno: prodotto_Stima_Prod_Anno,
        Prodotto_perc_Scarto: prodotto_perc_Scarto,
        Prodotto_Tariffa_Proposta: prodotto_Tariffa_Proposta,
        ProdottoPosIds: checked,
        Prodotto_UdM_ID_Sec: prodotto_UdM_ID_Sec,
        Prodotto_utente_id: prodotto_utente_id, //Ric#3
        Prodotto_flg_assegn_al_gruppo: prodotto_flag_assegna_gruppo //Ric#3
    };
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var urlInvio = '/Prodotto/InviaValidatoreProdotto';

    var conf = $.modal.confirm;
    //  conf.settings.confirmText = 'Si';
    conf(
            'Inviare il prodotto al validatore?',
            function () 
            {
                $.ajax({
                    type: "POST",
                    url: urlInvio,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) { location.href = "/Prodotto/ProdottoEdit/" + prodotto_id; }
                        else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
                    }
                });
             },
             function () { $.modal.alert('Annullato.'); }
    );
}
function CheckPosizioniProdotto()
{
    var prodotto_id = $('#prodotto_id').val();
    var checked = [];
    $("input[name='multicheckSelect']").each(function () {
        checked.push(parseInt($(this).val()));
    });

    var emp = { Prodotto_id: prodotto_id, ProdottoPosIds: checked };
    var urlRedirect = "/Prodotto/ProdottoEdit/" + prodotto_id;
    var urlInvio = '/Prodotto/CheckPosizioniProdotto';
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
function Ricarica(jsonDataChar, optionsChart) {

    var prodotto_idVal = $('#prodotto_id').val();
    $.getJSON
    (
        "/Prodotto/GetElencoChart",
            { prodotto_id: prodotto_idVal },
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
                    if (pos == 0) {
                        multiArray[pos][0] = itemData.Titolo;
                        multiArray[pos][1] = itemData.Val1;
                        multiArray[pos][2] = itemData.Val2;
                    }
                    else {
                        multiArray[pos][0] = itemData.Titolo;
                        multiArray[pos][1] = parseFloat(itemData.Val1);
                        multiArray[pos][2] = parseFloat(itemData.Val2);
                        if (Math.abs(maxDelta) < Math.abs(parseFloat(itemData.Val2) - parseFloat(itemData.Val1))) {
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
                if (faseRossa > 0) 
                {
                    res = (faseRossa - faseBlu) * 100 / faseRossa; 
                }
                res = roundNumber(res, 2);
                var strTot = "";
                strTot = res.toFixed(2).toString();
                strTot = strTot.replace('.', ',');
                //$('#Prodotto_ScostamentoFase').text(strTot + "%");
            }
    );
}