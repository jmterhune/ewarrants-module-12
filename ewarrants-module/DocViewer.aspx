<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="DocViewer.aspx.cs" Inherits="tjc.Modules.eWarrants.DocViewer" %>
<!DOCTYPE />
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>Document Viewer</title>
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <link href="css/jquery-ui-1.14.0.min.css" type="text/css" rel="stylesheet" />
    <link href="css/Bootstrap.min.css" rel="stylesheet" type="text/css" />
    <link href="css/viewer.css" rel="stylesheet" type="text/css" />
    <link href="css/atalaWebDocumentViewer.css" rel="Stylesheet" type="text/css" />
    <meta name="viewport" content="width=device-width, initial-scale=1">
</head>
<body>
    <form id="frmWarrant" runat="server">
        <iframe style="display: none" src="/Keepalive.aspx"></iframe>
        <asp:PlaceHolder ID="phMessage" runat="server" />
        <asp:HiddenField ID="hdFilePath" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdSignature" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdInitial" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdWarrantId" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdModuleId" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdUserId" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdSignatureHeight" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdSignatureWidth" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdInitialWidth" ClientIDMode="Static" runat="server" />
        <asp:HiddenField ID="hdInitialHeight" ClientIDMode="Static" runat="server" />
        <div id="ViewContainer" runat="server" class="container">
            <div>
                <asp:Literal ID="ltHeader" runat="server"><h1 class='title-header'><strong>Title: </strong>{0} | <strong>Defendant: </strong>{1} | <strong>Agency: </strong>{2}  </h1></asp:Literal>
            </div>
            <div class="main-viewer vh-100">
                <div id="_toolbar" class="atala-document-toolbar">
                    <button id="saveTextAnnotation" title="Save Text Annotation" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_save_text_anno ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-text-save"></span><span class="ui-button-icon-space"></span>Save text annotations</button>
                    <button id="insertTextAnnotation" title="Insert Custom Text" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_insert_text_anno ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-text-insert"></span><span class="ui-button-icon-space"></span>Insert custom text</button>
                    <button id="insertDateText" title="Insert Date" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_insert_date ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-text-date"></span><span class="ui-button-icon-space"></span>Insert date</button>
                    <button id="deleteAnnotaion" title="Delete Annotation" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_delete_anno ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-delete-anno"></span><span class="ui-button-icon-space"></span>Delete Annotation</button>
                    <button id="checkMark" title="Check Mark" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_check_mark ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-check-mark"></span><span class="ui-button-icon-space"></span>Check Mark</button>
                    <button id="crossMark" title="Cross Mark" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_cross_mark ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-cross-mark"></span><span class="ui-button-icon-space"></span>Cross Mark</button>
                    <button id="signatureAnnotation" title="Signature" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_signature ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-signature"></span><span class="ui-button-icon-space"></span>Signature</button>
                    <button id="initialAnnotation" title="Initial" class="ui-widget ui-button-icon-only atala-ui-button atala_toolbar_button atala_tool_button_initial ui-button ui-controlgroup-item"><span class="ui-button-icon ui-icon atala-ui-icon atala-ui-icon-initial"></span><span class="ui-button-icon-space"></span>Initial</button>
                </div>
                <div id="viewer-container" class="inner-viewer">
                    <div id="_containerThumbs" class="atala-document-thumbs"></div>
                    <div class="clickmeleft"></div>
                    <div id="_containerViewer" class="atala-document-viewer"></div>
                </div>
                <hr />
                <footer id="status" class="footer">
                    <asp:Button ID="cmdSave" runat="server" CssClass="btn btn-success button-save" Text="Approve" OnClientClick="ApproveWarrant()" />
                    <asp:Label AssociatedControlID="txtReason" runat="server" CssClass="input-group-text sr-only" ID="btnGroupAddon">Rejection Reason:</asp:Label>
                    <asp:TextBox runat="server" ID="txtReason" ClientIDMode="Static" CssClass="form-control text-reason" MaxLength="250" placeholder="Reject Reason" />
                    <asp:Button ID="cmdCancel" OnClick="cmdCancel_Click" CssClass="btn btn-primary" runat="server" CausesValidation="false" Text="Return to Signing Queue without Action" />
                    <asp:Button ID="cmdReject" CssClass="btn btn-danger button-reject" OnClick="cmdReject_Click" runat="server" CausesValidation="false" Text="Reject" />
                </footer>
            </div>
            <asp:Literal ID="ltAffidavitInfo" runat="server" />
            <div id="attachments" runat="server" visible="false">
                <asp:Repeater ID="rptAttachments" runat="server">
                    <HeaderTemplate>
                        <hr />
                        <div class="row">
                            <div class="col-md-6">
                                <h5><strong>Attachments:</strong></h5>
                                <ul class="attachmentList">
                    </HeaderTemplate>
                    <FooterTemplate>
                        </ul></div><div class="col-md-6">
                            <h5><strong>Instructions:</strong></h5>
                            <ul class="attachmentList">
                                <li><a href="/portals/0/checklist-reviewing-rpo.pdf" target="_blank">RPO Checklist</a></li>
                                <li><a id="showHearingDates" href="#hearingDates">RPO Hearing Dates</a></li>
                                <li><a href="/portals/0/RPO-Statute.pdf" target="_blank">RPO Statute</a></li>
                            </ul>
                        </div>
                        </div>
                    </FooterTemplate>
                    <ItemTemplate>
                        <li class="attachment"><a href='<%#"/desktopmodules/tjc.modules/ewarrants/handlers/attachmentViewer.ashx?aid=" + Eval("FileId") %>' target="_blank"><%#Eval("Description") %></a></li>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div id="LoadingGif" class="loadingGif" style="display: none;"></div>
            <div id="results" title="Results" style="display: none; overflow: auto;">
                <div id="resultsText" style="overflow: auto;"></div>
            </div>
            <div id="dialog" title="Alert Message" style="display: none">
                <div id="dialog-text" class="ui-dialog-content ui-widget-content ">
                </div>
            </div>
        </div>
    </form>
    <div id="dialog_customText">
        <ul id="customTextList">
            <li><span class="listItem">Per Schedule</span></li>
        </ul>
    </div>
    <div class="dimwrapper"></div>
    <script src="js/jquery-3.5.1.min.js"></script>
    <script src="js/jquery-ui-1.14.0.min.js"></script>
    <script src="js/clipboard.min.js" type="text/javascript"></script>
    <script src="js/moment.min.js" type="text/javascript"></script>
    <script src="js/raphael-min.js" type="text/javascript"></script>
    <script src="js/atalaWebDocumentViewer.js" type="text/javascript"></script>
    <script src="js/Initialization.js" type="text/javascript"></script>
</body>
</html>
<script>
    function setElementHeight() {
        const element = document.getElementById('viewer-container');
        const viewerDiv = document.getElementById('_containerViewer');
        if (element) {
            element.style.height = window.innerHeight - 160 + 'px';
            viewerDiv.style.height = element.style.height;
        }
    }

    // Set the height initially
    setElementHeight();

    // Update the height on window resize
    window.addEventListener('resize', setElementHeight);
</script>
