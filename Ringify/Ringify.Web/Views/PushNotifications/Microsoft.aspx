<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<IEnumerable<Ringify.Web.Models.UserModel>>" %>

<asp:Content ID="pushNotificationsTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Microsoft Push Notifications</asp:Content>
<asp:Content ID="userPermissionsContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2>
        Microsoft Push Notifications</h2>
    <p>
        You can send Microsoft Push Notifications to the following users:</p>
    <table>
        <tr>
            <th class="left-aligned">
                User
            </th>
            <th>
                Notification
            </th>
            <th>
                Commands
            </th>
        </tr>
        <%
            var i = 0;
            foreach (var userModel in this.Model)
            {
                var containerId = Guid.NewGuid();
                i++;
        %>
        <tr class="d<%: (i % 2) %>">
            <td class="left-aligned">
                <%: userModel.UserName %>
            </td>
            <td class="PushNotificationTextColumn">
                <input type="text" id="Push_<%= containerId %>_Message" maxlength="512" />
                <div class="notification-status">
                    <img class="sending hidden" id="Push_<%= containerId %>_Sending" src="/Content/sending.gif" alt="Sending push notification" />
                    <span id="Push_<%= containerId %>_Result"></span>
                </div>
            </td>
            <td class="PushNotificationSendColumn">
                <a class="send-actions" onclick="SendMicrosoftNotification('<%= containerId %>', '<%= userModel.UserId %>', toastActionUrl)">Send Toast</a>
                <a class="send-actions" onclick="SendMicrosoftNotification('<%= containerId %>', '<%= userModel.UserId %>', tileActionUrl)">Send Tile</a>
                <a class="send-actions" onclick="SendMicrosoftNotification('<%= containerId %>', '<%= userModel.UserId %>', rawActionUrl)">Send Raw</a>
            </td>
        </tr>
        <% } %>
    </table>
<script type="text/javascript">
//<![CDATA[
    var toastActionUrl = "<%= this.Url.Action("SendMicrosoftToast", new { userId = "_user_", message = "_message_" }) %>";
    var tileActionUrl = "<%= this.Url.Action("SendMicrosoftTile", new { userId = "_user_", message = "_message_" }) %>";
    var rawActionUrl = "<%= this.Url.Action("SendMicrosoftRaw", new { userId = "_user_", message = "_message_" }) %>";
//]]>
</script>
</asp:Content>
