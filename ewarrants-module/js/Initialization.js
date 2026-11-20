var _viewer;
var _thumbs;
var _scanPage = 1;
var _serverUrl = 'Handlers/WebDocumentViewerHandler.ashx';
var _docUrl = '';
var _savePath = '';
var _thumbsShowing = true;
var _initialViewerWidth;
var _nothumbsViewerWidth;
var _testing;
var _customTextDialog;
var _redirect;
var _signiturePath;
var _initialPath;
var _saveObject;
var _warrantId;
var _moduleId;
var _userId;
var _action;
$(function () {
    try {
        _docUrl = $("#hdFilePath").val();
        _rejectReason = $("#txtReason").val();
        _warrantId = $("#hdWarrantId").val();
        _moduleId = $("#hdModuleId").val();
        _userId = $("#hdUserId").val();
        _savePath = _docUrl.substring(0, _docUrl.lastIndexOf("/") + 1) + "burned/";
        _signiturePath = $("#hdSignature").val();
        _initialPath = $("#hdInitial").val();
        InitializeViewers();
        window.onresize = function () {
            $(".atala-document-viewer").height($(".inner-viewer").height());
            $('.atala-document-thumbs').height($(".inner-viewer").height());
            $('.clickmeleft').height($(".inner-viewer").height());
            $('.clickmeright').height($(".inner-viewer").height());
        };
        $("#dateImage").on("click", function () {
            $('#dateInput').datepicker('show');
        });
        $("#deleteAnnotaion").insertAfter('.atala_tool_button_text_anno');
        $("#saveTextAnnotation").insertAfter('.atala_tool_button_text_anno');
        $("#insertTextAnnotation").insertAfter('.atala_tool_button_text_anno');
        $("#insertDateText").insertAfter('.atala_tool_button_fit_width');
        $("#checkMark").insertAfter('.atala_tool_button_fit_width');
        $("#crossMark").insertAfter('.atala_tool_button_fit_width');
        $("#signatureAnnotation").insertAfter('.atala_tool_button_rect_anno');
        $("#initialAnnotation").insertAfter('.atala_tool_button_rect_anno');

        SetupClickBar();
        GetCustomTextList();
       
    } //End Try
    catch (error) {
        console.log(error);
        AppendStatus(error);
    }
    $('#customTextList').on('click', '.listItem', function () {
        var selectedText = $(this).text();
        if (selectedText != "") {
            CreateCustomTextAnnotation(selectedText);
        }
        $("#dialog_customText").dialog("close");
    });
    $('#saveTextAnnotation').on('click', function (e) {
        e.preventDefault();
        GetTextFromAnnotation();
    });
    _customTextDialog = $("#dialog_customText").dialog({
        autoOpen: false,
        title: "Custom Text List",
        height: 400,
        width: 600,
        modal: true,
        buttons: {
            Cancel: function () {
                _customTextDialog.dialog("close");
            }
        },
        close: function () {
        }
    });
    $("#insertTextAnnotation").on("click", function (e) {
        e.preventDefault();
        _customTextDialog.dialog("open");
        return false;
    });
    $("#deleteAnnotaion").on("click", function (e) {
        e.preventDefault();
        DeleteSelectedAnnotations();
        return false;
    });
    $("#signatureAnnotation").on("click", function (e) {
        e.preventDefault();
        InsertImageAnnotation('Signature', _signiturePath, $("#hdSignatureWidth").val(), $("#hdSignatureHeight").val(), 180);
        return false;
    });
    $("#initialAnnotation").on("click", function (e) {
        e.preventDefault();
        InsertImageAnnotation('Initial', _initialPath, $("#hdInitialWidth").val(), $("#hdInitialHeight").val(), 80);
        return false;
    });
    $("#insertDateText").on("click", function (e) {
        e.preventDefault();
        var dateAnnotationConfig = {
            'type': 'stamp',
            'name': 'Current Date',
            'fill': {
                'color': 'white',
                'opacity': 0.00
            },
            'outline': {
                'color': 'white',
                'opacity': 0.00,
                'width': 15
            },
            'text': {
                'value': moment().format('MM/DD/YYYY'),
                'align': 'center',
                'font': {
                    'bold': false,
                    'color': '#000',
                    'family': 'Arial',
                    'size': 30,
                }
            }
        };
        _thumbs.annotations.drawAnnotation(dateAnnotationConfig);
        return false;
    });
    $("#checkMark").on("click", function (e) {
        e.preventDefault();
        var checkAnnotationConfig = {
            'type': 'stamp',
            'name': 'Check Mark',
            'fill': {
                'color': 'white',
                'opacity': 0.00
            },
            'outline': {
                'color': 'white',
                'opacity': 0.00,
                'width': 15
            },
            'text': {
                'value': '✓',
                'align': 'center',
                'font': {
                    'bold': false,
                    'color': '#000',
                    'family': 'Arial',
                    'size': 30
                }
            }
        };
        _thumbs.annotations.drawAnnotation(checkAnnotationConfig);
        return false;
    });
    $("#crossMark").on("click", function (e) {
        e.preventDefault();
        var crossAnnotationConfig = {
            'type': 'stamp',
            'name': "Cross Out Mark",
            'fill': {
                'color': 'white',
                'opacity': 0.00,
            },
            'outline': {
                'color': 'white',
                'opacity': 0.00,
                'width': 15
            },
            'text': {
                'value': '✗',
                'align': 'center',
                'font': {
                    'bold': false,
                    'color': '#000',
                    'family': 'Arial',
                    'size': 30
                }
            }
        };
        _thumbs.annotations.drawAnnotation(crossAnnotationConfig);
        return false;
    });
    $(".main-viewer").on("keyup", function (e) {
        if (e.key == "Delete" || e.code == "Delete") {
            e.preventDefault();
            DeleteSelectedAnnotations();
        }
    });
    $("#cmdReplaceText").on("click", function (e) {
        var replacementText = $("#ReplacementText").val();
        replaceTextInAnnotation(replacementText);
    });
});
function InitializeViewers() {
    _viewer = new Atalasoft.Controls.WebDocumentViewer({
        parent: $('#_containerViewer'),
        toolbarparent: $('#_toolbar'),
        serverurl: _serverUrl,
        allowannotations: true,
        showbuttontext: false,
        forcepagefit: true,
        tiling: true,
        savepath: _savePath,
        savefileformat: 'tif'
    });

    _thumbs = new Atalasoft.Controls.WebDocumentThumbnailer({
        parent: $('#_containerThumbs'),
        serverurl: _serverUrl,			// server handler url to send image requests to
        documenturl: _docUrl, // + _docFile, 	// document url relative to the server handler url
        allowannotations: true,
        viewer: _viewer,
        maxwidth: 180,
        minwidth: 180
    });

    _viewer.bind({
        'error': onError,
        'documentloaded': onDocumentLoaded,
        'documentsaved': onDocumentSaved,
    });

    $('body').bind('beforeunload', function () {
        _viewer.zoom(1);
        _thumbs.zoom(1);
    });


    // This is where we set up the defaults for the annotations we want to draw on click
    _viewer.annotations.setDefaults([
        {
            type: 'line',
            outline: { color: '#000', opacity: 0.80, width: 2, endcap: { width: 'wide', height: 'long', style: 'block' } }
        },
        {
            type: 'highlight',
            outline: { color: '#000', opacity: 1 },
            fill: { color: '#000', opacity: 1 }
        },
        {
            type: 'freehand',
            outline: { color: '#000', opacity: 0.80, width: 2 }
        },
        {
            type: 'text',
            text: { value: 'Double-click to change text', align: 'left', font: { color: '#000', family: 'Times New Roman', size: 14 } },
            outline: { color: '#fff', opacity: 0.00, width: 1 },
            fill: { color: '#fff', opacity: 1 }
        },
        {
            type: 'rectangle',
            fill: { color: '#fff', opacity: 1 }
        }]);

    $('.atala-ui-icon-save').parent().css('display', 'none');
    $('.atala-ui-icon-ellipse').parent().css('display', 'none');
    $('.atala-ui-icon-polygon').parent().css('display', 'none');
    $('.atala-ui-icon-rotate-right').parent().css('display', 'none');
    $('.atala-ui-icon-rotate-left').parent().css('display', 'none');
    $('.atala-ui-icon-lines').parent().css('display', 'none');

    //Hide entries on the context menu when not appropriate.
    _viewer.bind('contextmenu', function (event, anno, menu) {
        if (anno.type === 'stamp')
            delete menu['Properties'];
    });
    SetViewerWidth();
}
function SetViewerWidth() {
    var totalWidth = parseInt($('.main-viewer').css('width'), 10);
    var thumbsWidth = parseInt($(".atala-document-thumbs").css('width'), 10);
    var clickWidth = parseInt($(".clickmeleft").css('width'), 10);
    _initialViewerWidth = totalWidth - (thumbsWidth + clickWidth + 3);
    _nothumbsViewerWidth = totalWidth - clickWidth - 2;
    $('.atala-document-viewer').css('width', _initialViewerWidth);
    $('#_containerThumbs').height($(".inner-viewer").height());
}
function SaveFile() {
    try {
        attachDateToSignatures();
        burnAllAnnos();
        _viewer.save('', 'tif', { id: _warrantId }, "");
    } catch (e) {
        HideLoadingGif();
        onError(e);
        return true;
    }
    return false;
}
function RejectFile() {
    try {
        burnAllAnnos();
        _viewer.save('', 'tif', { id: _warrantId }, "");
    } catch (e) {
        HideLoadingGif();
        onError(e);
        return true;
    }
    return false;
}
function FileSaved() {
    setTimeout(function () { ClosePage() }, 2000);
}
function ApproveWarrant() {
    _action = "approve";
    ShowLoadingGif();
    disableButtons();
    SaveFile();
}
function RejectWarrant() {
    _action = "reject";
    ShowLoadingGif();
    disableButtons();
    SaveFile();
}
function onDocumentSaved(e) {
    if (e.success) {
        var request = $.ajax({
            url: 'Handlers/ProcessingHandler.ashx',
            data: {
                action: _action,
                rejectReason: _rejectReason,
                document: e.fileName,
                warrantid: _warrantId,
                userid: _userId,
                moduleid: _moduleId,
                savepath: _savePath,
                temppath: _docUrl,
            },
            dataType: 'json',
            success: function (data, status) {
                if (data.success != 'true') {
                    ShowDialogBox("Unable to Process Warrant", "<span class=\"ui-icon ui-icon-alert\"></span> " + data.error, "Ok", "", "", false);
                }
                else {
                    FileSaved();
                }
            }, error: function (xhr, status, error) {
                ShowDialogBox("Unexpected Error", "<span class=\"ui-icon ui-icon-alert\"></span> " + error, "Ok", "", "", false);
            }
        });
    }
}
function onDocumentLoaded(e) {

}
function onError(e) {
    ShowDialogBox('Error Processing Request', '<p class="text-danger"><span class=\"ui-icon ui-icon-alert\"></span> Error: ' + e.name + '\n' + e.message + "</p>", "Ok", "", "", false);
}

// MUST return false to avoid a postback
function ClosePage() {
    HideLoadingGif();
    location.replace("/");
}
function attachDateToSignatures() {
    var pageCount = _viewer.getDocumentInfo().count;
    if (pageCount != null && pageCount > 0) {
        for (var i = 0; i < pageCount; i++) {
            attachDateToSignaturesOnPage(i);
        }
    }
}
function attachDateToSignaturesOnPage(pageIndex) {
    var annos = _viewer.getAnnotationsFromPage(pageIndex);
    if (annos != null) {
        for (var i = 0; i < annos.length; i++) {
            var anno = annos[i];
            if (anno.name == "Signature")
                AddDateToSignatures(pageIndex, anno.x, anno.y, anno.width, anno.height);
        }
    }
}
function AddDateToSignatures(index, x, y, w, h) {
    var datetime = moment().format('MM/DD/YYYY hh:mm:ss a');
    var dateAnnotationConfig = {
        'type': 'stamp',
        'name': 'Current Date',
        'x': x,
        'y': y,
        'width': w,
        'height': h,
        'fill': {
            'color': 'white',
            'opacity': 0.00
        },
        'outline': {
            'color': 'white',
            'opacity': 0.00,
            'width': 1
        },
        'text': {
            'value': datetime,
            'align': 'center',
            'font': {
                'bold': false,
                'color': 'rgba(204,204,204,0.5)',
                'family': 'Arial',
                'size': 30
            },
            'opacity': 0.50,
            'autoscale': true,
        }
    };
    _viewer.annotations.createOnPage(dateAnnotationConfig, index);
    return false;
}

// Sets properties needed to Burn all annotations on a single page
function burnAllAnnosOnPage(pageIndex) {
    var annos = _viewer.getAnnotationsFromPage(pageIndex);
    if (annos != null) {
        for (var i = 0; i < annos.length; i++) {
            var anno = annos[i];
            var annotionaName = "anno" + i;
            anno.burn = true;
            anno.update();
        }
    }
}
// Sets properties needed to burn all annos on all pages
function burnAllAnnos() {
    var docInfo = _viewer.getDocumentInfo();
    var pageCount = docInfo.count;
    if (pageCount != null && pageCount > 0) {
        for (var i = 0; i < pageCount; i++) {
            InsertDocumentId(i, docInfo);
            burnAllAnnosOnPage(i);
        }
    }
}

// This is needed because the insertPages call needs an array of indices
// so we pass in a number of pages to generate the needed src indicies array
//function makeSrcArray(count) {
//    var returnArray = new Array();
//    for (var i = 0; i < count; i++) {
//        returnArray.push(i);
//    }
//    return returnArray;
//}
function clearViewers() {
    _thumbs.OpenUrl('', '');
    // CRITICAL - if you do not return false it causes a postback!
    return false;
}
function SetupClickBar() {
    var clickBar = $(".clickmeleft");
    clickBar.click(function (event) {

        var mainThumbs = $(".atala-document-thumbs");

        if (_thumbsShowing) {
            mainThumbs.animate({ width: '0px' }, 500, function () {
                mainThumbs.hide();
                $(".atala-document-viewer").animate({ width: _nothumbsViewerWidth }, 250, function () { });
            });
            _thumbsShowing = false;
            clickBar.removeClass("clickmeleft");
            clickBar.removeClass("clickmehoverleft");
            clickBar.addClass("clickmeright");
        }
        else {
            mainThumbs.show();
            mainThumbs.animate({ width: _nothumbsViewerWidth - _initialViewerWidth + 'px' }, 500);
            _thumbsShowing = true;
            $(".atala-document-viewer").css("width", _initialViewerWidth);
            clickBar.removeClass("clickmeright");
            clickBar.removeClass("clickmehoverright");
            clickBar.addClass("clickmeleft");
        }
    });
    clickBar.hover(
        function () {
            if (_thumbsShowing)
                clickBar.addClass("clickmehoverleft");
            else
                clickBar.addClass("clickmehoverright");
        },
        function () {
            if (_thumbsShowing)
                clickBar.removeClass("clickmehoverleft");
            else
                clickBar.removeClass("clickmehoverright");
        }
    );
}
function ShowLoadingGif() {
    var viewer = $('.atala-document-viewer');
    var pos = viewer.position();
    var w = viewer.width();
    var h = viewer.height();
    var dim = $('.dimwrapper');
    dim.show();
    dim.css("left", (pos.left - 7));
    dim.css("top", (pos.top + 1));
    dim.css("height", h);
    dim.css("width", w);
    dim.css("background-color", "#424242");
    dim.css("opacity", 0.75);
    var gif = $("#LoadingGif");
    gif.show();
    gif.css("left", ((pos.left + w / 2) - 55));
    gif.css("top", ((pos.top + h / 2) - 55));
    gif.css("height", h);
    gif.css("width", w);
}
function HideLoadingGif() {
    var gif = $("#LoadingGif");
    gif.hide();
    var dim = $('.dimwrapper');
    dim.hide();
}
function AppendStatus(error) {
    console.log(error);
    ShowDialogBox("Unexpected Error", "<p><span class=\"ui-icon ui-icon-alert\"></span> The following unexpected error has occurred: " + error + "</p>", "Ok", "", "", false);
}
function s4() {
    return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
};
function guid() {
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
}
function ShowDialogBox(title, content, btn1text, btn2text, functionText, redirect) {
    var btn1css;
    var btn2css;
    _redirect = redirect;
    if (btn1text == '') {
        btn1css = "hidecss";
    } else {
        btn1css = "showcss";
    }
    $("#dialog-text").html(content);

    $("#dialog").dialog({
        resizable: false,
        title: title,
        modal: true,
        width: '500px',
        height: 'auto',
        bgiframe: false,
        hide: { effect: 'scale', duration: 400 },
        close: CloseDialog,
        buttons: [
            {
                text: btn1text,
                "class": btn1css,
                click: function () {
                    HideLoadingGif();
                    CloseDialog();
                }
            }
        ]
    });
}
function CloseDialog() {
    $("#dialog").dialog("close");

    if (_redirect) {
        location.replace(document.referrer);
    }
}
function disableButtons() {
    $('.btn').prop('disabled', true);
    setTimeout(function () {
        $('.btn').prop('disabled', false);
    }, 5 * 1000);
}
function GetCustomTextList() {
    var Url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/GetAnnotations/";
    var annotationList = $('#customTextList');
    var choices = '';
    annotationList.empty();
    annotationList.append('<li><span class="listItem">Per Schedule</span></li>');
    $.ajax({
        type: "GET",
        dataType: "json",
        url: Url,
        success: function (j) {
            if (j.length > 0) {
                for (var i = 0; i < j.length; i++) {
                    c = j[i];
                    choices += '<li><span class="listItem">' + c.Text + '</span><a class="deletebutton" href="#" onclick="DeleteCustomTextFromList(' + c.Value + '); return false;"><span class="ui-icon ui-icon-trash text-danger" title="Delete Custom Text Item"></span></a></li>';
                }
                annotationList.append(choices);
            }
        },
        error: function () {
            ShowDialogBox("Warning", "<span class=\"ui-icon ui-icon-alert\"></span> Error Retrieving Annotations.", "Ok", "", "", false);
        }
    });
}
// Add annotation text to Custom List
function AddCustomTextToList(text) {
    var Url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/AddAnnotation/";
    $.ajax({
        type: "POST",
        cache: "false",
        url: Url,
        headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
        data: ({ AnnotationText: text }),
        success: function () {
            GetCustomTextList();
            ShowDialogBox("Notice", "<p><span class=\"ui-icon ui-icon-info\"></span>The selected text has been added to your list</p>", "Ok", "", "", false);
        },
        error: function () {
            ShowDialogBox("Warning", "<p><span class=\"ui-icon ui-icon-alert\"></span> Error Adding Custom Text.</p>", "Ok", "", "", false);
        }
    });
    return false;
}
// Delete annotation text from Custom List
function DeleteCustomTextFromList(id) {
    var deleteConfirmed = confirm('Delete Selected Text?\n\nWARNING: This operation cannot be reversed.');
    if (deleteConfirmed) {
        var Url = "/DesktopModules/tjc.Modules/eWarrants/api/HttpData/DeleteAnnotation/";
        $.ajax({
            type: "POST",
            cache: "false",
            url: Url,
            headers: { "RequestVerificationToken": $('input[name="__RequestVerificationToken"]').val() },
            data: ({ AnnotationId: id }),
            success: function () {
                GetCustomTextList();
            },
            error: function () {
                ShowDialogBox("Warning", "<span class=\"ui-icon ui-icon-alert\"></span>Error Adding Custom Text.", "Ok", "", "", false);
            }
        });
    }
    return false;
}
// Update the text in the annotaion
function UpdateSelectedTextAnnotation(replacementText) {
    var annos = _viewer.annotations.getSelected();
    if (annos != null) {
        for (var i = 0; i < annos.length; i++) {
            var annoArray = annos[i];
            var anno = annoArray[0];
            if (anno != null && anno.type === "text") {
                anno.text.value = replacementText;
                anno.update();
            } else {
                ShowDialogBox("No Text Box Selected", "<span class=\"ui-icon ui-icon-info\"></span>Please select a textbox and try again.", "Ok", "", "", false);
            }
        }
        ShowDialogBox("No Text Box Selected", "<span class=\"ui-icon ui-icon-info\"></span>Please select a textbox and try again.", "Ok", "", "", false);
    }
}
// GetText From Selected Annotation
function GetTextFromAnnotation() {
    var annos = _viewer.annotations.getSelected();
    var selectedText = "";
    if (annos != null) {
        for (var i = 0; i < annos.length; i++) {
            var annoArray = annos[i];
            var anno = annoArray[0];
            if (anno != null && anno.type === "text") {
                selectedText = anno.text.value;
                AddCustomTextToList(selectedText);
            } else {
                ShowDialogBox("No Text Box Selected", "<span class=\"ui-icon ui-icon-info\"></span> Please select a textbox and try again.", "Ok", "", "", false);
            }
        }
        ShowDialogBox("No Text Box Selected", "<span class=\"ui-icon ui-icon-info\"></span> Please select a textbox and try again.", "Ok", "", "", false);
    }
}
function DeleteSelectedAnnotations() {
    if (confirm("Delete Selected Annotation(s)?")) {
        var pageIndex = _thumbs.getSelectedPageIndex();
        var annos = _viewer.annotations.getFromPage(pageIndex);
        if (annos != null) {
            for (var i = annos.length - 1; i > -1; i--) {
                var anno = annos[i];
                if (anno.selected)
                    _viewer.annotations.deleteFromPage(pageIndex, i);
            }
        }
    }
}
// Add Annotation to page
function CreateCustomTextAnnotation(annoText) {
    var pageIndex = _thumbs.getSelectedPageIndex();
    var textAnno = {
        'type': 'text',
        'x': 50,
        'y': 100,
        'width': 500,
        'height': 200,
        'fill': {
            'color': '#fff',
            'opacity': 0,
        },
        'outline': {
            'color': 'white',
            'width': 0,
            'opacity': 0,
        },
        'text': {
            'value': annoText,
            'align': 'left',
            'font': {
                'bold': false,
                'color': 'black',
                'family': 'Arial',
                'size': 14,
            }
        }
    };
    _viewer.annotations.createOnPage(textAnno, pageIndex);
}
function InsertDocumentId(pageIndex, pageInfo) {
    var idAnno = {
        'type': 'text',
        'x': 10,
        'y': pageInfo.size.height/1.65,
        'width': 500,
        'height': 40,
        'rotation': -90,
        'movable': false,
        'readonly': true,
        'selectable':false,
        'fill': {
            'color': '#fff',
            'opacity': 0,
        },
        'outline': {
            'color': 'white',
            'width': 0,
            'opacity': 0,
        },
        'text': {
            'value': "eWarrant # " + _warrantId,
            'align': 'left',
            'font': {
                'bold': false,
                'color': 'black',
                'family': 'Arial',
                'size': 14,
            }
        }
    };
    _viewer.annotations.createOnPage(idAnno, pageIndex);
}
// Arms the normal interactive "draw an image annotation" tool: the user then either
// - clicks once on the page, placing the image at the click point at a default size
//   (preserving its natural aspect ratio), or
// - clicks and drags, drawing out the image at whatever size/position they choose,
//   exactly as before.
// The viewer's own click-only placement uses a fixed square default size regardless of the
// image's aspect ratio, so a plain click would otherwise stretch/squash the image; we detect
// that case (mouseup very close to mousedown) and resize the resulting annotation to the
// correct aspect ratio in place, re-centered on the click point.
function InsertImageAnnotation(name, src, naturalWidth, naturalHeight, targetWidth) {
    var svg = $('#_containerViewer svg')[0];
    if (!svg) return false;

    var downPoint = null;
    function onDown(e) {
        downPoint = { x: e.clientX, y: e.clientY };
    }
    function onUp(e) {
        svg.removeEventListener('mousedown', onDown, true);
        svg.removeEventListener('mouseup', onUp, true);
        if (!downPoint) return;
        var dx = e.clientX - downPoint.x;
        var dy = e.clientY - downPoint.y;
        var wasClick = Math.sqrt(dx * dx + dy * dy) < 5;
        if (!wasClick) return;
        // Give the viewer a moment to finish creating the default annotation before we resize it.
        setTimeout(function () {
            var pageIndex = _thumbs.getSelectedPageIndex();
            var annos = _viewer.getAnnotationsFromPage(pageIndex);
            var anno = null;
            for (var i = annos.length - 1; i >= 0; i--) {
                if (annos[i].name === name) { anno = annos[i]; break; }
            }
            if (!anno) return;
            var width = targetWidth;
            var height = (naturalWidth > 0) ? (targetWidth * (naturalHeight / naturalWidth)) : targetWidth;
            var cx = anno.x + anno.width / 2;
            var cy = anno.y + anno.height / 2;
            anno.x = cx - width / 2;
            anno.y = cy - height / 2;
            anno.width = width;
            anno.height = height;
            anno.update();
        }, 50);
    }
    svg.addEventListener('mousedown', onDown, true);
    svg.addEventListener('mouseup', onUp, true);

    _thumbs.annotations.drawAnnotation({ 'type': 'image', 'name': name, 'src': src });
    return false;
}