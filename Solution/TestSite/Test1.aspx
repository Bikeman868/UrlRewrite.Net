<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="test1.aspx.cs" Inherits="TestSite.test1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <p>This is test1.aspx</p>
        <p>Raw URL: <%=Request.RawUrl%></p>
        <p>Path: <%=Request.Path%></p>
        <p>QueryString: <%=Request.QueryString%></p>
    </div>
    </form>
</body>
</html>
