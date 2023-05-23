//$.template.init(); 
//try
//{
//    $('#dataDa').glDatePicker({ zIndex: 100 });
//    $('#dataA').glDatePicker({ zIndex: 100 });
//} catch (e) { }



function showAlertErrorGeneric(message, titleMsg) {
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

            //"31/12/2012"
function getDate(dtString) {
    var dateArray = dtString.split("/");
    var dtRet = null;
    if (dateArray.length == 3) {

        var day = dateArray[0];
        var month = dateArray[1]-1;
        var year = dateArray[2];

        dtRet = new Date(year, month, day, 0, 0, 0);

        if (year != dtRet.getFullYear()) {
            return null;
        }

        if (month != dtRet.getMonth()) {
            return null;
        }

        if (day != dtRet.getDate()) {
            return null;
        }
    }
    return dtRet
}
function dateOK(dataDaVal, dataAVal) {
    var ret = false;

    var dtDa = getDate(dataDaVal);
    var dtA = getDate(dataAVal);
    if (dtDa != null && dtA != null) 
    {
        if (dtDa < dtA)
        {ret = true;}
     }
   
    
    return ret;
}
function roundNumber(num, dec) {
    var result = Math.round(num * Math.pow(10, dec)) / Math.pow(10, dec);
    return result;
}
function addRowToTable(destTable, itemData) {

    if (itemData.Titolo != " ") 
    {
        var f1 = parseFloat(itemData.Val1);
        var f2 = parseFloat(itemData.Val2);
        var f3 = ((f1 - f2) / f1) * 100;
        var color = "green";
        var sign = "+";
        f3 = roundNumber(f3, 2);
        if (f3 == 0) { color = "blue"; sign = ""; }
        if (f3 < 0) { color = "red"; sign = "-"; }

        var f3Val = "";
        f3Val = f3.toFixed(2).toString();

        var newRow = $("<tr>" +
                        "<td class=\"align-center\">" + itemData.Titolo + "</td>" +
                        "<td class=\"align-center\">" + itemData.Val1 + "</td>" +
                        "<td class=\"align-center\">" + itemData.Val2 + "</td>" +
                        "<td class=\"align-center\"><small class=\"tag " + color + "-bg\">" + sign + f3Val + "%</small></td>" +
                   "</tr>");
        $("#destTable").append(newRow);
    }
}
function Pulisci() {
    $('#dataDa').val("");
    $('#dataA').val("");
    $("#destTable tbody tr").remove();

    var multiArray = new Array(2);
    multiArray[0] = new Array(3);
    multiArray[0][0] = 'Prezzo';
    multiArray[0][1] = 'Deliberato';
    multiArray[0][2] = 'Attualizzato';
    multiArray[1] = new Array(3);
    multiArray[1][0] = ' ';
    multiArray[1][1] = 0;
    multiArray[1][2] = 0;

    jsonDataChar = multiArray;
    var ldata = google.visualization.arrayToDataTable(jsonDataChar);
    var chart = new google.visualization.AreaChart(document.getElementById('demo-chart'));
    chart.draw(ldata, optionsChart);
}
function Ricarica() 
{
    var idVal = $('#idCurr').val();
    var modeVal = $('#mode').val();
    var dataDaVal = $('#dataDa').val();
    var dataAVal = $('#dataA').val();

    if (( dataDaVal != "" && dataAVal != "" ) && dateOK(dataDaVal,dataAVal) ) {

        $.getJSON
            (
                "/Report/GetElencoChart",
                     { id: idVal,
                         mode: modeVal,
                         dataDa: dataDaVal,
                         dataA: dataAVal
                     },
                    function (data) {
                        var len = data.length;
                        var multiArray = new Array(len);
                        var pos = 0;
                        var destTable = $("#destTable");
                        $("#destTable tbody tr").remove(); 
                        $.each(data, function (index, itemData) {
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
                                addRowToTable(destTable, itemData);

                            }
                            pos = pos + 1;
                        });

                        jsonDataChar = multiArray;
                        var ldata = google.visualization.arrayToDataTable(jsonDataChar);
                        var chart = new google.visualization.AreaChart(document.getElementById('demo-chart'));
                        chart.draw(ldata, optionsChart);
                    }
            );
    }
    else 
    {
        showAlertErrorGeneric('Selezionare un intervallo temporale valido.', 'Attenzione');
    }

}
