<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Ringify.Web.UserAccountWrappers" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Home
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: ViewData["Message"] %></h2>
    <h3>
        Welcome to the administration site for the Mobile Cloud Application Services!
    </h3>
    <% if (MembershipHelper.IsUserLoggedIn())
       {
           if (MembershipHelper.IsUserAdmin())
           { %>
    <p>
        Select an option from the menu to manage users' permissions and send Push Notifications to mobile devices.
    </p>
        <%
            }
               %>
        <%
        }
    else
    {
        %>
    <p>
        Click <%: Html.ActionLink("Log On", "LogOn", "Account") %> to authenticate.
    </p>
    <%
        }
           %>
</asp:Content>

