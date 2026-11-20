<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WarrantReview.aspx.cs" Inherits="tjc.Modules.eWarrants.WarrantReview" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Warrant Review</title>
    <meta content="text/html; charset=UTF-8" http-equiv="Content-Type" />
    <meta id="MetaRobots" name="robots" content="NOINDEX, NOFOLLOW" />
    <link rel="stylesheet" type="text/css" href="/portals/_default/skins/Xcillion/bootstrap/css/bootstrap.min.css" />
    <link rel="stylesheet" type="text/css" href="/Portals/_default/skins/xcillion/css/font-awesome.min.css" />
    <link href="/Resources/Libraries/dataTables.bootstrap5.min.css?cdv=168" type="text/css" rel="stylesheet" />
    <link rel="SHORTCUT ICON" href="/Portals/0/favicon.ico?ver=gE-UzhY6CsRyUbgAf3S-9g%3d%3d" type="image/x-icon" />
    <script src="/Resources/Libraries/jQuery/03_05_01/jquery.js"></script>
    <script src="/Resources/Libraries/jQuery-UI/01_12_01/jquery-ui.js" type="text/javascript"></script>
    <meta name="viewport" content="width=device-width,initial-scale=1" />
    <style>
        body, html, form {
            margin: 0;
            padding: 0;
            height: 100%;
            overflow: hidden;
        }

        iframe {
            border: none;
            width: 100%;
            height: 100%;
        }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <script src="/Portals/_default/Skins/Xcillion/Bootstrap/js/bootstrap.min.js"></script>
        <div style="text-align: center; padding: 8px">
            <asp:Button ID="cmdComplete" CssClass="btn btn-success me-2" Text="Accept Warrant" runat="server" OnClick="cmdComplete_Click" />
            <asp:Button ID="cmdReject" CssClass="btn btn-danger" Text="Reject as Unsigned" runat="server" OnClick="cmdReject_Click" />
        </div>
        <iframe id="ifDocumentViewer" runat="server"></iframe>
        <div style="padding: 0 25%">
            <asp:Literal ID="ltMessage" runat="server" /></div>
    </form>
</body>
</html>
