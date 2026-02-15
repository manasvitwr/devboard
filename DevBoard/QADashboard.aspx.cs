using DevBoard.Models;
using DevBoard.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DevBoard
{
    public partial class QADashboard : Page
    {
        private TicketService _ticketService;

        protected void Page_Load(object sender, EventArgs e)
        {
            _ticketService = new TicketService(new DevBoardContext());

            if (!IsPostBack)
            {
                LoadProjects();
                if (ProjectDropDown.Items.Count > 0)
                {
                    LoadDashboard();
                }
            }
        }

        private void LoadProjects()
        {
            using (var context = new DevBoardContext())
            {
                var projects = context.Projects.ToList();
                ProjectDropDown.DataSource = projects;
                ProjectDropDown.DataTextField = "Name";
                ProjectDropDown.DataValueField = "Id";
                ProjectDropDown.DataBind();
            }
        }

        private void LoadDashboard()
        {
            if (string.IsNullOrEmpty(ProjectDropDown.SelectedValue))
                return;

            int projectId = int.Parse(ProjectDropDown.SelectedValue);
            var tickets = _ticketService.GetTicketsByProject(projectId);

            // Summary stats
            TotalTicketsLabel.Text = tickets.Count.ToString();
            QADebtLabel.Text = tickets.Count(t => t.Type == TicketType.QADebt || t.Type == TicketType.Bug).ToString();
            FlakyLabel.Text = tickets.Count(t => t.Flaky).ToString();
            MissingTestsLabel.Text = tickets.Count(t => t.MissingTests).ToString();

            // Chart data
            var chartData = new
            {
                typeLabels = new[] { "Feature", "Bug", "QA Debt", "Chore" },
                types = new[]
                {
                    tickets.Count(t => t.Type == TicketType.Feature),
                    tickets.Count(t => t.Type == TicketType.Bug),
                    tickets.Count(t => t.Type == TicketType.QADebt),
                    tickets.Count(t => t.Type == TicketType.Chore)
                },
                statusLabels = new[] { "To Do", "In Progress", "Done" },
                statuses = new[]
                {
                    tickets.Count(t => t.Status == Status.Todo),
                    tickets.Count(t => t.Status == Status.InProgress),
                    tickets.Count(t => t.Status == Status.Done)
                }
            };

            ChartDataHidden.Value = JsonConvert.SerializeObject(chartData);

            // Pain scores
            LoadPainScores(projectId);
        }

        private void LoadPainScores(int projectId)
        {
            using (var context = new DevBoardContext())
            {
                var modules = context.Modules
                    .Where(m => m.ProjectId == projectId)
                    .Include(m => m.Tickets)
                    .Include(m => m.Tickets.Select(t => t.Votes))
                    .ToList();

                var painScoreData = modules.Select(module =>
                {
                    var openQADebtCount = module.Tickets.Count(t =>
                        (t.Type == TicketType.QADebt || t.Type == TicketType.Bug) &&
                        t.Status != Status.Done);

                    var flakyCount = module.Tickets.Count(t => t.Flaky);

                    var upvotesOnQADebt = module.Tickets
                        .Where(t => t.Type == TicketType.QADebt || t.Type == TicketType.Bug)
                        .SelectMany(t => t.Votes)
                        .Where(v => v.Value > 0)
                        .Sum(v => v.Value);

                    var painScore = (openQADebtCount * 2) + upvotesOnQADebt + (flakyCount * 3);

                    return new
                    {
                        ModuleName = module.Name,
                        OpenQADebt = openQADebtCount,
                        FlakyCount = flakyCount,
                        Upvotes = upvotesOnQADebt,
                        PainScore = painScore
                    };
                }).OrderByDescending(m => m.PainScore).ToList();

                PainScoreGridView.DataSource = painScoreData;
                PainScoreGridView.DataBind();
            }
        }

        protected void ProjectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        protected void PainScoreGridView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var painScore = int.Parse(e.Row.Cells[4].Text);

                // Heat coloring based on pain score
                if (painScore >= 15)
                {
                    e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(220, 53, 69); // Red
                    e.Row.Cells[4].ForeColor = System.Drawing.Color.White;
                }
                else if (painScore >= 8)
                {
                    e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(255, 193, 7); // Yellow
                }
                else if (painScore >= 3)
                {
                    e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(13, 202, 240); // Cyan
                }
                else
                {
                    e.Row.Cells[4].BackColor = System.Drawing.Color.FromArgb(25, 135, 84); // Green
                    e.Row.Cells[4].ForeColor = System.Drawing.Color.White;
                }
            }
        }
    }
}
