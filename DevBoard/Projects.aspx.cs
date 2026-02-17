using DevBoard.Models;
using DevBoard.Services;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DevBoard
{
    public partial class Projects : Page
    {
        private ProjectService _projectService;

        protected void Page_Load(object sender, EventArgs e)
        {
            _projectService = new ProjectService(new DevBoardContext());

            if (!IsPostBack)
            {
                BindProjects();
            }
        }

        private void BindProjects()
        {
            var projects = _projectService.GetAllProjects();
            ProjectsRepeater.DataSource = projects;
            ProjectsRepeater.DataBind();

            NoProjectsPanel.Visible = projects.Count == 0;
        }

        protected void ShowCreateButton_Click(object sender, EventArgs e)
        {
            CreatePanel.Visible = true;
            ProjectIdHidden.Value = "";
            NameTextBox.Text = "";
            DescriptionTextBox.Text = "";
            RepoUrlTextBox.Text = "";
            ConfigPathTextBox.Text = "devboard.modules.json";
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                if (string.IsNullOrEmpty(ProjectIdHidden.Value))
                {
                    // Create new project
                    var project = new Project
                    {
                        Name = NameTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        RepoUrl = RepoUrlTextBox.Text,
                        ConfigPath = ConfigPathTextBox.Text
                    };
                    _projectService.CreateProject(project);
                    ShowMessage("Project created successfully!", "alert-success");
                }
                else
                {
                    // Update existing project
                    var projectId = int.Parse(ProjectIdHidden.Value);
                    var project = _projectService.GetProjectById(projectId);
                    if (project != null)
                    {
                        project.Name = NameTextBox.Text;
                        project.Description = DescriptionTextBox.Text;
                        project.RepoUrl = RepoUrlTextBox.Text;
                        project.ConfigPath = ConfigPathTextBox.Text;
                        _projectService.UpdateProject(project);
                        ShowMessage("Project updated successfully!", "alert-success");
                    }
                }

                CreatePanel.Visible = false;
                BindProjects();
            }
            catch (Exception ex)
            {
                ShowMessage("Error: " + ex.Message, "alert-danger");
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            CreatePanel.Visible = false;
        }

        protected async void ProjectsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            int projectId = int.Parse(e.CommandArgument.ToString());

            if (e.CommandName == "Edit")
            {
                var project = _projectService.GetProjectById(projectId);
                if (project != null)
                {
                    CreatePanel.Visible = true;
                    ProjectIdHidden.Value = project.Id.ToString();
                    NameTextBox.Text = project.Name;
                    DescriptionTextBox.Text = project.Description;
                    RepoUrlTextBox.Text = project.RepoUrl;
                    ConfigPathTextBox.Text = project.ConfigPath;
                }
            }
            else if (e.CommandName == "Delete")
            {
                try
                {
                    _projectService.DeleteProject(projectId);
                    ShowMessage("Project deleted successfully!", "alert-success");
                    BindProjects();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error deleting project: " + ex.Message, "alert-danger");
                }
            }
            else if (e.CommandName == "Sync")
            {
                try
                {
                    await _projectService.SyncModulesFromGitHubAsync(projectId);
                    ShowMessage("Modules synced successfully from GitHub!", "alert-success");
                    BindProjects();
                }
                catch (Exception ex)
                {
                    ShowMessage("Error syncing modules: " + ex.Message, "alert-danger");
                }
            }
        }

        private void ShowMessage(string message, string cssClass)
        {
            MessageLabel.Text = message;
            MessageLabel.CssClass = "alert " + cssClass;
            MessageLabel.Visible = true;
        }
    }
}
