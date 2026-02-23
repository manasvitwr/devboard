using DevBoard.Models;
using DevBoard.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DevBoard
{
    public partial class Kanban : Page
    {
        private TicketService _ticketService;
        private ProjectService _projectService;

        protected void Page_Load(object sender, EventArgs e)
        {
            var context = new DevBoardContext();
            _ticketService = new TicketService(context);
            _projectService = new ProjectService(context);

            if (!IsPostBack)
            {
                LoadProjects();

                string projectIdQuery = Request.QueryString["projectId"];
                if (!string.IsNullOrEmpty(projectIdQuery) && int.TryParse(projectIdQuery, out int projectId))
                {
                    var item = ProjectDropDown.Items.FindByValue(projectIdQuery);
                    if (item != null)
                    {
                        ProjectDropDown.SelectedValue = projectIdQuery;
                    }
                }

                if (ProjectDropDown.Items.Count > 0)
                {
                    LoadModules();
                }
            }

            // Always reload tickets (covers both initial load and post-save full postback)
            if (ProjectDropDown.Items.Count > 0)
            {
                LoadTickets();
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

        private void LoadModules()
        {
            if (string.IsNullOrEmpty(ProjectDropDown.SelectedValue)) return;
            int projectId = int.Parse(ProjectDropDown.SelectedValue);

            using (var context = new DevBoardContext())
            {
                var modules = context.Modules.Where(m => m.ProjectId == projectId).ToList();
                ModuleDropDown.DataSource = modules;
                ModuleDropDown.DataTextField = "Name";
                ModuleDropDown.DataValueField = "Id";
                ModuleDropDown.DataBind();
                ModuleDropDown.Items.Insert(0, new ListItem("-- Select Module --", ""));
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
            LoadModules();
        }

        protected void TicketRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "EditTicket")
            {
                int ticketId = int.Parse(e.CommandArgument.ToString());
                var ticket = _ticketService.GetTicketById(ticketId);
                if (ticket != null)
                {
                    TicketIdHidden.Value = ticket.Id.ToString();
                    TitleTextBox.Text = ticket.Title;
                    DescriptionTextBox.Text = ticket.Description;
                    TypeDropDown.SelectedValue = ((int)ticket.Type).ToString();
                    PriorityDropDown.SelectedValue = ((int)ticket.Priority).ToString();
                    StatusDropDown.SelectedValue = ((int)ticket.Status).ToString();

                    if (ticket.ModuleId.HasValue && ModuleDropDown.Items.FindByValue(ticket.ModuleId.Value.ToString()) != null)
                        ModuleDropDown.SelectedValue = ticket.ModuleId.Value.ToString();
                    else
                        ModuleDropDown.SelectedIndex = 0;

                    AssignToTextBox.Text = ticket.AssignedToId;
                    GitHubSyncCheckBox.Checked = false; // Reset sync checkbox on edit
                    
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowModal", "showEditTicketModal();", true);
                }
            }
        }

        protected void SaveTicketButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid) return;

            try
            {
                int projectId = int.Parse(ProjectDropDown.SelectedValue);
                int? moduleId = string.IsNullOrEmpty(ModuleDropDown.SelectedValue) ? (int?)null : int.Parse(ModuleDropDown.SelectedValue);
                
                if (string.IsNullOrEmpty(TicketIdHidden.Value))
                {
                    // Create New
                    var ticket = new Ticket
                    {
                        ProjectId = projectId,
                        Title = TitleTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        Type = (TicketType)int.Parse(TypeDropDown.SelectedValue),
                        Priority = (Priority)int.Parse(PriorityDropDown.SelectedValue),
                        Status = (Status)int.Parse(StatusDropDown.SelectedValue),
                        ModuleId = moduleId,
                        CreatedById = User.Identity.Name ?? "Anonymous", // Fallback if not auth
                        AssignedToId = AssignToTextBox.Text
                    };

                    _ticketService.CreateTicket(ticket);

                    if (GitHubSyncCheckBox.Checked)
                    {
                        try
                        {
                            // Run off the ASP.NET sync context to avoid InvalidOperationException
                            Task.Run(() => _projectService.CreateGitHubIssueAsync(projectId, ticket)).GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            // Ticket already saved — just warn about GitHub sync
                            ScriptManager.RegisterStartupScript(this, GetType(), "SyncError", $"alert('Ticket created but GitHub sync failed: {ex.Message.Replace("'", "\\'")}');", true);
                        }
                    }
                }
                else
                {
                    // Update Existing
                    int ticketId = int.Parse(TicketIdHidden.Value);
                    var ticket = _ticketService.GetTicketById(ticketId);
                    if (ticket != null)
                    {
                        ticket.Title = TitleTextBox.Text;
                        ticket.Description = DescriptionTextBox.Text;
                        ticket.Type = (TicketType)int.Parse(TypeDropDown.SelectedValue);
                        ticket.Priority = (Priority)int.Parse(PriorityDropDown.SelectedValue);
                        ticket.Status = (Status)int.Parse(StatusDropDown.SelectedValue);
                        ticket.ModuleId = moduleId;
                        ticket.AssignedToId = AssignToTextBox.Text;

                        _ticketService.UpdateTicket(ticket);
                    }
                }

                // Board is re-rendered via the full postback response
                LoadTickets();
            }
            catch (Exception ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "Error", $"alert('Error saving ticket: {ex.Message.Replace("'", "\\'")}');", true);
            }
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

        protected int GetUserVote(int ticketId)
        {
            if (!User.Identity.IsAuthenticated) return 0;
            return _ticketService.GetUserVote(ticketId, User.Identity.Name);
        }
    }
}
