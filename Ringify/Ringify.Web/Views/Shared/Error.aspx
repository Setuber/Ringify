<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Error
</asp:Content>
<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Sorry, an error occurred while processing your request.</h2>
    <% if ((this.Model != null) && (this.Model.Exception != null))
       { %>
    <%= this.Model.Exception.ToString() %>
    <% }
       else
       {
    %>
    <p>
        Please review the troubleshooting section of the Windows Azure Toolkit for WP7 for guidance in this issue.</p>
    <%
        }
    %>
</asp:Content>
