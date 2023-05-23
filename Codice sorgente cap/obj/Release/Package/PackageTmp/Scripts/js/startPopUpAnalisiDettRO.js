$.template.init();

var mydialogAnalisiDettRO ;
function openAnalisiDettRO(urlDialog) {
    var timeout;
    mydialogAnalisiDettRO = $.modal({
        contentAlign: 'center',
        width: 1000,
        height: 610,
        title: 'Dettaglio',
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

    mydialogAnalisiDettRO.loadModalContent(urlDialog, null);
};


