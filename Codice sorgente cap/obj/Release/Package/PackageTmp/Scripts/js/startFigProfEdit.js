// Call template init (optional, but faster if called manually)
$.template.init();


function deleteFigProfEdit() {
    var FigProf_ID = $('#FigProf_ID').val();
    var emp = { FigProf_ID: FigProf_ID }
    var urlSave = '/Settings/deleteFigProfEdit';
    $.ajax({
        type: "POST",
        url: urlSave,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (retdata.ok) {

                location.href = "/Settings/FigProf"
            }
            else 
            {
                showAlertError(retdata.infopersonali, "Eliminazione impossibile.");
            }

        }
    });
}

function saveFigProfEdit() {

        var FigProf_ID = $('#FigProf_ID').val();
        var txtCodice = $('#txtCodice').val();
        var txtDescrizione = $('#txtDescrizione').val();
        var txtCosto_ind = $('#txtCosto_ind').val();
        var emp = { FigProf_Codice: txtCodice,
                    FigProf_Desc: txtDescrizione,
                    FigProf_ID: FigProf_ID,
                    FigProf_Cost: txtCosto_ind
                }
                if (txtCodice == null || txtCodice == "" || txtDescrizione == null || txtDescrizione == ""  ||
                    txtCosto_ind==null || txtCosto_ind=="") {
                    showAlertErrorGeneric('Valorizzare tutti i campi prima di procedere al salvataggio.', 'Attenzione');
                    return;
                }

                var urlSave = '/Settings/saveFigProfEdit';
                $.ajax({
                    type: "POST",
                    url: urlSave,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) {

                            location.href = "/Settings/FigProfEdit/" + retdata.id;
                        }
                        else {
                            showAlertError(retdata.infopersonali, "Codice presente");
                        }

                    }
                });
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


var mydialogTrkFigProf;
function openFigProfWorkFlow(urlDialog, FigProf_codice) {
        var timeout;
        mydialogTrkFigProf = $.modal({
            contentAlign: 'center',
            width: 900,
            height: 500,
            title: 'Workflow figura professionale - ' + FigProf_codice,
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

        mydialogTrkFigProf.loadModalContent(urlDialog, null);
    };