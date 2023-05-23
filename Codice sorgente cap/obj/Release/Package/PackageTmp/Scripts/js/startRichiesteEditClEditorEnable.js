// Call template init (optional, but faster if called manually)
$.template.init();


// CLEditor
var editorTextarea = $('#cleditor');
var editorWrapper = editorTextarea.parent();
var editor = editorTextarea.cleditor({width: 650,height: 250})[0];
// Update size when resizing
editorWrapper.sizechange
(   function () {
        editor.refresh();
});

