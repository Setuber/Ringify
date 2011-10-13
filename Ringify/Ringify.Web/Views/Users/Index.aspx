<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Ringify.Web.Models.UserPermissionsModel>>" %>

<asp:Content ID="userPermissionsTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Manage User Permissions</asp:Content>
<asp:Content ID="userPermissionsContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Manage User Permissions</h2>
    <p>
        Select what content type can access each user.
    </p>
    <table>
        <tr>
            <th class="left-aligned">
                User
            </th>
            <th>
                Tables
            </th>
            <th>
                Blobs
            </th>
            <th>
                Queues
            </th>
        </tr>
        <%
            var i = 0;
            foreach (var item in this.Model)
            {
                i++;
        %>
        <tr class="d<%: (i % 2) %>">
            <td class="left-aligned">
                <%: item.UserName %>
            </td>
            <td>
                <input id="Tables_<%= item.UserId %>" onclick="EnableContent('<%= item.UserId %>', actionUrl)"
                    type="checkbox" <%= item.TablesUsage ? "checked=\"checked\"" : string.Empty %> />
            </td>
            <td>
                <input id="Blobs_<%= item.UserId %>" onclick="EnableContent('<%= item.UserId %>', actionUrl)"
                    type="checkbox" <%= item.BlobsUsage ? "checked=\"checked\"" : string.Empty %> />
            </td>
            <td>
                <input id="Queues_<%= item.UserId %>" onclick="EnableContent('<%= item.UserId %>', actionUrl)"
                    type="checkbox" <%= item.QueuesUsage ? "checked=\"checked\"" : string.Empty %> />
            </td>
        </tr>
        <% } %>
    </table>
<script type="text/javascript">
//<![CDATA[
    var actionUrl = "<%= this.Url.Action("SetUserPermissions", new { userId = "_user_", useTables = "_useTables_", useBlobs = "_useBlobs_", useQueues = "_useQueues_" })  %>";
//]]>
</script>
</asp:Content>
