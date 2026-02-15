using System;
using System.Web.UI;

namespace DevBoard
{
    public partial class _Default : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // Role-based redirect
            if (User.IsInRole("Dev"))
            {
                Response.Redirect("~/Kanban.aspx");
            }
            else if (User.IsInRole("QA") || User.IsInRole("Stakeholder"))
            {
                Response.Redirect("~/QADashboard.aspx");
            }
            else if (User.IsInRole("Admin"))
            {
                Response.Redirect("~/Projects.aspx");
            }
            // If no specific role, stay on this page
        }
    }
}
