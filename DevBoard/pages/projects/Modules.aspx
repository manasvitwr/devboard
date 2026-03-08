<%@ Page Title="Modules" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Modules.aspx.cs" Inherits="DevBoard.Pages.Modules" %>

    <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <style>
            .module-row {
                background: #fff;
                border: 1px solid #dee2e6;
                border-radius: 6px;
                margin-bottom: 10px;
                overflow: hidden;
            }

            .module-header {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding: 12px 16px;
                background: #f8f9fa;
                border-bottom: 1px solid #dee2e6;
                cursor: pointer;
                user-select: none;
            }

            .module-header:hover {
                background: #e9ecef;
            }

            .module-header .module-name {
                font-weight: 600;
                font-size: 0.95rem;
                color: #212529;
            }

            .module-header .module-path {
                font-size: 0.8rem;
                color: #6c757d;
                margin-left: 10px;
            }

            .module-meta {
                display: flex;
                align-items: center;
                gap: 8px;
            }

            .module-toggle-icon {
                transition: transform 0.2s;
                color: #6c757d;
            }

            .module-toggle-icon.collapsed {
                transform: rotate(-90deg);
            }

            .categories-panel {
                padding: 8px 16px 12px 16px;
            }

            .category-item {
                display: flex;
                align-items: center;
                justify-content: space-between;
                padding: 6px 10px;
                border-radius: 4px;
                margin-bottom: 4px;
                background: #f0f4f8;
                font-size: 0.875rem;
            }

            .category-item:last-child {
                margin-bottom: 0;
            }

            .category-name {
                color: #343a40;
            }

            .no-categories {
                color: #adb5bd;
                font-size: 0.82rem;
                font-style: italic;
                padding: 4px 10px;
            }
        </style>
    </asp:Content>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1>Modules - <asp:Label ID="ProjectNameLabel" runat="server"></asp:Label>
            </h1>
            <a href="Projects.aspx" class="btn btn-secondary">Back to Projects</a>
        </div>

        <asp:Repeater ID="ModulesRepeater" runat="server">
            <ItemTemplate>
                <div class="module-row">
                    <div class="module-header" onclick="toggleCategories(this)">
                        <div>
                            <span class="module-name">
                                <%# Eval("Name") %>
                            </span>
                            <span class="module-path">
                                <%# Eval("Path") %>
                            </span>
                        </div>
                        <div class="module-meta">
                            <span class="badge bg-secondary" title="Tickets">
                                <%# ((System.Collections.Generic.ICollection<DevBoard.Models.Ticket>
                                    )Eval("Tickets")).Count %> tickets
                            </span>
                            <span class="badge bg-info text-dark" title="Categories">
                                <%# ((System.Collections.Generic.ICollection<DevBoard.Models.Category>
                                    )Eval("Categories")).Count %> categories
                            </span>
                            <svg class="module-toggle-icon" xmlns="http://www.w3.org/2000/svg" width="16" height="16"
                                fill="currentColor" viewBox="0 0 16 16">
                                <path fill-rule="evenodd"
                                    d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z" />
                            </svg>
                        </div>
                    </div>
                    <div class="categories-panel">
                        <asp:Repeater ID="CategoriesRepeater" runat="server" DataSource='<%# Eval("Categories") %>'>
                            <ItemTemplate>
                                <div class="category-item">
                                    <span class="category-name">
                                        <%# Eval("Name") %>
                                    </span>
                                    <div class="d-flex gap-2">
                                        <span class="badge bg-light text-dark border" title="Severity Multiplier">
                                            <%# string.Format("{0:0.##}", Eval("SeverityMultiplier")) %>x severity
                                        </span>
                                        <span class="badge bg-light text-dark border" title="Tickets">
                                            <%# ((System.Collections.Generic.ICollection<DevBoard.Models.Ticket>
                                                )Eval("Tickets")).Count %> tickets
                                        </span>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                        <asp:Label ID="NoCategoriesLabel" runat="server"
                            Visible='<%# ((System.Collections.Generic.ICollection<DevBoard.Models.Category>)Eval("Categories")).Count == 0 %>'
                            CssClass="no-categories" Text="No categories defined for this module." />
                    </div>
                </div>
            </ItemTemplate>
        </asp:Repeater>

        <asp:Panel ID="EmptyPanel" runat="server" Visible="false">
            <div class="alert alert-info">No modules found for this project. Use the "Sync from GitHub" button on the
                Projects page to import modules.</div>
        </asp:Panel>

        <script type="text/javascript">
            function toggleCategories(header) {
                var panel = header.nextElementSibling;
                var icon = header.querySelector('.module-toggle-icon');
                if (panel.style.display === 'none') {
                    panel.style.display = '';
                    icon.classList.remove('collapsed');
                } else {
                    panel.style.display = 'none';
                    icon.classList.add('collapsed');
                }
            }
        </script>
    </asp:Content>
