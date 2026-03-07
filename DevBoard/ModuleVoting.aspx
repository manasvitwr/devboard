<%@ Page Title="Module Voting" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="ModuleVoting.aspx.cs" Inherits="DevBoard.ModuleVoting" EnableEventValidation="false" %>

    <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <style>
            /* ── Layout ── */
            .mv-layout {
                display: flex;
                gap: 20px;
                align-items: flex-start;
            }

            .mv-main {
                flex: 1 1 70%;
                min-width: 0;
            }

            .mv-sidebar {
                flex: 0 0 28%;
            }

            /* ── Page Header ── */
            .mv-page-header {
                display: flex;
                align-items: center;
                justify-content: space-between;
                margin-bottom: 20px;
            }

            .mv-page-title {
                font-size: 1.6rem;
                font-weight: 700;
                display: flex;
                align-items: center;
                gap: 12px;
            }

            .mv-page-actions {
                display: flex;
                gap: 8px;
            }

            /* ── Tabs ── */
            .mv-tabs {
                display: flex;
                border-bottom: 2px solid #dee2e6;
                margin-bottom: 16px;
            }

            .mv-tab {
                padding: 8px 16px;
                font-weight: 500;
                font-size: 0.9rem;
                cursor: pointer;
                color: #6c757d;
                border-bottom: 3px solid transparent;
                margin-bottom: -2px;
                transition: color .15s, border-color .15s;
            }

            .mv-tab.active {
                color: #0d6efd;
                border-bottom-color: #0d6efd;
            }

            .mv-tab:hover:not(.active) {
                color: #343a40;
            }

            /* ── Module Card ── */
            .module-card {
                background: #fff;
                border: 1px solid #e0e0e0;
                border-radius: 10px;
                margin-bottom: 14px;
                overflow: hidden;
                box-shadow: 0 2px 6px rgba(0, 0, 0, .04);
                transition: box-shadow .2s;
            }

            .module-card:hover {
                box-shadow: 0 4px 14px rgba(0, 0, 0, .09);
            }

            .module-header {
                display: flex;
                align-items: center;
                gap: 12px;
                padding: 13px 16px;
                cursor: pointer;
                user-select: none;
                background: #fafafa;
            }

            .module-icon {
                width: 34px;
                height: 34px;
                border-radius: 8px;
                display: flex;
                align-items: center;
                justify-content: center;
                font-size: 1rem;
                flex-shrink: 0;
            }

            .module-icon.critical {
                background: #fee2e2;
                color: #dc2626;
            }

            .module-icon.normal {
                background: #dcfce7;
                color: #16a34a;
            }

            .module-name {
                font-weight: 700;
                font-size: 1.05rem;
                flex: 1;
            }

            .module-tags {
                display: flex;
                gap: 6px;
                align-items: center;
            }

            .tag-critical {
                background: #fee2e2;
                color: #dc2626;
                font-size: .72rem;
                font-weight: 600;
                padding: 2px 8px;
                border-radius: 20px;
            }

            .tag-underperform {
                background: #fef9c3;
                color: #a16207;
                font-size: .72rem;
                font-weight: 600;
                padding: 2px 8px;
                border-radius: 20px;
            }

            .tag-moderate {
                background: #dbeafe;
                color: #1d4ed8;
                font-size: .72rem;
                font-weight: 600;
                padding: 2px 8px;
                border-radius: 20px;
            }

            .tag-high-risk {
                background: #fde68a;
                color: #92400e;
                font-size: .72rem;
                font-weight: 600;
                padding: 2px 8px;
                border-radius: 20px;
                animation: pulse-warn 1.5s ease-in-out infinite;
            }

            @keyframes pulse-warn {

                0%,
                100% {
                    opacity: 1;
                }

                50% {
                    opacity: .6;
                }
            }

            .module-health-pct {
                min-width: 48px;
                text-align: center;
                padding: 4px 10px;
                border-radius: 8px;
                font-weight: 700;
                font-size: .95rem;
            }

            .health-green {
                background: #dcfce7;
                color: #15803d;
            }

            .health-yellow {
                background: #fef9c3;
                color: #a16207;
            }

            .health-red {
                background: #fee2e2;
                color: #dc2626;
            }

            .module-chevron {
                font-size: .8rem;
                color: #9ca3af;
                transition: transform .2s;
                flex-shrink: 0;
            }

            .module-chevron.open {
                transform: rotate(180deg);
            }

            /* health bar strip */
            .module-health-bar-wrap {
                height: 4px;
                background: #f1f5f9;
            }

            .module-health-bar {
                height: 100%;
                transition: width .4s;
            }

            /* ── Categories Panel ── */
            .categories-panel {
                padding: 0 14px 12px;
                display: none;
            }

            .categories-panel.open {
                display: block;
            }

            .category-row {
                display: flex;
                align-items: center;
                gap: 10px;
                padding: 9px 10px;
                border-radius: 8px;
                margin-bottom: 4px;
                background: #f8fafc;
                border: 1px solid #f1f5f9;
                transition: background .15s;
            }

            .category-row:hover {
                background: #f0f4f8;
            }

            .category-row.high-risk {
                border-color: #fbbf24;
                background: #fffbeb;
            }

            /* vote buttons — match Kanban style exactly */
            .cat-vote-col {
                display: flex;
                align-items: center;
                gap: 4px;
                flex-shrink: 0;
            }

            .cat-name {
                font-weight: 500;
                font-size: .875rem;
                flex: 1;
                min-width: 0;
            }

            /* stress bar */
            .cat-bar-wrap {
                width: 80px;
                height: 8px;
                background: #e5e7eb;
                border-radius: 4px;
                flex-shrink: 0;
            }

            .cat-bar {
                height: 100%;
                border-radius: 4px;
                transition: width .4s;
            }

            .cat-tickets {
                font-size: .78rem;
                color: #6b7280;
                white-space: nowrap;
                flex-shrink: 0;
            }

            /* ── Sidebar ── */
            .sidebar-card {
                background: #fff;
                border: 1px solid #e0e0e0;
                border-radius: 10px;
                box-shadow: 0 2px 6px rgba(0, 0, 0, .04);
                overflow: hidden;
            }

            .sidebar-card-header {
                padding: 12px 16px;
                font-weight: 700;
                font-size: .95rem;
                border-bottom: 1px solid #f1f5f9;
                background: #fafafa;
                display: flex;
                justify-content: space-between;
                align-items: center;
            }

            .vote-feed-item {
                display: flex;
                align-items: center;
                gap: 10px;
                padding: 10px 14px;
                border-bottom: 1px solid #f8fafc;
                font-size: .82rem;
            }

            .vote-feed-item:last-child {
                border-bottom: none;
            }

            .vote-feed-avatar {
                width: 30px;
                height: 30px;
                border-radius: 50%;
                display: flex;
                align-items: center;
                justify-content: center;
                font-weight: 700;
                font-size: .75rem;
                flex-shrink: 0;
            }

            .avatar-up {
                background: #dcfce7;
                color: #16a34a;
            }

            .avatar-down {
                background: #fee2e2;
                color: #dc2626;
            }

            .vote-feed-text {
                flex: 1;
                line-height: 1.4;
            }

            .vote-feed-text strong {
                font-weight: 600;
            }

            .vote-feed-time {
                color: #9ca3af;
                font-size: .75rem;
                flex-shrink: 0;
            }

            /* ── Empty state ── */
            .mv-empty {
                text-align: center;
                padding: 40px 20px;
                color: #9ca3af;
            }

            @media (max-width:900px) {
                .mv-layout {
                    flex-direction: column;
                }

                .mv-sidebar {
                    flex: unset;
                    width: 100%;
                }
            }
        </style>
    </asp:Content>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

        <div class="mv-page-header">
            <div class="mv-page-title">
                Module Voting
                <asp:DropDownList ID="ProjectDropDown" runat="server" CssClass="form-select form-select-sm"
                    style="width:auto;font-size:.8rem;font-weight:600;" AutoPostBack="true"
                    OnSelectedIndexChanged="ProjectDropDown_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
            <div class="mv-page-actions">
                <button type="button" class="btn btn-outline-secondary btn-sm">Filters</button>
                <button type="button" class="btn btn-primary btn-sm" onclick="showCreateIssueModal()">+ Create
                    Issue</button>
            </div>
        </div>

        <div class="mv-tabs">
            <div class="mv-tab active" id="tab-top" onclick="switchTab('top')">Top Modules</div>
            <div class="mv-tab" id="tab-low" onclick="switchTab('low')">Low Priority</div>
        </div>

        <div class="mv-layout">
            <!-- Module List -->
            <div class="mv-main" id="mv-main-col">

                <asp:Repeater ID="ModulesRepeater" runat="server" OnItemDataBound="ModulesRepeater_ItemDataBound">
                    <ItemTemplate>
                        <div class="module-card" data-tab='<%# Eval("Tab") %>' data-module-id='<%# Eval("Id") %>'>

                            <div class="module-header" onclick="toggleModule(this)">
                                <div class='<%# "module-icon " + Eval("IconClass") %>'>
                                    <i
                                        class='<%# (bool)Eval("IsCritical") ? "bi bi-exclamation-triangle-fill" : "bi bi-check-circle-fill" %>'></i>
                                </div>
                                <div class="module-name">
                                    <%# Eval("Name") %>
                                </div>
                                <div class="module-tags">
                                    <%# Eval("TagHtml") %>
                                </div>
                                <div class='<%# "module-health-pct " + Eval("HealthClass") %>'>
                                    <%# Eval("HealthPct") %>%
                                </div>
                                <div class="module-chevron">&#9660;</div>
                            </div>

                            <div class="module-health-bar-wrap">
                                <div class="module-health-bar"
                                    style='<%# "width:" + Eval("HealthPct") + "%;background:" + Eval("HealthBarColor") %>'>
                                </div>
                            </div>

                            <div class="categories-panel">
                                <asp:Repeater ID="CategoriesRepeater" runat="server">
                                    <ItemTemplate>
                                        <div class='<%# "category-row" + ((bool)Eval("IsHighRisk") ? " high-risk" : "") %>'
                                            data-category-id='<%# Eval("Id") %>'
                                            data-severity='<%# Eval("SeverityMultiplier") %>'
                                            data-user-vote='<%# Eval("UserVote") %>'>

                                            <!-- Thumbs-up / down vote buttons — same pattern as Kanban -->
                                            <div class="cat-vote-col">
                                                <button type="button"
                                                    class='<%# "btn btn-sm vote-btn " + ((int)Eval("UserVote")==1 ? "btn-success" : "btn-outline-success") %>'
                                                    onclick='castVote(this,<%# Eval("Id") %>,1)' title="Signal issue">
                                                    <i class="bi bi-hand-thumbs-up"></i>
                                                </button>
                                                <button type="button"
                                                    class='<%# "btn btn-sm vote-btn " + ((int)Eval("UserVote")==-1 ? "btn-danger" : "btn-outline-danger") %>'
                                                    onclick='castVote(this,<%# Eval("Id") %>,-1)' title="De-signal">
                                                    <i class="bi bi-hand-thumbs-down"></i>
                                                </button>
                                            </div>

                                            <div class="cat-name">
                                                <%# Eval("Name") %>
                                                    <%# (bool)Eval("IsHighRisk")
                                                        ? "<span class='tag-high-risk'>High Risk</span>" : "" %>
                                            </div>

                                            <!-- Health bar only — no raw numbers -->
                                            <div class="cat-bar-wrap">
                                                <div class="cat-bar"
                                                    style='<%# "width:" + Eval("BarWidth") + "%;background:" + Eval("BarColor") %>'>
                                                </div>
                                            </div>

                                            <div class="cat-tickets">
                                                <%# Eval("OpenTickets") %> open
                                            </div>
                                        </div>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Label ID="NoCatsLabel" runat="server" Visible="false" CssClass="mv-empty"
                                    style="font-size:.82rem;padding:8px 10px;" />
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>

                <asp:Panel ID="EmptyPanel" runat="server" Visible="false">
                    <div class="mv-empty">No modules found for this project.</div>
                </asp:Panel>
            </div>

            <!-- Sidebar: Recent Votes -->
            <div class="mv-sidebar">
                <div class="sidebar-card">
                    <div class="sidebar-card-header">
                        Recent Votes
                    </div>

                    <asp:Repeater ID="FeedRepeater" runat="server">
                        <ItemTemplate>
                            <div class="vote-feed-item">
                                <div
                                    class='<%# "vote-feed-avatar " + ((int)Eval("Value")==1 ? "avatar-up" : "avatar-down") %>'>
                                    <%# Eval("UserDisplay").ToString().Substring(0,1).ToUpper() %>
                                </div>
                                <div class="vote-feed-text">
                                    <strong>
                                        <%# Eval("UserDisplay") %>
                                    </strong>
                                    <%# (int)Eval("Value")==1 ? "signalled" : "de-signalled" %>
                                        <strong>
                                            <%# Eval("CategoryName") %>
                                        </strong>
                                        in <%# Eval("ModuleName") %>
                                </div>
                                <div class="vote-feed-time">
                                    <%# Eval("TimeAgo") %>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>

                    <asp:Panel ID="EmptyFeedPanel" runat="server" Visible="false">
                        <div class="mv-empty" style="font-size:.82rem;">No votes yet.</div>
                    </asp:Panel>
                </div>
            </div>
        </div>

        <!-- Create Issue modal -->
        <div class="modal fade" id="createIssueModal" tabindex="-1" aria-hidden="true">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header py-2">
                        <h5 class="modal-title">Create Issue</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <p class="text-muted small mb-3">Use the Kanban Board to create issues with full module +
                            category assignment.</p>
                        <a href="Kanban.aspx" class="btn btn-primary w-100">Go to Kanban Board</a>
                    </div>
                </div>
            </div>
        </div>

        <script type="text/javascript">

            // Tab filter
            function switchTab(tab) {
                ['top', 'low'].forEach(function (t) {
                    document.getElementById('tab-' + t).classList.toggle('active', t === tab);
                });
                document.querySelectorAll('.module-card').forEach(function (card) {
                    card.style.display = (card.dataset.tab === tab) ? '' : 'none';
                });
            }

            // Module expand/collapse
            function toggleModule(header) {
                var card = header.closest('.module-card');
                var panel = card.querySelector('.categories-panel');
                var chevron = header.querySelector('.module-chevron');
                var open = panel.classList.contains('open');
                panel.classList.toggle('open', !open);
                chevron.classList.toggle('open', !open);
            }

            // Category vote (AJAX)
            function castVote(btn, categoryId, value) {
                var row = btn.closest('.category-row');
                var upBtn = row.querySelector('.btn-outline-success, .btn-success');
                var downBtn = row.querySelector('.btn-outline-danger, .btn-danger');
                var barEl = row.querySelector('.cat-bar');

                fetch('CategoryVoteHandler.ashx', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                    body: 'categoryId=' + categoryId + '&value=' + value
                })
                    .then(function (r) { return r.json(); })
                    .then(function (data) {
                        if (data.error) { alert(data.error); return; }

                        var sc = parseFloat(data.stressScore);

                        // Update bar
                        var barPct = Math.min(sc / 1.5 * 100, 100);
                        barEl.style.width = Math.max(barPct, 2) + '%';
                        barEl.style.background = stressBarColor(sc);

                        // High-risk border
                        row.classList.toggle('high-risk', sc > 0.5);

                        // Button state — match Kanban pattern
                        var uv = data.userVote;
                        upBtn.className = 'btn btn-sm vote-btn ' + (uv === 1 ? 'btn-success' : 'btn-outline-success');
                        downBtn.className = 'btn btn-sm vote-btn ' + (uv === -1 ? 'btn-danger' : 'btn-outline-danger');
                        row.dataset.userVote = uv;
                    })
                    .catch(function (err) { alert('Vote failed: ' + err); });
            }

            function stressBarColor(sc) {
                if (sc > 0.5) return '#dc2626';
                if (sc > 0.25) return '#f59e0b';
                if (sc > 0) return '#16a34a';
                return '#9ca3af';
            }

            // Modal
            function showCreateIssueModal() {
                bootstrap.Modal.getOrCreateInstance(
                    document.getElementById('createIssueModal')
                ).show();
            }
        </script>
    </asp:Content>