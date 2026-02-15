using DevBoard.Services;
using System;
using System.Linq;
using System.Web.UI;

namespace DevBoard
{
    public partial class Modules : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int projectId;
                if (int.TryParse(Request.QueryString["projectId"], out projectId))
                {
                    using (var context = new DevBoardContext())
                    {
                        var projectService = new ProjectService(context);
                        var project = projectService.GetProjectById(projectId);

                        if (project != null)
                        {
                            ProjectNameLabel.Text = project.Name;
                            var modules = context.Modules.Where(m => m.ProjectId == projectId).ToList();
                            ModulesGridView.DataSource = modules;
                            ModulesGridView.DataBind();
                        }
                        else
                        {
                            Response.Redirect("~/Projects.aspx");
                        }
                    }
                }
                else
                {
                    Response.Redirect("~/Projects.aspx");
                }
            }
        }
    }
}
