<%@ Page Title="Users" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" 
	Inherits="System.Web.Mvc.ViewPage<IEnumerable<Beavers.Encounter.Core.User>>" %>
<%@ Import Namespace="Beavers.Encounter.Core" %>
<%@ Import Namespace="Beavers.Encounter.Web.Controllers" %>
 

<asp:Content ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h1>Users</h1>

    <% if (ViewContext.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()] != null) { %>
        <p id="pageMessage"><%= ViewContext.TempData[ControllerEnums.GlobalViewDataProperty.PageMessage.ToString()]%></p>
    <% } %>

    <table>
        <thead>
            <tr>
			    <th>Login</th>
			    <th>Password</th>
			    <th>Nick</th>
			    <th>Phone</th>
			    <th>Icq</th>
			    <th>Team</th>
			    <th colspan="3">Action</th>
            </tr>
        </thead>

		<%
		foreach (User user in ViewData.Model) { %>
			<tr>
				<td><%= user.Login %></td>
				<td><%= user.Password %></td>
				<td><%= user.Nick %></td>
				<td><%= user.Phone %></td>
				<td><%= user.Icq %></td>
				<td><%= user.Team %></td>
				<td><%=Html.ActionLink<UsersController>( c => c.Show( user.Id ), "Details ") %></td>
				<td><%=Html.ActionLink<UsersController>( c => c.Edit( user.Id ), "Edit") %></td>
				<td>
    				<% using (Html.BeginForm<UsersController>(c => c.Delete(user.Id))) { %>
                        <%= Html.AntiForgeryToken() %>
    				    <input type="submit" value="Delete" onclick="return confirm('Are you sure?');" />
                    <% } %>
				</td>
			</tr>
		<%} %>
    </table>

    <p><%= Html.ActionLink<UsersController>(c => c.Create(), "Create New User") %></p>
</asp:Content>
