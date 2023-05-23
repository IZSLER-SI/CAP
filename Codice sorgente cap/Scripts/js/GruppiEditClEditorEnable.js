// Call template init (optional, but faster if called manually)
$.template.init();


// CLEditor
var editorTextareaGruppi = $('#cleditorGruppi');
var editorWrapperGruppi = editorTextareaGruppi.parent();
var editorGruppi = editorTextareaGruppi.cleditor({width: 650,height: 250})[0];
// Update size when resizing
editorWrapperGruppi.sizechange
(   function () {
        editorGruppi.refresh();
});

    function saveGruppoEdit() {

        var Grurep_ID = $('#Grurep_ID').val();
        var txtCodice = $('#txtCodice').val();
        var txtDescrizione = $('#txtDescrizione').val();
        var cleditorGruppi = $('#cleditorGruppi').val();
        var txtCosto_ind = $('#txtCosto_ind').val();
       
        var txtCosto_Accettazione = $('#txtCosto_Accettazione').val();
        var emp = { Grurep_Codice: txtCodice,
                    Grurep_Desc: txtDescrizione,
                    Grurep_ID: Grurep_ID,
                    Grurep_DescEstesa: cleditorGruppi,
                    Grurep_Cost_Ind: txtCosto_ind,
                    Grurep_PrezzoUnit_Accettazione: txtCosto_Accettazione
                }
                if (txtCodice == null || txtCodice == "" || txtDescrizione == null || txtDescrizione == "") {
                    showAlertErrorGeneric('Valorizzare tutti i campi prima di procedere al salvataggio.', 'Attenzione');
                    return;
                }

                var urlSave = '/Settings/saveGruppoEdit';
                $.ajax({
                    type: "POST",
                    url: urlSave,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) {

                            location.href = "/Settings/GruppiEdit/" + retdata.id;
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
