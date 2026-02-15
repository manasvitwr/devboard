using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DevBoard
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void LoginControl_Authenticate(object sender, AuthenticateEventArgs e)
        {
            var login = (System.Web.UI.WebControls.Login)sender;
            
            if (Membership.ValidateUser(login.UserName, login.Password))
            {
                FormsAuthentication.SetAuthCookie(login.UserName, login.RememberMeSet);
                e.Authenticated = true;
            }
            else
            {
                e.Authenticated = false;
            }
        }
    }
}
