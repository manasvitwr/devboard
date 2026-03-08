<%@ Page Title="Analytics" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Analytics.aspx.cs" Inherits="DevBoard.Pages.Analytics" %>

    <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
        <style>
            /* Typography and Layout styles matching Module Voting */
            .analytics-layout {
                display: flex;
                flex-direction: column;
                gap: 24px;
            }

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

            /* Standardized Cards */
            .analytics-card {
                background: #fff;
                border: 1px solid #e0e0e0;
                border-radius: 10px;
                box-shadow: 0 2px 6px rgba(0, 0, 0, .04);
                padding: 24px;
                display: flex;
                flex-direction: column;
            }

            .analytics-card-title {
                font-size: 1.05rem;
                font-weight: 700;
                margin-bottom: 16px;
                color: #343a40;
                border-bottom: 1px solid #f1f5f9;
                padding-bottom: 12px;
            }

            /* Health Gauge */
            .health-gauge-container {
                display: flex;
                flex-direction: column;
                align-items: center;
                justify-content: center;
                padding: 20px 0;
            }

            .health-gauge-value {
                font-size: 4rem;
                font-weight: 800;
                line-height: 1;
            }

            .health-gauge-label {
                font-size: 1.1rem;
                font-weight: 600;
                color: #6c757d;
                margin-top: 8px;
            }

            /* Health Colors */
            .health-green {
                color: #15803d;
            }

            .health-yellow {
                color: #a16207;
            }

            .health-red {
                color: #dc2626;
            }

            /* Stress Map List */
            .stress-map-list {
                display: flex;
                flex-direction: column;
                gap: 16px;
            }

            .stress-item {
                background: #fafafa;
                border: 1px solid #f1f5f9;
                border-radius: 8px;
                padding: 16px;
            }

            .stress-item-header {
                display: flex;
                justify-content: space-between;
                align-items: center;
                margin-bottom: 12px;
            }

            .stress-item-title {
                font-size: 1rem;
                font-weight: 700;
                color: #1e293b;
            }

            .stress-item-health {
                font-weight: 700;
                font-size: 0.95rem;
                padding: 4px 10px;
                border-radius: 8px;
            }

            .bg-health-green {
                background: #dcfce7;
                color: #15803d;
            }

            .bg-health-yellow {
                background: #fef9c3;
                color: #a16207;
            }

            .bg-health-red {
                background: #fee2e2;
                color: #dc2626;
            }

            .failing-categories {
                display: flex;
                flex-direction: column;
                gap: 8px;
            }

            .failing-category-row {
                display: flex;
                justify-content: space-between;
                align-items: center;
                font-size: 0.85rem;
                background: #fff;
                padding: 8px 12px;
                border: 1px solid #e2e8f0;
                border-radius: 6px;
                border-left: 4px solid #f59e0b;
            }

            .failing-category-row.critical-risk {
                border-left-color: #dc2626;
            }

            .cat-name-box {
                font-weight: 600;
                color: #475569;
            }

            .cat-signals-box {
                font-weight: 700;
                color: #dc2626;
            }

            .charts-row {
                display: flex;
                gap: 24px;
            }

            @media (max-width:900px) {
                .charts-row {
                    flex-direction: column;
                }

                .chart-col {
                    width: 100%;
                }
            }

            .chart-col {
                flex: 1;
                min-width: 0;
            }
        </style>
    </asp:Content>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="mv-page-header">
            <div class="mv-page-title">
                Analytics
                <asp:DropDownList ID="ProjectDropDown" runat="server" CssClass="form-select form-select-sm"
                    style="width:auto;font-size:.8rem;font-weight:600;" AutoPostBack="true"
                    OnSelectedIndexChanged="ProjectDropDown_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
        </div>

        <div class="analytics-layout">
            <!-- Top Row: Overview & Stress Map -->
            <div class="row align-items-stretch" style="row-gap: 24px;">
                <div class="col-md-5 d-flex">
                    <div class="analytics-card w-100">
                        <div class="analytics-card-title">Project Health</div>
                        <div class="health-gauge-container flex-grow-1">
                            <asp:Label ID="ProjectHealthLabel" runat="server" CssClass="health-gauge-value" Text="100%">
                            </asp:Label>
                            <div class="health-gauge-label">
                                <asp:Label ID="ProjectHealthStatusLabel" runat="server" Text="Neutral / Stable">
                                </asp:Label>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="col-md-7 d-flex">
                    <div class="analytics-card w-100">
                        <div class="analytics-card-title">Stress Map (Top Unstable Modules)</div>
                        <div class="stress-map-list">
                            <asp:Repeater ID="StressMapRepeater" runat="server"
                                OnItemDataBound="StressMapRepeater_ItemDataBound">
                                <ItemTemplate>
                                    <div class="stress-item">
                                        <div class="stress-item-header">
                                            <div class="stress-item-title">
                                                <%# Eval("ModuleName") %>
                                            </div>
                                            <div class='<%# "stress-item-health " + Eval("HealthBgClass") %>'>
                                                <%# Eval("HealthPct") %>%
                                            </div>
                                        </div>
                                        <div class="failing-categories">
                                            <asp:Repeater ID="FailingCategoriesRepeater" runat="server">
                                                <ItemTemplate>
                                                    <div
                                                        class='<%# "failing-category-row " + ((bool)Eval("IsCriticalRisk") ? "critical-risk" : "") %>'>
                                                        <div class="cat-name-box">
                                                            <%# Eval("CategoryName") %>
                                                        </div>
                                                        <div class="cat-signals-box">
                                                            <%# Eval("Downvotes") %> Instability Signals
                                                        </div>
                                                    </div>
                                                </ItemTemplate>
                                            </asp:Repeater>
                                            <asp:Label ID="NoFailingLabel" runat="server" Visible="false"
                                                CssClass="text-muted" style="font-size:0.85rem;"
                                                Text="No failing categories." />
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                            <asp:Panel ID="EmptyStressMapPanel" runat="server" Visible="false">
                                <div class="text-center text-muted py-4">All modules are stable.</div>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Charts Row -->
            <div class="charts-row">
                <div class="chart-col">
                    <div class="analytics-card">
                        <div class="analytics-card-title">Tickets by Category</div>
                        <div style="position: relative; height: 300px; width: 100%;">
                            <canvas id="ticketsChart"></canvas>
                        </div>
                    </div>
                </div>
                <div class="chart-col">
                    <div class="analytics-card">
                        <div class="analytics-card-title">Vote Sentiment Breakdown</div>
                        <div style="position: relative; height: 300px; width: 100%;">
                            <canvas id="votesChart"></canvas>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <asp:HiddenField ID="ChartDataHidden" runat="server" />

        <script type="text/javascript">
            document.addEventListener('DOMContentLoaded', function () {
                var rawData = document.getElementById('<%= ChartDataHidden.ClientID %>').value;
                if (!rawData) return;
                var chartData = JSON.parse(rawData);

                // 1. Tickets by Category (Stacked Bar Chart or simple Bar Chart)
                if (chartData.categories && chartData.categories.length > 0) {
                    var ctxTickets = document.getElementById('ticketsChart').getContext('2d');
                    new Chart(ctxTickets, {
                        type: 'bar',
                        data: {
                            labels: chartData.categories,
                            datasets: [
                                {
                                    label: 'To Do',
                                    data: chartData.todoTickets,
                                    backgroundColor: '#6c757d',
                                    borderRadius: 4
                                },
                                {
                                    label: 'In Progress',
                                    data: chartData.inProgressTickets,
                                    backgroundColor: '#0d6efd',
                                    borderRadius: 4
                                },
                                {
                                    label: 'Done',
                                    data: chartData.doneTickets,
                                    backgroundColor: '#198754',
                                    borderRadius: 4
                                }
                            ]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {
                                legend: {
                                    display: true,
                                    position: 'bottom',
                                    labels: {
                                        usePointStyle: true,
                                        boxWidth: 8
                                    }
                                }
                            },
                            scales: {
                                x: { stacked: true, grid: { display: false } },
                                y: { stacked: true, beginAtZero: true, border: { display: false } }
                            }
                        }
                    });
                } else {
                    var canvas = document.getElementById('ticketsChart');
                    var ctx = canvas.getContext('2d');
                    ctx.font = '14px Inter, sans-serif';
                    ctx.fillStyle = '#9ca3af';
                    ctx.textAlign = 'center';
                    ctx.fillText('No ticket data', canvas.width / 2, canvas.height / 2);
                }

                // 2. Vote Sentiment (Doughnut)
                var ctxVotes = document.getElementById('votesChart').getContext('2d');
                if (chartData.totalUpvotes > 0 || chartData.totalDownvotes > 0) {
                    new Chart(ctxVotes, {
                        type: 'doughnut',
                        data: {
                            labels: ['Stability Upvotes', 'Instability Downvotes'],
                            datasets: [{
                                data: [chartData.totalUpvotes, chartData.totalDownvotes],
                                backgroundColor: ['#16a34a', '#dc2626'],
                                borderWidth: 0,
                                hoverOffset: 4
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            cutout: '70%',
                            plugins: {
                                legend: {
                                    display: true,
                                    position: 'bottom',
                                    labels: {
                                        usePointStyle: true,
                                        boxWidth: 8
                                    }
                                }
                            }
                        }
                    });
                } else {
                    new Chart(ctxVotes, {
                        type: 'doughnut',
                        data: {
                            labels: ['No Votes'],
                            datasets: [{
                                data: [1],
                                backgroundColor: ['#e2e8f0'],
                                borderWidth: 0
                            }]
                        },
                        options: {
                            responsive: true,
                            maintainAspectRatio: false,
                            cutout: '70%',
                            plugins: { tooltip: { enabled: false }, legend: { display: false } }
                        }
                    });
                    var c = document.getElementById('votesChart');
                    var ctxxt = c.getContext('2d');
                    ctxxt.font = '14px Inter, sans-serif';
                    ctxxt.fillStyle = '#64748b';
                    ctxxt.textAlign = 'center';
                    ctxxt.fillText('No Votes Cast', c.width / 2, c.height / 2);
                }
            });
        </script>
    </asp:Content>