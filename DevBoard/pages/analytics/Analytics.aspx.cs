using DevBoard.Core.Models;
using DevBoard.Core.Services;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DevBoard.Pages
{
    public partial class Analytics : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
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
                var projects = context.Projects.OrderBy(p => p.Name).ToList();
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

            using (var ctx = new DevBoardContext())
            {
                var service = new AnalyticsService(ctx);
                var dto = service.GetDashboardData(projectId);

                ProjectHealthLabel.Text = dto.ProjectHealthPct.ToString() + "%";
                ProjectHealthStatusLabel.Text = dto.ProjectHealthStatus;
                ProjectHealthLabel.CssClass = dto.ProjectHealthCssClass;

                if (dto.TopUnstableModules.Any())
                {
                    StressMapRepeater.DataSource = dto.TopUnstableModules;
                    StressMapRepeater.DataBind();
                    StressMapRepeater.Visible = true;
                    EmptyStressMapPanel.Visible = false;
                }
                else
                {
                    StressMapRepeater.Visible = false;
                    EmptyStressMapPanel.Visible = true;
                }

                var chartData = new
                {
                    categories = dto.CategoryLabels,
                    todoTickets = dto.TodoTickets,
                    inProgressTickets = dto.InProgressTickets,
                    doneTickets = dto.DoneTickets,
                    totalUpvotes = dto.TotalUpvotes,
                    totalDownvotes = dto.TotalDownvotes
                };

                ChartDataHidden.Value = JsonConvert.SerializeObject(chartData);
            }
        }

        protected void ProjectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadDashboard();
        }

        protected void StressMapRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var dto = e.Item.DataItem as StressMapDto;
                var childRepeater = e.Item.FindControl("FailingCategoriesRepeater") as Repeater;
                var noFailing = e.Item.FindControl("NoFailingLabel") as Label;

                if (dto.FailingCategories != null && dto.FailingCategories.Any())
                {
                    childRepeater.DataSource = dto.FailingCategories;
                    childRepeater.DataBind();
                    childRepeater.Visible = true;
                    if (noFailing != null) noFailing.Visible = false;
                }
                else
                {
                    childRepeater.Visible = false;
                    if (noFailing != null) noFailing.Visible = true;
                }
            }
        }
    }
}
