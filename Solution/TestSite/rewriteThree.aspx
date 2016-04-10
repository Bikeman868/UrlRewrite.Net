<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="rewriteThree.aspx.cs" Inherits="TestSite.rewriteThree" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>This is rewriteThree.aspx</p>
        <p>Raw URL: <%=Request.RawUrl%></p>
        <p>Path: <%=Request.Path%></p>
        <p>QueryString: <%=Request.QueryString%></p>
    </div>
    </form>
</body>
</html>
