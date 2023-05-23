$.template.init();

function CreateFile(tipo) {

    var urlCreate = "/Report/CreateFile"
    var emp = { Tipologia: tipo };

    $.ajax({
        type: "POST",
        url: urlCreate,
        data: JSON.stringify(emp),
        datatype: "JSON",
        contentType: "application/json; charset=utf-8",
        success: function (retdata) {
            if (!retdata.ok) {
                showAlertErrorGeneric(retdata.infopersonali, 'Attenzione');
                             }
            else {
                    $.modal.alert("File creato",
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
                                 { modal.closeModal(); location.href = "/Report/Index/"; }
                             }
                        }
                   }
                   );
                }
            } 
      });
}