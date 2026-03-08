using System;
using System.Web.UI;

namespace DevBoard.Pages
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Role-based redirect
            if (User.IsInRole("Dev") || User.IsInRole("QA"))
            {
                Response.Redirect("~/pages/kanban/Kanban.aspx");
            }
            else if (User.IsInRole("Stakeholder"))
            {
                Response.Redirect("~/pages/analytics/Analytics.aspx");
            }
            else
            {
                Response.Redirect("~/pages/projects/Projects.aspx");
            }
            // If no specific role, stay on this page
        }
    }
}
