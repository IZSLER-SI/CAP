// Call template init (optional, but faster if called manually)
$.template.init();

// Favicon count
//Tinycon.setBubble(2);

// If the browser support the Notification API, ask user for permission (with a little delay)
if (notify.hasNotificationAPI() && !notify.isNotificationPermissionSet()) {
   /* setTimeout(function () {
        notify.showNotificationPermission('Your browser supports desktop notification, click here to enable them.', function () {
            // Confirmation message
            if (notify.hasNotificationPermission()) {
                notify('Notifications API enabled!', 'You can now see notifications even when the application is in background', {
                    icon: '../../Content/img/demo/icon.png',
                    system: true
                });
            }
            else {
                notify('Notifications API disabled!', 'Desktop notifications will not be used.', {
                    icon: '../../Content/img/demo/icon.png'
                });
            }
        });

    }, 2000);*/
}

/*
* Handling of 'other actions' menu
*/

var otherActions = $('#otherActions'),
			current = false;

// Other actions
$('.list .button-group a:nth-child(2)').menuTooltip(otherActions, {

    classes: ['with-mid-padding'],

    onShow: function (target) {
        // Remove auto-hide class
        target.parent().removeClass('show-on-parent-hover');
    },

    onRemove: function (target) {
        // Restore auto-hide class
        target.parent().addClass('show-on-parent-hover');
    }
});

// Delete button
$('.list .button-group a:last-child').data('confirm-options', {

    onShow: function () {
        // Remove auto-hide class
        $(this).parent().removeClass('show-on-parent-hover');
    },

    onConfirm: function () {
        // Remove element
        $(this).closest('li').fadeAndRemove();

        // Prevent default link behavior
        return false;
    },

    onRemove: function () {
        // Restore auto-hide class
        $(this).parent().addClass('show-on-parent-hover');
    }

});

// Demo modal
function openModal() {
    $.modal({
        content: '<p>This is an example of modal window. You can open several at the same time (click links below!), move them and resize them.</p>' +
						  '<p>The plugin provides several other functions to control them, try below:</p>' +
						  '<ul class="simple-list with-icon">' +
						  '    <li><a href="javascript:void(0)" onclick="openModal()">Open new blocking modal</a></li>' +
						  '    <li><a href="javascript:void(0)" onclick="$.modal.alert(\'This is a non-blocking modal, you can switch between me and the other modal\', { blocker: false })">Open non-blocking modal</a></li>' +
						  '    <li><a href="javascript:void(0)" onclick="$(this).getModalWindow().setModalTitle(\'\')">Remove title</a></li>' +
						  '    <li><a href="javascript:void(0)" onclick="$(this).getModalWindow().setModalTitle(\'New title\')">Change title</a></li>' +
						  '    <li><a href="javascript:void(0)" onclick="$(this).getModalWindow().loadModalContent(\'ajax-demo/auto-setup.html\')">Load Ajax content</a></li>' +
						  '</ul>',
        title: 'Example modal window',
        width: 300,
        scrolling: false,
        actions: {
            'Chiudi': {
                color: 'red',
                click: function (win) { win.closeModal(); }
            },
            'Center': {
                color: 'green',
                click: function (win) { win.centerModal(true); }
            },
            'Refresh': {
                color: 'blue',
                click: function (win) { win.closeModal(); }
            },
            'Abort': {
                color: 'orange',
                click: function (win) { win.closeModal(); }
            }
        },
        buttons: {
            'Chiudi': {
                classes: 'huge blue-gradient glossy full-width',
                click: function (win) { win.closeModal(); }
            }
        },
        buttonsLowPadding: true
    });
};

// Demo alert
function openAlert() {
    $.modal.alert('This is an alert message', {
        buttons: {
            'Thanks, captain obvious': {
                classes: 'huge blue-gradient glossy full-width',
                click: function (win) { win.closeModal(); }
            }
        }
    });
};

// Demo prompt
function openPrompt() {
    var cancelled = false;

    $.modal.prompt('Please enter a value between 5 and 10:', function (value) {
        value = parseInt(value);
        if (isNaN(value) || value < 5 || value > 10) {
            $(this).getModalContentBlock().message('Please enter a correct value', { append: false, classes: ['red-gradient'] });
            return false;
        }

        $.modal.alert('Value: <strong>' + value + '</strong>');

    }, function () {
        if (!cancelled) {
            $.modal.alert('Oh, come on....');
            cancelled = true;
            return false;
        }
    });
};

// Demo confirm
function openConfirm() {
    $.modal.confirm('Challenge accepted?', function () {
        $.modal.alert('Me gusta!');

    }, function () {
        $.modal.alert('Meh.');
    });
};



/*
* Agenda scrolling
* This example shows how to remotely control an agenda. most of the time, the built-in controls
* using headers work just fine
*/

// Days
var daysName = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],

// Name display
			agendaDay = $('#agenda-day'),

// Agenda scrolling
			agenda = $('#agenda').scrollAgenda({
			    first: 2,
			    onRangeChange: function (start, end) {
			        if (start != end) {
			            agendaDay.text(daysName[start].substr(0, 3) + ' - ' + daysName[end].substr(0, 3));
			        }
			        else {
			            agendaDay.text(daysName[start]);
			        }
			    }
			});

// Remote controls
$('#agenda-previous').click(function (event) {
    event.preventDefault();
    agenda.scrollAgendaToPrevious();
});
$('#agenda-today').click(function (event) {
    event.preventDefault();
    agenda.scrollAgendaFirstColumn(2);
});
$('#agenda-next').click(function (event) {
    event.preventDefault();
    agenda.scrollAgendaToNext();
});

// Demo loading modal
function openLoadingModal() {
    var timeout;

    $.modal({
        contentAlign: 'center',
        width: 240,
        title: 'Loading',
        content: '<div style="line-height: 25px; padding: 0 0 10px"><span id="modal-status">Contacting server...</span><br><span id="modal-progress">0%</span></div>',
        buttons: {},
        scrolling: false,
        actions: {
            'Cancel': {
                color: 'red',
                click: function (win) { win.closeModal(); }
            }
        },
        onOpen: function () {
            // Progress bar
            var progress = $('#modal-progress').progress(100, {
                size: 200,
                style: 'large',
                barClasses: ['anthracite-gradient', 'glossy'],
                stripes: true,
                darkStripes: false,
                showValue: false
            }),

            // Loading state
						loaded = 0,

            // Window
						win = $(this),

            // Status text
						status = $('#modal-status'),

            // Function to simulate loading
						simulateLoading = function () {
						    ++loaded;
						    progress.setProgressValue(loaded + '%', true);
						    if (loaded === 100) {
						        progress.hideProgressStripes().changeProgressBarColor('green-gradient');
						        status.text('Done!');
						        /*win.getModalContentBlock().message('Content loaded!', {
						        classes: ['green-gradient', 'align-center'],
						        arrow: 'bottom'
						        });*/
						        setTimeout(function () { win.closeModal(); }, 1500);
						    }
						    else {
						        if (loaded === 1) {
						            status.text('Loading data...');
						            progress.changeProgressBarColor('blue-gradient');
						        }
						        else if (loaded === 25) {
						            status.text('Loading assets (1/3)...');
						        }
						        else if (loaded === 45) {
						            status.text('Loading assets (2/3)...');
						        }
						        else if (loaded === 85) {
						            status.text('Loading assets (3/3)...');
						        }
						        else if (loaded === 92) {
						            status.text('Initializing...');
						        }
						        timeout = setTimeout(simulateLoading, 50);
						    }
						};

            // Start
            timeout = setTimeout(simulateLoading, 2000);
        },
        onClose: function () {
            // Stop simulated loading if needed
            clearTimeout(timeout);
        }
    });
};



// Demo loading modal
function openLoadingInformazioni() {
    var timeout;

    $.modal({
        contentAlign: 'center',
        width: 240,
        title: 'Caricamento',
        content: '<div style="line-height: 25px; padding: 0 0 10px"><span id="modal-status">Connessione al server...</span><br><span id="modal-progress">0%</span></div>',
        buttons: {},
        scrolling: false,
        actions: {
            'Annulla': {
                color: 'red',
                click: function (win) { win.closeModal(); }
            }
        },
        onOpen: function () {
            // Progress bar
            var progress = $('#modal-progress').progress(100, {
                size: 200,
                style: 'large',
                barClasses: ['anthracite-gradient', 'glossy'],
                stripes: true,
                darkStripes: false,
                showValue: false
            }),

            // Loading state
						loaded = 0,

            // Window
						win = $(this),

            // Status text
						status = $('#modal-status'),

            // Function to simulate loading
						simulateLoading = function () {
						    ++loaded;
						    progress.setProgressValue(loaded + '%', true);
						    if (loaded === 100) {
						        progress.hideProgressStripes().changeProgressBarColor('green-gradient');
						        status.text('Fatto!');
						        /*win.getModalContentBlock().message('Content loaded!', {
						        classes: ['green-gradient', 'align-center'],
						        arrow: 'bottom'
						        });*/
						        setTimeout(
                                            function () {
                                                win.closeModal();
                                                window.location.reload(true);
                                            }
                                            , 500);
						    }
						    else {
						        if (loaded === 1) {
						            status.text('Caricamento Dati...');
						            progress.changeProgressBarColor('blue-gradient');
						        }
						        else if (loaded === 25) {
						            status.text('Caricamento al 25%...');
						        }
						        else if (loaded === 45) {
						            status.text('Caricamento al 50%...');
						        }
						        else if (loaded === 70) {
						            status.text('Caricamento al 75%...');
						        }
						        else if (loaded === 85) {
						            status.text('Caricamento al 95%...');
						        }
						        else if (loaded === 92) {
						            status.text('Inizializzazione...');
						        }
						        timeout = setTimeout(simulateLoading, 20);
						    }
						};

            // Start
            timeout = setTimeout(simulateLoading, 400);
        },
        onClose: function () {
            // Stop simulated loading if needed
            clearTimeout(timeout);
        }
    });
}

function getBody() {
    var a = document.getElementsByTagName("body")[0];

    var info = document.createElement('script')
    info.setAttribute("type", "text/javascript")
    info.text = 'alert(1);';
    a.appendChild(info);
    var res = a.outerHTML;
    var i = 0;
    i = i + 1;
}
function showAlertMessageGeneric(message, titleMsg) {
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
                    classes: 'blue-gradient glossy big full-width',
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
                    classes: 'blue-gradient glossy big full-width',
                    click: function (modal) { modal.closeModal(); }
                }
            },
            buttonsAlign: 'center',
            buttonsLowPadding: true
        }
       );
    }
}
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
function apriPopUp(url,titolo,stringaParametri) 
{ 
// 'scrollbars=no,resizable=yes, width=200,height=200,status=no,location=no,toolbar=no’
    newin = window.open(url, titolo, stringaParametri);
}


//Ric#8
function deleteRichiestaList(rc_id, ric_titolo, richiedente_utente_id, destinatario_utente_id, ric_testo, ric_stato, t_ricpri_id) {

    var id = rc_id;
    var titolo = ric_titolo;
    var richiedente_utente_id = richiedente_utente_id;
    var destinatario_utente_id = destinatario_utente_id;
    var testo = ric_testo;
    var staric_desc = ric_stato;
    var t_ricpri_id = t_ricpri_id;
    var info = null;

    var emp = {
        Richie_id: id,
        Richie_titolo: titolo,
        Richie_richiedente_utente_id: richiedente_utente_id,
        Richie_destinatario_utente_id: destinatario_utente_id,
        Richie_testo: testo,
        Richie_t_ricpri_id: t_ricpri_id,
        T_staric_desc: null,
        PaginaOrigine: info
    };
    var urlDelete = '/Home/DeleteRichiesta';
    var conf = $.modal.confirm;
    //  conf.settings.confirmText = 'Si';
    conf('Sicuro di voler procedere?',
            function () {
                $.ajax({
                    type: "POST",
                    url: urlDelete,
                    data: JSON.stringify(emp),
                    datatype: "JSON",
                    contentType: "application/json; charset=utf-8",
                    success: function (retdata) {
                        if (retdata.ok) { location.href = "/Home/Index/" }
                        else { showAlertErrorGeneric(retdata.infopersonali, 'Attenzione'); }
                    }
                });
            },
            function () { $.modal.alert('Annullato.'); }
         );
}