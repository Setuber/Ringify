<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Ringify.Web.Models.StorageItemPermissionsModel>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Table Permissions</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Manage Table Permissions</h2>
    <p>
        Select which users can access each table.
    </p>
        <% 
            var users = this.ViewData["users"] as IEnumerable<Ringify.Web.Models.UserModel>;
            foreach (var item in this.Model)
            {                
                %>
                <div class="storage-heading">
                    <div class="public-permissions">
                        <input type="hidden" id="Public_<%= item.StorageItemName %>" value="<%= item.IsPublic ? "true" : "false"%>" />
                        <div class="access-title">
                            <span>Table Access: </span>                        
                            <span class="access-value <%= !item.IsPublic ? "hidden" : ""%>" id="Public_<%= item.StorageItemName %>_Public">Public</span>
                            <span class="access-value <%= item.IsPublic ? "hidden" : ""%>" id="Public_<%= item.StorageItemName %>_Private">Private</span>
                        </div>&nbsp;&nbsp;
                        <div class="access-action">
                            (<a onclick="SetPublic('<%= item.StorageItemName %>', publicActionUrl)">
                                <span id="Public_<%= item.StorageItemName %>_SetPublic" class="<%= item.IsPublic ? "hidden" : ""%>">Set Public</span>
                                <span id="Public_<%= item.StorageItemName %>_SetPrivate" class="<%= !item.IsPublic ? "hidden" : ""%>">Set Private</span>
                            </a>)
                        </div>
                    </div>
                    <p class="storage-title">Table: <span><%: item.StorageItemName %></span></p>
                </div>                    
                <fieldset class="group">
                    <legend></legend>
                <% 
                         
                   foreach (var user in users)
                   {
                       var containerId = Guid.NewGuid();
                       var hasAccess = item.AllowedUserIds.Contains(user.UserId);
                %>
                    <div class="user">
                        <input id="<%= item.StorageItemName %>_<%= containerId %>" name="<%= item.StorageItemName %>_<%= containerId %>" onclick="SetPermission('<%= containerId %>', '<%= user.UserId %>', '<%= item.StorageItemName %>', addTablePermissionActionUrl, removeTablePermissionActionUrl)"
                            type="checkbox" <%= hasAccess ? "checked=\"checked\"" : string.Empty %> <%= item.IsPublic ? "disabled=\"disabled\"" : string.Empty %> />
                        <label for="<%= item.StorageItemName %>_<%= containerId %>">
                            <%: user.UserName %></label>
                    </div>
                <% } %>
                </fieldset>
        <% } %>
<script type="text/javascript">
//<![CDATA[
    var publicActionUrl = "<%= this.Url.Action("SetTablePublic", new { table = "_storage_", isPublic = "_isPublic_" }) %>";
    var addTablePermissionActionUrl = "<%= this.Url.Action("AddTablePermission", new { table = "_storage_", userId = "_user_" }) %>";
    var removeTablePermissionActionUrl = "<%= this.Url.Action("RemoveTablePermission", new { table = "_storage_", userId = "_user_" }) %>";
//]]>
</script>
</asp:Content>