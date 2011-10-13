<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>
<%@ Import Namespace="Ringify.Web.UserAccountWrappers" %>
<%
    var userName = MembershipHelper.UserName;    
    if (!string.IsNullOrEmpty(userName))
    {
%>
        Welcome <span class="user-display"><%: userName %></span>!
        / <%: Html.ActionLink("Log Off", "LogOff", "Account") %>
<%
    }
    else
    {
%> 
        <%: Html.ActionLink("Log On", "LogOn", "Account") %>
<%
    }
%>

