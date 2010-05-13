<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" AutoEventWireup="true" 
    Inherits="System.Web.Mvc.ViewPage" %>
<%@ Import Namespace="Beavers.Encounter.Core"%>
<%@ Import Namespace="Beavers.Encounter.Web.Controllers" %>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContentPlaceHolder" runat="server">
    <h2>��������� ������</h2>
    <h3>��� ������?</h3>
    <%
    if (!((User) User).Identity.IsAuthenticated) 
    { %>
    <p>
        ��� ����� � ������� ��������� �� ������ <%= Html.ActionLink("�����", "LogOn", "Account") %> � ������� ������ ���� ��������.
    </p>
    <p>
        ���� �� ��� �� �����������������, �� ����������������� ����� <%= Html.ActionLink("�����������", "Register", "Account")%>.
    </p>
    <%
    }
    else if (((User)User).Role.IsAuthor)
    { %>
    <p>
        <div>�� ����� ���� &quot;<%= Html.ActionLink<GamesController>(c => c.Edit(((User)User).Game.Id), ((User)User).Game.Name)%>&quot;.</div>
        <div>�� ��������� ��������� �� ������ ����� ����� ����, ��� ��� �� �� ������� ������������� ���.</div>
    </p>
    <%
    }
    else if (((User)User).Team == null)
    { %>
    <p>
        <div>�� ������ �������� � ������������ �������.</div>
        <div>���� �� ������� ������ �������, ���������� � �������, �� �� ������������� ������� �� ���������.</div>
        <p>
        ��� ���������� � ������� ����� ���� �������. �������� ������� �������� ��� � ������ ����, ������ ����� �� ���� � ������� � ����� ����� �������� ��� ������� ����� �������, ����� ��� ����� ����� � ��� ������������������.
        </p>
        <div>���� ���� ���������� � ���, ��� ��� ������� ���� �������� ������� �����, ������� ���������� � ������� ���� � �������� �������� ��� �������.</div>
    </p>
    <%
    }
    else if (((User)User).Team != null && ((User)User).Role.IsTeamLeader)
    { %>
    <p>
        <div>�� ������� ������� &quot;<%= Html.Encode(((User)User).Team.Name)%>&quot;.</div>
        <div>��������, ���������������� �� ���� ������� �� ������� � ������������ ��� ����.</div>
        <p>
        �� ������ ��������� ������� ������� � ����� �������.<br />
        </p>
        <div>�� ������ ������� ����������� ����������, � ���� ������ ��������� ������ �����, ������� � ������ ������� ����� �� ����.</div>
    </p>
    <%
    } 
    else if (((User)User).Team != null && ((User)User).Role.IsPlayer)
    { %>
    <p>
        <div>�� ����� ������� &quot;<%= Html.Encode(((User)User).Team.Name)%>&quot;.</div>
        <div>���� ���� ������� ���������������� �� ������� � �����-���� ����, �� �������� ������ ����.</div>
        <p>
        ��� ������� �� ������ �������� �������, �� �� ���� ����� ��������� � ����������� ������ ��������. ��� ���������� ���������� � ������� ��� ����������� ��� �������.
        </p>
    </p>
    <%
    } %>
</asp:Content>
