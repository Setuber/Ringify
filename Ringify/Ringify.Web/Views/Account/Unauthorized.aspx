<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="unauthorizedTitle" ContentPlaceHolderID="TitleContent" runat="server">Unauthorized</asp:Content>

<asp:Content ID="unauthorizedContent" ContentPlaceHolderID="MainContent" runat="server">

    <h2>Unauthorized</h2>

    <p>You have no permissions to access this resource.</p>

</asp:Content>
