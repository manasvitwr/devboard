<%@ Page Title="Kanban Board" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Kanban.aspx.cs" Inherits="DevBoard.Kanban" Async="true" %>

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

            /* Filled state when user has voted — !important beats Bootstrap 5 CSS vars */
            .vote-btn.vote-active-up {
                background-color: #198754 !important;
                color: #fff !important;
                border-color: #198754 !important;
            }

            .vote-btn.vote-active-down {
                background-color: #dc3545 !important;
                color: #fff !important;
                border-color: #dc3545 !important;
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
                <button type="button" class="btn btn-primary ms-2" onclick="showCreateTicketModal()">
                    <i class="bi bi-plus-lg"></i> New Ticket
                </button>
            </div>
        </div>

        <!-- Ticket Modal — intentionally OUTSIDE the UpdatePanel so Bootstrap's
             modal instance is not destroyed by partial postbacks.
             Save button does a full postback; page reloads with updated board. -->
        <div class="modal fade" id="ticketModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog modal-lg">
                <div class="modal-content">
                    <div class="modal-header py-2">
                        <h5 class="modal-title" id="ticketModalLabel">Create/Edit Ticket</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                    </div>
                    <div class="modal-body py-3">
                        <asp:HiddenField ID="TicketIdHidden" runat="server" />
                        <div class="row">
                            <div class="col-md-8 mb-2">
                                <label class="form-label mb-1">Title <span class="text-danger">*</span></label>
                                <asp:TextBox ID="TitleTextBox" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Ticket Summary">
                                </asp:TextBox>
                                <asp:RequiredFieldValidator ID="TitleValidator" runat="server"
                                    ControlToValidate="TitleTextBox" ErrorMessage="Title is required"
                                    CssClass="text-danger small" ValidationGroup="TicketGroup" Display="Dynamic" />
                            </div>
                            <div class="col-md-4 mb-2">
                                <label class="form-label mb-1">Column</label>
                                <asp:DropDownList ID="StatusDropDown" runat="server"
                                    CssClass="form-select form-select-sm">
                                    <asp:ListItem Text="To Do" Value="0" />
                                    <asp:ListItem Text="In Progress" Value="1" />
                                    <asp:ListItem Text="Done" Value="2" />
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="mb-2">
                            <label class="form-label mb-1">Description</label>
                            <asp:TextBox ID="DescriptionTextBox" runat="server" CssClass="form-control form-control-sm"
                                TextMode="MultiLine" Rows="2">
                            </asp:TextBox>
                        </div>
                        <div class="row">
                            <div class="col-md-4 mb-2">
                                <label class="form-label mb-1">Type</label>
                                <asp:DropDownList ID="TypeDropDown" runat="server"
                                    CssClass="form-select form-select-sm">
                                    <asp:ListItem Text="Feature" Value="0" />
                                    <asp:ListItem Text="Bug" Value="1" />
                                    <asp:ListItem Text="QA Debt" Value="2" />
                                    <asp:ListItem Text="Chore" Value="3" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-4 mb-2">
                                <label class="form-label mb-1">Priority</label>
                                <asp:DropDownList ID="PriorityDropDown" runat="server"
                                    CssClass="form-select form-select-sm">
                                    <asp:ListItem Text="Low" Value="0" />
                                    <asp:ListItem Text="Medium" Value="1" />
                                    <asp:ListItem Text="High" Value="2" />
                                </asp:DropDownList>
                            </div>
                            <div class="col-md-4 mb-2">
                                <label class="form-label mb-1">Module</label>
                                <asp:DropDownList ID="ModuleDropDown" runat="server"
                                    CssClass="form-select form-select-sm">
                                </asp:DropDownList>
                            </div>
                        </div>
                        <div class="row align-items-center">
                            <div class="col-md-6 mb-2">
                                <label class="form-label mb-1">Assign To</label>
                                <asp:TextBox ID="AssignToTextBox" runat="server" CssClass="form-control form-control-sm"
                                    placeholder="Email (optional)">
                                </asp:TextBox>
                            </div>
                            <div class="col-md-6 mb-2 pt-3">
                                <div class="form-check">
                                    <asp:CheckBox ID="GitHubSyncCheckBox" runat="server" CssClass="form-check-input" />
                                    <label class="form-check-label" for="<%= GitHubSyncCheckBox.ClientID %>">Create
                                        Issue on GitHub</label>
                                </div>
                            </div>
                        </div>
                        <div id="modalError" class="alert alert-danger d-none"></div>
                    </div>
                    <div class="modal-footer py-2">
                        <button type="button" class="btn btn-secondary btn-sm" data-bs-dismiss="modal">Close</button>
                        <asp:Button ID="SaveTicketButton" runat="server" Text="Save" CssClass="btn btn-primary btn-sm"
                            OnClick="SaveTicketButton_Click" ValidationGroup="TicketGroup" />
                    </div>
                </div>
            </div>
        </div>

        <asp:UpdatePanel ID="KanbanUpdatePanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>

                <div class="row">
                    <div class="col-md-4">
                        <h4 class="text-center mb-3">To Do</h4>
                        <asp:Panel ID="TodoPanel" runat="server" CssClass="kanban-column" data-status="0">
                            <asp:Repeater ID="TodoRepeater" runat="server" OnItemCommand="TicketRepeater_ItemCommand">
                                <ItemTemplate>
                                    <div class="ticket-card" data-ticket-id='<%# Eval("Id") %>'>
                                        <div class="d-flex justify-content-between">
                                            <h6>
                                                <%# Eval("Title") %>
                                            </h6>
                                            <asp:LinkButton ID="EditBtn" runat="server" CommandName="EditTicket"
                                                CommandArgument='<%# Eval("Id") %>' CssClass="text-secondary"><i
                                                    class="bi bi-pencil-square"></i></asp:LinkButton>
                                        </div>
                                        <p class="small mb-2">
                                            <%# Eval("Description") %>
                                        </p>
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div>
                                                <span
                                                    class='badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>'>
                                                    <%# Eval("Type") %>
                                                </span>
                                                <span
                                                    class='badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>'>
                                                    <%# Eval("Priority") %>
                                                </span>
                                            </div>
                                            <div class="vote-section"
                                                data-user-vote='<%# GetUserVote((int)Eval("Id")) %>'>
                                                <button type="button"
                                                    class='btn btn-sm <%# GetUserVote((int)Eval("Id")) == 1 ? "btn-success" : "btn-outline-success" %> vote-btn'
                                                    data-ticket-id='<%# Eval("Id") %>' data-value="1"><i
                                                        class="bi bi-hand-thumbs-up"></i></button>
                                                <span class="badge bg-secondary">
                                                    <%# GetTicketScore((int)Eval("Id")) %>
                                                </span>
                                                <button type="button"
                                                    class='btn btn-sm <%# GetUserVote((int)Eval("Id")) == -1 ? "btn-danger" : "btn-outline-danger" %> vote-btn'
                                                    data-ticket-id='<%# Eval("Id") %>' data-value="-1"><i
                                                        class="bi bi-hand-thumbs-down"></i></button>
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
                            <asp:Repeater ID="InProgressRepeater" runat="server"
                                OnItemCommand="TicketRepeater_ItemCommand">
                                <ItemTemplate>
                                    <div class="ticket-card" data-ticket-id='<%# Eval("Id") %>'>
                                        <div class="d-flex justify-content-between">
                                            <h6>
                                                <%# Eval("Title") %>
                                            </h6>
                                            <asp:LinkButton ID="EditBtn" runat="server" CommandName="EditTicket"
                                                CommandArgument='<%# Eval("Id") %>' CssClass="text-secondary"><i
                                                    class="bi bi-pencil-square"></i></asp:LinkButton>
                                        </div>
                                        <p class="small mb-2">
                                            <%# Eval("Description") %>
                                        </p>
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div>
                                                <span
                                                    class='badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>'>
                                                    <%# Eval("Type") %>
                                                </span>
                                                <span
                                                    class='badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>'>
                                                    <%# Eval("Priority") %>
                                                </span>
                                            </div>
                                            <div class="vote-section"
                                                data-user-vote='<%# GetUserVote((int)Eval("Id")) %>'>
                                                <button type="button"
                                                    class='btn btn-sm <%# GetUserVote((int)Eval("Id")) == 1 ? "btn-success" : "btn-outline-success" %> vote-btn'
                                                    data-ticket-id='<%# Eval("Id") %>' data-value="1"><i
                                                        class="bi bi-hand-thumbs-up"></i></button>
                                                <span class="badge bg-secondary">
                                                    <%# GetTicketScore((int)Eval("Id")) %>
                                                </span>
                                                <button type="button"
                                                    class='btn btn-sm <%# GetUserVote((int)Eval("Id")) == -1 ? "btn-danger" : "btn-outline-danger" %> vote-btn'
                                                    data-ticket-id='<%# Eval("Id") %>' data-value="-1"><i
                                                        class="bi bi-hand-thumbs-down"></i></button>
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
                            <asp:Repeater ID="DoneRepeater" runat="server" OnItemCommand="TicketRepeater_ItemCommand">
                                <ItemTemplate>
                                    <div class="ticket-card" data-ticket-id='<%# Eval("Id") %>'>
                                        <div class="d-flex justify-content-between">
                                            <h6>
                                                <%# Eval("Title") %>
                                            </h6>
                                            <asp:LinkButton ID="EditBtn" runat="server" CommandName="EditTicket"
                                                CommandArgument='<%# Eval("Id") %>' CssClass="text-secondary"><i
                                                    class="bi bi-pencil-square"></i></asp:LinkButton>
                                        </div>
                                        <p class="small mb-2">
                                            <%# Eval("Description") %>
                                        </p>
                                        <div class="d-flex justify-content-between align-items-center">
                                            <div>
                                                <span
                                                    class='badge bg-<%# GetTypeBadgeColor((DevBoard.Models.TicketType)Eval("Type")) %>'>
                                                    <%# Eval("Type") %>
                                                </span>
                                                <span
                                                    class='badge bg-<%# GetPriorityBadgeColor((DevBoard.Models.Priority)Eval("Priority")) %>'>
                                                    <%# Eval("Priority") %>
                                                </span>
                                            </div>
                                            <div class="vote-section"
                                                data-user-vote='<%# GetUserVote((int)Eval("Id")) %>'>
                                                <button type="button"
                                                    class='btn btn-sm <%# GetUserVote((int)Eval("Id")) == 1 ? "btn-success" : "btn-outline-success" %> vote-btn'
                                                    data-ticket-id='<%# Eval("Id") %>' data-value="1"><i
                                                        class="bi bi-hand-thumbs-up"></i></button>
                                                <span class="badge bg-secondary">
                                                    <%# GetTicketScore((int)Eval("Id")) %>
                                                </span>
                                                <button type="button"
                                                    class='btn btn-sm <%# GetUserVote((int)Eval("Id")) == -1 ? "btn-danger" : "btn-outline-danger" %> vote-btn'
                                                    data-ticket-id='<%# Eval("Id") %>' data-value="-1"><i
                                                        class="bi bi-hand-thumbs-down"></i></button>
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

        <script type="text/javascript">
            function updateSaveBtn() {
                var title = document.getElementById('<%= TitleTextBox.ClientID %>').value.trim();
                var btn = document.getElementById('<%= SaveTicketButton.ClientID %>');
                btn.disabled = title.length === 0;
            }

            function showCreateTicketModal() {
                document.getElementById('<%= TicketIdHidden.ClientID %>').value = '';
                document.getElementById('<%= TitleTextBox.ClientID %>').value = '';
                document.getElementById('<%= DescriptionTextBox.ClientID %>').value = '';
                document.getElementById('<%= StatusDropDown.ClientID %>').value = '0';
                document.getElementById('ticketModalLabel').innerText = 'Create Ticket';
                updateSaveBtn();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('ticketModal')).show();
            }

            function showEditTicketModal() {
                document.getElementById('ticketModalLabel').innerText = 'Edit Ticket';
                updateSaveBtn();
                bootstrap.Modal.getOrCreateInstance(document.getElementById('ticketModal')).show();
            }

            document.addEventListener('DOMContentLoaded', function () {
                document.getElementById('<%= TitleTextBox.ClientID %>').addEventListener('input', updateSaveBtn);
            });
        </script>
    </asp:Content>