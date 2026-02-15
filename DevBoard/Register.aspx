<%@ Page Title="Register" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true"
    CodeBehind="Register.aspx.cs" Inherits="DevBoard.Register" %>

    <asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card shadow">
                    <div class="card-body">
                        <h2 class="card-title text-center mb-4">
                            <i class="bi bi-person-plus"></i> Create Account
                        </h2>

                        <div class="mb-3">
                            <label for="<%= EmailTextBox.ClientID %>" class="form-label">Email:</label>
                            <asp:TextBox ID="EmailTextBox" runat="server" CssClass="form-control" TextMode="Email">
                            </asp:TextBox>
                            <asp:RequiredFieldValidator ID="EmailRequired" runat="server"
                                ControlToValidate="EmailTextBox" ErrorMessage="Email is required."
                                CssClass="text-danger" Display="Dynamic">
                            </asp:RequiredFieldValidator>
                        </div>

                        <div class="mb-3">
                            <label for="<%= PasswordTextBox.ClientID %>" class="form-label">Password:</label>
                            <asp:TextBox ID="PasswordTextBox" runat="server" CssClass="form-control"
                                TextMode="Password"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="PasswordRequired" runat="server"
                                ControlToValidate="PasswordTextBox" ErrorMessage="Password is required."
                                CssClass="text-danger" Display="Dynamic">
                            </asp:RequiredFieldValidator>
                            <small class="form-text text-muted">Minimum 6 characters</small>
                        </div>

                        <div class="mb-3">
                            <label for="<%= ConfirmPasswordTextBox.ClientID %>" class="form-label">Confirm
                                Password:</label>
                            <asp:TextBox ID="ConfirmPasswordTextBox" runat="server" CssClass="form-control"
                                TextMode="Password"></asp:TextBox>
                            <asp:CompareValidator ID="PasswordCompare" runat="server"
                                ControlToValidate="ConfirmPasswordTextBox" ControlToCompare="PasswordTextBox"
                                ErrorMessage="Passwords do not match." CssClass="text-danger" Display="Dynamic">
                            </asp:CompareValidator>
                        </div>

                        <div class="mb-3">
                            <label for="<%= RoleDropDown.ClientID %>" class="form-label">Role:</label>
                            <asp:DropDownList ID="RoleDropDown" runat="server" CssClass="form-select">
                                <asp:ListItem Value="Dev" Selected="True">Developer</asp:ListItem>
                                <asp:ListItem Value="QA">QA Engineer</asp:ListItem>
                                <asp:ListItem Value="Stakeholder">Stakeholder</asp:ListItem>
                                <asp:ListItem Value="Admin">Administrator</asp:ListItem>
                            </asp:DropDownList>
                        </div>

                        <asp:Label ID="ErrorLabel" runat="server" CssClass="text-danger" Visible="false"></asp:Label>

                        <div class="d-grid">
                            <asp:Button ID="RegisterButton" runat="server" Text="Register"
                                CssClass="btn btn-primary btn-lg" OnClick="RegisterButton_Click" />
                        </div>

                        <div class="text-center mt-3">
                            <p>Already have an account? <a href="Login.aspx">Login here</a></p>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </asp:Content>