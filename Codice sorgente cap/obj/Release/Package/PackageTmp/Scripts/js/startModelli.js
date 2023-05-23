// Call template init (optional, but faster if called manually)
$.template.init();

// Table sort - DataTables
$('#sorting-Modelli').dataTable({
    "oLanguage": { "sUrl": "../../Scripts/js/libs/DataTables/dataTables.it-IT.txt" },
    'aoColumnDefs': [
				{ 'bSortable': false, 'aTargets': [] }
			],
    'sPaginationType': 'full_numbers',
    'sDom': '<"dataTables_header"lfr>t<"dataTables_footer"ip>',
    'bFilter': true,
    'bInfo': true
});


