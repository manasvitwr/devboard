<%@ Page Title="Login" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="DevBoard.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row justify-content-center">
        <div class="col-md-6">
            <div class="card shadow">
                <div class="card-body">
                    <h2 class="card-title text-center mb-4">
                        <i class="bi bi-box-arrow-in-right"></i> Login to DevBoard
                    </h2>
                    
                    <asp:Login ID="LoginControl" runat="server" 
                        OnAuthenticate="LoginControl_Authenticate"
                        DestinationPageUrl="~/Default.aspx"
                        FailureText="Invalid username or password."
                        UserNameLabelText="Email:"
                        PasswordLabelText="Password:"
                        RememberMeText="Remember me"
                        LoginButtonText="Login"
                        CssClass="login-form">
                        <LayoutTemplate>
                            <div class="mb-3">
                                <asp:Label ID="UserNameLabel" runat="server" AssociatedControlID="UserName" CssClass="form-label">Email:</asp:Label>
                                <asp:TextBox ID="UserName" runat="server" CssClass="form-control" TextMode="Email"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="UserNameRequired" runat="server" 
                                    ControlToValidate="UserName" 
                                    ErrorMessage="Email is required." 
                                    CssClass="text-danger"
                                    Display="Dynamic">
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="mb-3">
                                <asp:Label ID="PasswordLabel" runat="server" AssociatedControlID="Password" CssClass="form-label">Password:</asp:Label>
                                <asp:TextBox ID="Password" runat="server" CssClass="form-control" TextMode="Password"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="PasswordRequired" runat="server" 
                                    ControlToValidate="Password" 
                                    ErrorMessage="Password is required." 
                                    CssClass="text-danger"
                                    Display="Dynamic">
                                </asp:RequiredFieldValidator>
                            </div>
                            <div class="mb-3 form-check">
                                <asp:CheckBox ID="RememberMe" runat="server" CssClass="form-check-input" />
                                <asp:Label ID="RememberMeLabel" runat="server" AssociatedControlID="RememberMe" CssClass="form-check-label">Remember me</asp:Label>
                            </div>
                            <asp:Literal ID="FailureText" runat="server" EnableViewState="False"></asp:Literal>
                            <div class="d-grid">
                                <asp:Button ID="LoginButton" runat="server" CommandName="Login" Text="Login" CssClass="btn btn-primary btn-lg" />
                            </div>
                        </LayoutTemplate>
                    </asp:Login>
                    
                    <div class="text-center mt-3">
                        <p>Don't have an account? <a href="Register.aspx">Register here</a></p>
                    </div>
                    
                    <div class="alert alert-info mt-4">
                        <strong>Demo Accounts:</strong><br />
                        Admin: admin@devboard.com / Dev@123<br />
                        Dev: dev@devboard.com / Dev@123<br />
                        QA: qa@devboard.com / QA@123<br />
                        Stakeholder: stake@devboard.com / Stake@123
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
