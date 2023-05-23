// Call template init (optional, but faster if called manually)
$.template.init();
function saveSetting() {
    var set_id = $('#set_id').val();
    var set_Valore = $('#set_Valore').val();
    var set_Codice = $('#set_Codice').val();


    if (set_Codice == "" || set_Codice == null) {
        showAlertError("Inserire un codice setting", "Errore!");
        return;
    }
    if (set_Valore == "" || set_Valore == null) {
        showAlertError("Inserire un valore", "Errore!");
        return;
    }

    var emp = {
        Setting_ID: set_id,
        Setting_Codice: set_Codice,
        Setting_Valore: set_Valore
    };
    var urlSave = '/Settings/SaveSetting';
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
                                { modal.closeModal(); location.href = "/Settings/SettingsEdit/"  +set_id; }
                         }

                     }
                }
                );
                
            }
        }
    });
}
