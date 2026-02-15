<%@ Page Title="Modules" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Modules.aspx.cs" Inherits="DevBoard.Modules" %>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1>Modules - <asp:Label ID="ProjectNameLabel" runat="server"></asp:Label>
            </h1>
            <a href="Projects.aspx" class="btn btn-secondary">Back to Projects</a>
        </div>

        <asp:GridView ID="ModulesGridView" runat="server" CssClass="table table-striped table-hover"
            AutoGenerateColumns="false">
            <Columns>
                <asp:BoundField DataField="Name" HeaderText="Module Name" />
                <asp:BoundField DataField="Path" HeaderText="Path" />
                <asp:TemplateField HeaderText="Tickets">
                    <ItemTemplate>
                        <span class="badge bg-info">
                            <%# ((System.Collections.Generic.ICollection<DevBoard.Models.Ticket>)Eval("Tickets")).Count
                                %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <div class="alert alert-info">No modules found for this project. Use the "Sync from GitHub" button on
                    the Projects page to import modules.</div>
            </EmptyDataTemplate>
        </asp:GridView>
    </asp:Content>