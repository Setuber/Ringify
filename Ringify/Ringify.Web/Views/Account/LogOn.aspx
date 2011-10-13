<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<Ringify.Web.Models.LogOnModel>" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="TitleContent" runat="server">Log On</asp:Content>

<asp:Content ID="loginContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>Log On</h2>
    <p>
        Please enter your username and password.

    </p>
    
    <% using (Html.BeginForm()) { %>    
    <%: Html.ValidationSummary(true, "Login was unsuccessful. Please correct the errors and try again.") %>
    <div>
        <h3>Account Information</h3>
        <fieldset>
            <legend></legend>
            <%= Html.AntiForgeryToken() %>
            <div style="float:right">
                <p class="note border">
                    <strong>Administrator credentials</strong><br />
                    Username: admin<br />
                    Password: Passw0rd!
                </p>
            </div>
            <div class="editor-label">
                <%: Html.LabelFor(m => m.UserName) %>
            </div>
            <div class="editor-field">
                <%: Html.TextBoxFor(m => m.UserName) %>
                <%: Html.ValidationMessageFor(m => m.UserName) %>
            </div>
                
            <div class="editor-label">
                <%: Html.LabelFor(m => m.Password) %>
            </div>
            <div class="editor-field">
                <%: Html.PasswordFor(m => m.Password) %>
                <%: Html.ValidationMessageFor(m => m.Password) %>
            </div>
                
            <div class="editor-label">
                <%: Html.CheckBoxFor(m => m.RememberMe) %>
                <%: Html.LabelFor(m => m.RememberMe) %>
            </div>
            
            <input type="image" value="Log On" src="<%: Url.Content("~/Content/LogOn.png") %>" />
        </fieldset>
    </div>
    <% } %>
</asp:Content>
