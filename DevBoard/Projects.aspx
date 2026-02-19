<%@ Page Title="Projects" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Async="true"
    CodeBehind="Projects.aspx.cs" Inherits="DevBoard.Projects" %>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1>Projects</h1>
            <% if (User.IsInRole("Admin") || User.IsInRole("Dev")) { %>
                <asp:Button ID="ShowCreateButton" runat="server" Text="Create New Project" CssClass="btn btn-primary"
                    OnClick="ShowCreateButton_Click" />
                <% } %>
        </div>

        <asp:Label ID="MessageLabel" runat="server" CssClass="alert" Visible="false"></asp:Label>

        <asp:Panel ID="CreatePanel" runat="server" Visible="false" CssClass="card mb-4">
            <div class="card-body">
                <h3>Create/Edit Project</h3>
                <asp:HiddenField ID="ProjectIdHidden" runat="server" />
                <div class="mb-3">
                    <label class="form-label">Name:</label>
                    <asp:TextBox ID="NameTextBox" runat="server" CssClass="form-control"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="NameRequired" runat="server" ControlToValidate="NameTextBox"
                        ErrorMessage="Name is required" CssClass="text-danger" Display="Dynamic"
                        ValidationGroup="ProjectValidation"></asp:RequiredFieldValidator>
                </div>
                <div class="mb-3">
                    <label class="form-label">Description:</label>
                    <asp:TextBox ID="DescriptionTextBox" runat="server" CssClass="form-control" TextMode="MultiLine"
                        Rows="3"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label class="form-label">GitHub Repository URL:</label>
                    <asp:TextBox ID="RepoUrlTextBox" runat="server" CssClass="form-control"
                        placeholder="https://github.com/user/repo"></asp:TextBox>
                </div>
                <div class="mb-3">
                    <label class="form-label">Config Path:</label>
                    <asp:TextBox ID="ConfigPathTextBox" runat="server" CssClass="form-control"
                        Text="devboard.modules.json"></asp:TextBox>
                </div>
                <asp:Button ID="SaveButton" runat="server" Text="Save" CssClass="btn btn-success"
                    OnClick="SaveButton_Click" ValidationGroup="ProjectValidation" />
                <asp:Button ID="CancelButton" runat="server" Text="Cancel" CssClass="btn btn-secondary"
                    OnClick="CancelButton_Click" CausesValidation="false" />
            </div>
        </asp:Panel>

        <div class="row">
            <asp:Repeater ID="ProjectsRepeater" runat="server" OnItemCommand="ProjectsRepeater_ItemCommand">
                <ItemTemplate>
                    <div class="col-md-6 col-lg-4 mb-3">
                        <div class="card h-100">
                            <div class="card-body">
                                <h5 class="card-title">
                                    <a href='<%# "Kanban.aspx?projectId=" + Eval("Id") %>'
                                        class="text-decoration-none text-dark">
                                        <%# Eval("Name") %>
                                    </a>
                                </h5>
                                <p class="card-text">
                                    <%# Eval("Description") %>
                                </p>
                                <%# !string.IsNullOrEmpty(Eval("RepoUrl") as string)
                                    ? "<p class='card-text'><small class='text-muted'><i class='bi bi-github'></i> <a href='"
                                    + Eval("RepoUrl") + "' target='_blank'>" + Eval("RepoUrl") + "</a></small></p>" : ""
                                    %>
                                    <p class="card-text">
                                        <span class="badge bg-secondary">
                                            <%# ((System.Collections.Generic.ICollection<DevBoard.Models.Module>
                                                )Eval("Modules")).Count %> Modules
                                        </span>
                                    </p>
                            </div>
                            <div class="card-footer">
                                <a href='<%# "Kanban.aspx?projectId=" + Eval("Id") %>' class="btn btn-sm btn-primary">
                                    <i class="bi bi-kanban"></i> View Board
                                </a>
                                <a href='<%# "Modules.aspx?projectId=" + Eval("Id") %>'
                                    class="btn btn-sm btn-outline-info">Modules</a>
                                <% if (User.IsInRole("Admin") || User.IsInRole("Dev")) { %>
                                    <asp:LinkButton ID="SyncButton" runat="server" CssClass="btn btn-sm btn-success"
                                        CommandName="Sync" CommandArgument='<%# Eval("Id") %>'
                                        Visible='<%# !string.IsNullOrEmpty(Eval("RepoUrl") as string) %>'>
                                        <i class="bi bi-arrow-repeat"></i> Sync
                                    </asp:LinkButton>
                                    <asp:LinkButton ID="EditButton" runat="server" CssClass="btn btn-sm btn-warning"
                                        CommandName="Edit" CommandArgument='<%# Eval("Id") %>'>Edit</asp:LinkButton>
                                    <asp:LinkButton ID="DeleteButton" runat="server" CssClass="btn btn-sm btn-danger"
                                        CommandName="Delete" CommandArgument='<%# Eval("Id") %>'
                                        OnClientClick="return confirm('Are you sure you want to delete this project?');">
                                        Delete</asp:LinkButton>
                                    <% } %>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <asp:Panel ID="NoProjectsPanel" runat="server" Visible="false" CssClass="alert alert-info">
            No projects found. Create your first project to get started!
        </asp:Panel>
    </asp:Content>