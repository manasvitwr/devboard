using DevBoard.Models;
using DevBoard.Services;
using System;
using System.Linq;
using System.Web.UI;

namespace DevBoard
{
    public partial class Kanban : Page
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
                    LoadTickets();
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

        private void LoadTickets()
        {
            if (string.IsNullOrEmpty(ProjectDropDown.SelectedValue))
                return;

            int projectId = int.Parse(ProjectDropDown.SelectedValue);
            var tickets = _ticketService.GetTicketsByProject(projectId);

            TodoRepeater.DataSource = tickets.Where(t => t.Status == Status.Todo).ToList();
            TodoRepeater.DataBind();

            InProgressRepeater.DataSource = tickets.Where(t => t.Status == Status.InProgress).ToList();
            InProgressRepeater.DataBind();

            DoneRepeater.DataSource = tickets.Where(t => t.Status == Status.Done).ToList();
            DoneRepeater.DataBind();
        }

        protected void ProjectDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadTickets();
        }

        protected string GetTypeBadgeColor(TicketType type)
        {
            switch (type)
            {
                case TicketType.Feature: return "primary";
                case TicketType.Bug: return "danger";
                case TicketType.QADebt: return "warning";
                case TicketType.Chore: return "secondary";
                default: return "secondary";
            }
        }

        protected string GetPriorityBadgeColor(Priority priority)
        {
            switch (priority)
            {
                case Priority.High: return "danger";
                case Priority.Medium: return "warning";
                case Priority.Low: return "info";
                default: return "secondary";
            }
        }

        protected int GetTicketScore(int ticketId)
        {
            return _ticketService.GetTicketScore(ticketId);
        }
    }
}
