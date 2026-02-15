using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace DevBoard
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.IsAuthenticated)
            {
                Response.Redirect("~/Default.aspx");
            }
        }

        protected void RegisterButton_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
                return;

            try
            {
                // Create user
                MembershipUser newUser = Membership.CreateUser(
                    EmailTextBox.Text,
                    PasswordTextBox.Text,
                    EmailTextBox.Text
                );

                if (newUser != null)
                {
                    // Add user to selected role
                    string selectedRole = RoleDropDown.SelectedValue;
                    if (!string.IsNullOrEmpty(selectedRole))
                    {
                        Roles.AddUserToRole(EmailTextBox.Text, selectedRole);
                    }

                    // Sign in and redirect
                    FormsAuthentication.SetAuthCookie(EmailTextBox.Text, false);
                    Response.Redirect("~/Default.aspx");
                }
            }
            catch (MembershipCreateUserException ex)
            {
                ErrorLabel.Text = GetErrorMessage(ex.StatusCode);
                ErrorLabel.Visible = true;
            }
            catch (Exception ex)
            {
                ErrorLabel.Text = "An error occurred: " + ex.Message;
                ErrorLabel.Visible = true;
            }
        }

        private string GetErrorMessage(MembershipCreateStatus status)
        {
            switch (status)
            {
                case MembershipCreateStatus.DuplicateUserName:
                case MembershipCreateStatus.DuplicateEmail:
                    return "An account with this email already exists.";
                case MembershipCreateStatus.InvalidPassword:
                    return "The password is invalid. Please use at least 6 characters.";
                case MembershipCreateStatus.InvalidEmail:
                    return "The email address is invalid.";
                case MembershipCreateStatus.InvalidUserName:
                    return "The username is invalid.";
                default:
                    return "An unknown error occurred. Please try again.";
            }
        }
    }
}
