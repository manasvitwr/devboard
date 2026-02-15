<%@ Page Title="QA Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="QADashboard.aspx.cs" Inherits="DevBoard.QADashboard" %>

    <asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
        <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    </asp:Content>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="d-flex justify-content-between align-items-center mb-3">
            <h1><i class="bi bi-graph-up"></i> QA Dashboard</h1>
            <div>
                <label for="<%= ProjectDropDown.ClientID %>" class="me-2">Project:</label>
                <asp:DropDownList ID="ProjectDropDown" runat="server" CssClass="form-select d-inline-block w-auto"
                    AutoPostBack="true" OnSelectedIndexChanged="ProjectDropDown_SelectedIndexChanged">
                </asp:DropDownList>
            </div>
        </div>

        <div class="row mb-4">
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <h5 class="card-title">Total Tickets</h5>
                        <h2 class="text-primary">
                            <asp:Label ID="TotalTicketsLabel" runat="server" Text="0"></asp:Label>
                        </h2>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <h5 class="card-title">QA Debt</h5>
                        <h2 class="text-warning">
                            <asp:Label ID="QADebtLabel" runat="server" Text="0"></asp:Label>
                        </h2>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <h5 class="card-title">Flaky Tests</h5>
                        <h2 class="text-danger">
                            <asp:Label ID="FlakyLabel" runat="server" Text="0"></asp:Label>
                        </h2>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <h5 class="card-title">Missing Tests</h5>
                        <h2 class="text-info">
                            <asp:Label ID="MissingTestsLabel" runat="server" Text="0"></asp:Label>
                        </h2>
                    </div>
                </div>
            </div>
        </div>

        <div class="row mb-4">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Tickets by Type</h5>
                        <canvas id="typeChart"></canvas>
                    </div>
                </div>
            </div>
            <div class="col-md-6">
                <div class="card">
                    <div class="card-body">
                        <h5 class="card-title">Tickets by Status</h5>
                        <canvas id="statusChart"></canvas>
                    </div>
                </div>
            </div>
        </div>

        <h3 class="mb-3">Module Pain Scores</h3>
        <asp:GridView ID="PainScoreGridView" runat="server" CssClass="table table-striped table-hover"
            AutoGenerateColumns="false" OnRowDataBound="PainScoreGridView_RowDataBound">
            <Columns>
                <asp:BoundField DataField="ModuleName" HeaderText="Module" />
                <asp:BoundField DataField="OpenQADebt" HeaderText="Open QA Debt" />
                <asp:BoundField DataField="FlakyCount" HeaderText="Flaky Tests" />
                <asp:BoundField DataField="Upvotes" HeaderText="Upvotes on QA Debt" />
                <asp:BoundField DataField="PainScore" HeaderText="Pain Score" />
            </Columns>
            <EmptyDataTemplate>
                <div class="alert alert-info">No modules found for this project.</div>
            </EmptyDataTemplate>
        </asp:GridView>

        <asp:HiddenField ID="ChartDataHidden" runat="server" />

        <script type="text/javascript">
            document.addEventListener('DOMContentLoaded', function () {
                var chartData = JSON.parse(document.getElementById('<%= ChartDataHidden.ClientID %>').value || '{}');

                if (chartData.types && chartData.types.length > 0) {
                    new Chart(document.getElementById('typeChart'), {
                        type: 'doughnut',
                        data: {
                            labels: chartData.typeLabels,
                            datasets: [{
                                data: chartData.types,
                                backgroundColor: ['#0d6efd', '#dc3545', '#ffc107', '#6c757d']
                            }]
                        }
                    });
                }

                if (chartData.statuses && chartData.statuses.length > 0) {
                    new Chart(document.getElementById('statusChart'), {
                        type: 'bar',
                        data: {
                            labels: chartData.statusLabels,
                            datasets: [{
                                label: 'Tickets',
                                data: chartData.statuses,
                                backgroundColor: ['#6c757d', '#0d6efd', '#198754']
                            }]
                        },
                        options: {
                            scales: {
                                y: {
                                    beginAtZero: true
                                }
                            }
                        }
                    });
                }
            });
        </script>
    </asp:Content>