<%@ Page Title="Kanban Board" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Kanban.aspx.cs" Inherits="DevBoard.Kanban" %>

    <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <style>
            .kanban-column {
                min-height: 500px;
                background-color: #f8f9fa;
                border-radius: 8px;
                padding: 15px;
            }

            .ticket-card {
                background: white;
                border: 1px solid #dee2e6;
                border-radius: 6px;
                padding: 12px;
                margin-bottom: 10px;
                cursor: move;
                transition: box-shadow 0.2s;
            }

            .ticket-card:hover {
                box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            }

            .ticket-card.dragging {
                opacity: 0.5;
            }

            .vote-section {
                display: flex;
                align-items: center;
                gap: 5px;
            }
        </style>
    </asp:Content>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1><i class="bi bi-kanban"></i> Kanban Board</h1>
            <div>
                <label for="<%= ProjectDropDown.ClientID %>" class="me-2">Project:</label>
                <asp:DropDownList ID="ProjectDropDown" runat="server" CssClass="form-select d-inline-block w-auto"
                    AutoPostBack="true" OnSelectedIndexChanged="ProjectDropDown_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
        </div>

        <asp:UpdatePanel ID="KanbanUpdatePanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div class="row">
                    <div class="col-md-4">
                        <h4 class="text-center mb-3">To Do</h4>
                        <asp:Panel ID="TodoPanel" runat="server" CssClass="kanban-column" data-status="0">
                            <asp:Repeater ID="TodoRepeater" runat="server">
                                <ItemTemplate>
<<<<<<< HEAD
                                    <div class="ticket-card" data-ticket-id="<%# Eval(" Id") %>">
=======
                                    <div class="ticket-card" data-ticket-id="<%# Eval("Id") %>">
>>>>>>> b25426f (temp: save local fixes)
                                        <h6>
                                            <%# Eval("Title") %>
                                        </h6>
                                        <p class="small mb-2">
                                            <%# Eval("Description") %>
                                        </p>
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div>
                                                <span
<<<<<<< HEAD
                                                    class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("
                                                    Type")) %>"><%# Eval("Type") %></span>
                                                <span
                                                    class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("
                                                    Priority")) %>"><%# Eval("Priority") %></span>
                                            </div>
                                            <div class="vote-section">
                                                <button type="button" class="btn btn-sm btn-outline-success vote-btn"
                                                    data-ticket-id="<%# Eval(" Id") %>" data-value="1">
=======
                                                    class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>"><%# Eval("Type") %></span>
                                                <span
                                                    class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>"><%# Eval("Priority") %></span>
                                            </div>
                                            <div class="vote-section">
                                                <button type="button" class="btn btn-sm btn-outline-success vote-btn"
                                                    data-ticket-id="<%# Eval("Id") %>" data-value="1">
>>>>>>> b25426f (temp: save local fixes)
                                                    <i class="bi bi-hand-thumbs-up"></i>
                                                </button>
                                                <span class="badge bg-secondary">
                                                    <%# GetTicketScore((int)Eval("Id")) %>
                                                </span>
                                                <button type="button" class="btn btn-sm btn-outline-danger vote-btn"
<<<<<<< HEAD
                                                    data-ticket-id="<%# Eval(" Id") %>" data-value="-1">
=======
                                                    data-ticket-id="<%# Eval("Id") %>" data-value="-1">
>>>>>>> b25426f (temp: save local fixes)
                                                    <i class="bi bi-hand-thumbs-down"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                    </div>

                    <div class="col-md-4">
                        <h4 class="text-center mb-3">In Progress</h4>
                        <asp:Panel ID="InProgressPanel" runat="server" CssClass="kanban-column" data-status="1">
                            <asp:Repeater ID="InProgressRepeater" runat="server">
                                <ItemTemplate>
<<<<<<< HEAD
                                    <div class="ticket-card" data-ticket-id="<%# Eval(" Id") %>">
=======
                                    <div class="ticket-card" data-ticket-id="<%# Eval("Id") %>">
>>>>>>> b25426f (temp: save local fixes)
                                        <h6>
                                            <%# Eval("Title") %>
                                        </h6>
                                        <p class="small mb-2">
                                            <%# Eval("Description") %>
                                        </p>
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div>
                                                <span
<<<<<<< HEAD
                                                    class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("
                                                    Type")) %>"><%# Eval("Type") %></span>
                                                <span
                                                    class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("
                                                    Priority")) %>"><%# Eval("Priority") %></span>
                                            </div>
                                            <div class="vote-section">
                                                <button type="button" class="btn btn-sm btn-outline-success vote-btn"
                                                    data-ticket-id="<%# Eval(" Id") %>" data-value="1">
=======
                                                    class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>"><%# Eval("Type") %></span>
                                                <span
                                                    class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>"><%# Eval("Priority") %></span>
                                            </div>
                                            <div class="vote-section">
                                                <button type="button" class="btn btn-sm btn-outline-success vote-btn"
                                                    data-ticket-id="<%# Eval("Id") %>" data-value="1">
>>>>>>> b25426f (temp: save local fixes)
                                                    <i class="bi bi-hand-thumbs-up"></i>
                                                </button>
                                                <span class="badge bg-secondary">
                                                    <%# GetTicketScore((int)Eval("Id")) %>
                                                </span>
                                                <button type="button" class="btn btn-sm btn-outline-danger vote-btn"
<<<<<<< HEAD
                                                    data-ticket-id="<%# Eval(" Id") %>" data-value="-1">
=======
                                                    data-ticket-id="<%# Eval("Id") %>" data-value="-1">
>>>>>>> b25426f (temp: save local fixes)
                                                    <i class="bi bi-hand-thumbs-down"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                    </div>

                    <div class="col-md-4">
                        <h4 class="text-center mb-3">Done</h4>
                        <asp:Panel ID="DonePanel" runat="server" CssClass="kanban-column" data-status="2">
                            <asp:Repeater ID="DoneRepeater" runat="server">
                                <ItemTemplate>
<<<<<<< HEAD
                                    <div class="ticket-card" data-ticket-id="<%# Eval(" Id") %>">
=======
                                    <div class="ticket-card" data-ticket-id="<%# Eval("Id") %>">
>>>>>>> b25426f (temp: save local fixes)
                                        <h6>
                                            <%# Eval("Title") %>
                                        </h6>
                                        <p class="small mb-2">
                                            <%# Eval("Description") %>
                                        </p>
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div>
                                                <span
<<<<<<< HEAD
                                                    class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("
                                                    Type")) %>"><%# Eval("Type") %></span>
                                                <span
                                                    class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("
                                                    Priority")) %>"><%# Eval("Priority") %></span>
                                            </div>
                                            <div class="vote-section">
                                                <button type="button" class="btn btn-sm btn-outline-success vote-btn"
                                                    data-ticket-id="<%# Eval(" Id") %>" data-value="1">
=======
                                                    class="badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>"><%# Eval("Type") %></span>
                                                <span
                                                    class="badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>"><%# Eval("Priority") %></span>
                                            </div>
                                            <div class="vote-section">
                                                <button type="button" class="btn btn-sm btn-outline-success vote-btn"
                                                    data-ticket-id="<%# Eval("Id") %>" data-value="1">
>>>>>>> b25426f (temp: save local fixes)
                                                    <i class="bi bi-hand-thumbs-up"></i>
                                                </button>
                                                <span class="badge bg-secondary">
                                                    <%# GetTicketScore((int)Eval("Id")) %>
                                                </span>
                                                <button type="button" class="btn btn-sm btn-outline-danger vote-btn"
<<<<<<< HEAD
                                                    data-ticket-id="<%# Eval(" Id") %>" data-value="-1">
=======
                                                    data-ticket-id="<%# Eval("Id") %>" data-value="-1">
>>>>>>> b25426f (temp: save local fixes)
                                                    <i class="bi bi-hand-thumbs-down"></i>
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </asp:Panel>
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>
    </asp:Content>