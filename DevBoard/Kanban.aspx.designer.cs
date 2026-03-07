namespace DevBoard
{
    public partial class Kanban
    {
        protected global::System.Web.UI.WebControls.DropDownList ProjectDropDown;
        protected global::System.Web.UI.UpdatePanel KanbanUpdatePanel;
        protected global::System.Web.UI.WebControls.HiddenField TicketIdHidden;
        protected global::System.Web.UI.WebControls.HiddenField SelectedCategoryId;
        protected global::System.Web.UI.WebControls.HiddenField ModuleCategoriesJson;
        protected global::System.Web.UI.WebControls.TextBox TitleTextBox;
        protected global::System.Web.UI.WebControls.RequiredFieldValidator TitleValidator;
        protected global::System.Web.UI.WebControls.TextBox DescriptionTextBox;
        protected global::System.Web.UI.WebControls.DropDownList StatusDropDown;
        protected global::System.Web.UI.WebControls.DropDownList TypeDropDown;
        protected global::System.Web.UI.WebControls.DropDownList PriorityDropDown;
        protected global::System.Web.UI.WebControls.DropDownList ModuleDropDown;
        protected global::System.Web.UI.WebControls.DropDownList CategoryDropDown;
        protected global::System.Web.UI.WebControls.TextBox AssignToTextBox;
        protected global::System.Web.UI.WebControls.CheckBox GitHubSyncCheckBox;
        protected global::System.Web.UI.WebControls.Button SaveTicketButton;
        protected global::System.Web.UI.WebControls.Panel TodoPanel;
        protected global::System.Web.UI.WebControls.Panel InProgressPanel;
        protected global::System.Web.UI.WebControls.Panel DonePanel;
        protected global::System.Web.UI.WebControls.Repeater TodoRepeater;
        protected global::System.Web.UI.WebControls.Repeater InProgressRepeater;
        protected global::System.Web.UI.WebControls.Repeater DoneRepeater;
    }
}
