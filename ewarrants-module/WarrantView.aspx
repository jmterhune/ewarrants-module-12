<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WarrantView.aspx.cs" Inherits="tjc.Modules.eWarrants.WarrantView" %>

<%@ Register Assembly="Atalasoft.dotImage.WebControls" Namespace="Atalasoft.Imaging.WebControls" TagPrefix="AWC" %>
<%@ Register TagPrefix="zc" TagName="Zooming" Src="~/DesktopModules/tjc.Modules/ewarrants/controls/ZoomControl.ascx" %>
<%@ Register TagPrefix="nc" TagName="Navigation" Src="~/DesktopModules/tjc.Modules/ewarrants/controls/NavigationControl.ascx" %>
<%@ Register Assembly="Atalasoft.dotImage.WebControls" Namespace="Atalasoft.Imaging.WebControls.Annotations" TagPrefix="AWA" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="hdrWarrant" runat="server">
    <link rel="stylesheet" type="text/css" href="css/JudeView.min.css?ver=6-5-24" />
    <link rel="stylesheet" href="/Resources/Libraries/jQuery-UI/01_13_02/jquery-ui.css?v=1" type="text/css" />
    <style>
        .affidavit {
            color: #000;
            list-style: none;
        }

            .affidavit li {
                color: #000;
                display: block !important;
                padding: 5px 20px;
            }

        h5 {
            font-weight: bold;
            color: #000;
            font-size: 1.2em;
            margin-top: 20px;
            margin-bottom: 5px
        }

        strong {
            font-weight: bold !important
        }
    </style>
    <!--[if lte IE 8]>
<script src="js/html5.js" type="text/javascript"></script>
        <link rel="stylesheet" type="text/css" href="css/ie.css" />
<![endif]-->

    <title>Review Warrant</title>
</head>
<body>
    <form id="frmWarrant" runat="server">
        <script src="js/jquery-1.7.1.min.js?v=1" type="text/javascript"></script>
        <script src="/Resources/Libraries/jQuery-UI/01_12_01/jquery-ui.js?v=1" type="text/javascript"></script>
        <script type="text/javascript" src="js/jquery.form.js?v=1"></script>
        <asp:PlaceHolder ID="phMessage" runat="server" />
        <div id="container" runat="server">
            <header>
                <hgroup>
                    <h1>
                        <asp:Literal ID="ltHeader" runat="server"></asp:Literal></h1>
                </hgroup>
                <menu type="toolbar" class="main">
                    <ul class="main_ul">

                        <li>
                            <nc:Navigation runat="server" />
                        </li>

                        <li>
                            <zc:Zooming runat="server" />
                        </li>

                        <li>
                            <menu class="sub" label="Annotations">
                                <asp:ImageButton CssClass="button" ID="Button_Redaction" runat="server" ImageUrl="~/Icons/Redact.png"
                                    ToolTip="Creates redaction annotations." Height="24px" Width="24px" OnClientClick="CreateAnnotation('RectangleData', 'DefaultRedaction'); return false;" />
                                <asp:ImageButton CssClass="button" ID="Button_Delete" runat="server" ImageUrl="~/Icons/Delete-blue.png"
                                    ToolTip="Deletes the Selected Annotation from the Image" Height="24px" Width="24px"
                                    OnClientClick="Delete(); return false;" />
                            </menu>
                            <a>Annotations</a>
                        </li>
                        <li>
                            <menu class="sub" label="Sign">
                                <asp:ImageButton CssClass="button" ID="Button_SignStamp" runat="server" ImageUrl="~/Icons/rubber stamp.png"
                                    ToolTip="Creates signature annotation." Height="24px" Width="24px" OnClientClick="CreateAnnotation('EmbeddedImageData', 'DefaultSignature'); return false;" />
                            </menu>
                            <a>Sign</a>
                        </li>
                        <li>
                            <menu class="sub" label="Initial">
                                <asp:ImageButton CssClass="button" ID="Button_InitialStamp" runat="server" ImageUrl="~/Icons/rubber stamp-i.png"
                                    ToolTip="Creates Initials annotation." Height="24px" Width="24px" OnClientClick="CreateAnnotation('EmbeddedImageData', 'DefaultInitial'); return false;" />
                            </menu>
                            <a>Initial</a>
                        </li>
                        <li>
                            <menu class="sub" label="CheckMark">
                                <asp:ImageButton CssClass="button" ID="Button_CheckMark" runat="server" ImageUrl="~/Icons/check.png"
                                    ToolTip="Creates Check Mark annotation." Height="24px" Width="24px" OnClientClick="CreateAnnotation('EmbeddedImageData', 'DefaultCheckMark'); return false;" />

                            </menu>
                            <a>Check</a>
                        </li>

                        <li id="dateNormal" runat="server">
                            <menu class="sub" label="Stamp">
                                <asp:ImageButton class="button" ID="Button_Date" runat="server" ImageUrl="~/Icons/calendar.png"
                                    ToolTip="Creates Date annotation." Height="24px" Width="24px" OnClientClick="CreateAnnotation('EmbeddedImageData', 'DefaultDateStamp'); return false;" />
                            </menu>
                            <a>Date</a>
                        </li>
                        <li id="dateRPO" runat="server" visible="false">
                            <menu class="sub" label="Stamp">
                                <input type="text" id="dateInput" class="datepicker" style="display: none;" /><img src="/Icons/calendar.png" id="dateImage" style="cursor: pointer" />
                            </menu>
                            <a>Date</a>

                        </li>
                        <li>
                            <menu class="sub" label="Stamp">
                                <span style="display: inline-block; margin-top: 2px; float: right;">
                                    <input type="text" id="txtStampText" name="lname" maxlength="50" style="width: 100px" /><asp:Button ID="btnText" OnClientClick="SetText(); return false;" runat="server" Text="Insert" /></span>

                                <asp:ImageButton CssClass="button" ID="Button_PS" runat="server" ImageUrl="~/Icons/ps.png"
                                    ToolTip="Creates Per Schedule annotation." Height="24px" Width="24px" OnClientClick="CreateAnnotation('EmbeddedImageData', 'DefaultPerscheduleStamp'); return false;" />
                            </menu>
                            <a>Per Schedule</a>
                        </li>
                        <li>
                            <menu class="sub" label="Save Text">
                                <asp:ImageButton CssClass="button" ID="Button_Text" runat="server" ImageUrl="~/Icons/Text.png"
                                    ToolTip="Creates text annotations." Height="24px" Width="24px" OnClientClick="CreateAnnotation('TextData', 'DefaultTextAnnotation'); return false;" />

                                <asp:ImageButton CssClass="button" ID="SaveText" runat="server" ImageUrl="~/Icons/SaveText.png"
                                    ToolTip="Save text annotations" Height="24px" Width="24px" OnClientClick="GetTextFromAnnotation(); return false;" />
                                <asp:ImageButton CssClass="button" ClientIDMode="Static" ID="showCustomTextList" runat="server" ImageUrl="~/Icons/add-list.png"
                                    ToolTip="Insert custom text" Height="24px" Width="24px" />

                            </menu>
                            <a>Custom Text</a>
                        </li>

                    </ul>
                </menu>
            </header>
            <section id="tjcViewer">
                <ul>
                    <li id="wtv">
                        <AWC:WebThumbnailViewer ID="WebThumbnailViewer1" runat="server" BackColor="Gainsboro"
                            Height="100%" Width="100%" ViewerID="WebAnnotationViewer1" Centered="True" ShowAnnotations="true"
                            AllowDragDrop="false" ThumbStyleSelected-BackColor="DimGray"
                            ThumbStyleSelectedHover-BackColor="LightSlateGray" ThumbSize="100,100" />
                    </li>
                    <li id="wav">
                        <AWA:WebAnnotationViewer ID="WebAnnotationViewer1" runat="server" BackColor="Gainsboro"
                            Height="100%" Width="100%" Centered="True" AntialiasDisplay="ReductionOnly" AutoLinkThumbnailViewer="True"
                            InteractMode="Modify" AuthorMode="Single" />
                    </li>

                </ul>
            </section>
            <div id="attachments" runat="server" visible="false">
                <asp:Repeater ID="rptAttachments" runat="server">
                    <HeaderTemplate>
                        <ul class="attachmentList">
                            <li><strong>Attachments:</strong></li>
                    </HeaderTemplate>
                    <FooterTemplate>
                        </ul>
                        <ul class="attachmentList">
                            <li><strong>Instructions:</strong></li>
                            <li><a href="/portals/0/checklist-reviewing-rpo.pdf" target="_blank">RPO Checklist</a></li>
                            <li><a id="showHearingDates" href="#hearingDates">RPO Hearing Dates</a></li>
                            <li><a href="/portals/0/RPO-Statute.pdf" target="_blank">RPO Statute</a></li>
                        </ul>

                    </FooterTemplate>
                    <ItemTemplate>
                        <li class="attachment"><a href='<%#"/desktopmodules/tjc.modules/ewarrants/handlers/attachmentViewer.ashx?aid=" + Eval("FileId") %>' target="_blank"><%#Eval("Description") %></a></li>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
            <div id="spin-overlay" class="overlay" style="display: none">
                <div class="overlay-text"><strong>Large files take a long time to process. Please Wait!</strong></div>
                <div class="spinner">
                    <img src="Images/atala-ui-spinner.gif" /></div>
            </div>
            <footer id="status" class="dnnForm dnnClear">
                <ul class="dnnActions dnnClear">
                    <li id="sb">
                        <asp:Button ID="cmdSave" runat="server" CssClass="dnnPrimaryAction" Text="Approve" OnClientClick="ApproveWarrant(); return false;" /></li>
                    <li id="rb">
                        <input id="txtReason" type="text" name="Reason" maxlength="250" placeholder="Reject Reason" />
                        <asp:Button ID="cmdReject" CssClass="dnnSecondaryAction" runat="server" CausesValidation="true" Text="Reject" OnClientClick="RejectWarrant(); return false;" /></li>
                    <li id="cb">
                        <asp:Button ID="cmdCancel" CssClass="dnnSecondaryAction" runat="server" CausesValidation="false" Text="Return to Signing Queue without Action" OnClientClick="CancelWarrant(); return false;" /></li>
                </ul>
                <asp:Literal ID="ltAffidavitInfo" runat="server" />
            </footer>
            <div id="dialog" title="Alert message" style="display: none">
                <div class="ui-dialog-content ui-widget-content">
                    <p>
                        <span class="ui-icon ui-icon-alert" style="float: left; margin: 0 7px 20px 0"></span>
                        <label id="lblMessage">
                        </label>
                    </p>
                </div>
            </div>
            <div id="dialog-confirm" title="Signature Verification" style="display:none">
                <p><span class="ui-icon ui-icon-alert" style="float: left; margin: 12px 12px 20px 0;"></span> A Signature has not been detected? To Continue without a signature, select the Continue button. To Cancel Approval and return to the warrant, select the Cancel button</p>
            </div>
        </div>
        <asp:HiddenField ID="hdtoCancel" runat="server" />
        <asp:Literal runat="server" ID="keepalive"> <iframe src="/desktopmodules/tjc.Modules/ewarrants/blank.aspx" style="display:none; visibility:hidden;width:0px;height:0px;"></iframe></asp:Literal>
    </form>

    <div id="dialog_customText">
        <ul id="customTextList">
            <li><span class="listItem">Per Schedule</span></li>
        </ul>
    </div>
    <div id="dialogHearingDates">
        <p>The probate judges have blocked time for compliance hearings and final hearings. Compliance hearings and final hearings should be set on one of the days listed below. The proposed order already has the time, you just need to select the date.</p>
        <br />
        <ul class="dialogList">
            <li><strong>Compliance hearings</strong> - within 3 business days of the date of the temporary order</li>
            <li><strong>Final hearings</strong> - use the latest date available that falls within 14 days of the date of the temporary order</li>
        </ul>
        <div style="margin: 15px; border: solid 1px #ccc; padding: 5px;">
            <ul class="dialogList">
                <li><strong>Desoto</strong> – Mondays and Fridays (11:00am)</li>
                <li><strong>Manatee</strong> – Mondays and Thursdays (1:30pm)</li>
                <li><strong>Sarasota</strong> – Wednesdays and Thursdays (1:00pm)</li>
            </ul>
        </div>
    </div>

    <script type="text/javascript">
        var currentTime = "<%=DateTime.Today.ToString("MM/dd/yyyy")%>";
        var isRPO =<%=_isRPO%>;
        var refreshbutton = $("#cmdRefresh");
        function NavigateManageText() {
            location.replace("<%=NavigateUrl%>");
            return false;
        }
        if (window.innerWidth <= 1024) {
            window.setTimeout(function () {
                var iFrameContent = $('#WebAnnotationViewer1_ov').contents();
                var divHeight = iFrameContent.find('div[unselectable="on"]').first().height();
                if (divHeight > 1100) {
                    $('#tjcViewer').height(divHeight);
                }
            }, 2000);
        }
    </script>
    <script type="text/javascript" src="js/HTMLAdvDocViewer.min.js?ver=ver=1-13-25-4"></script>
</body>
</html>
