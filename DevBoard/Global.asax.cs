using DevBoard.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace DevBoard
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            // Force load SQLite EF6 provider
            var _ = typeof(System.Data.SQLite.EF6.SQLiteProviderFactory).Name;
            System.Reflection.Assembly.Load("System.Data.SQLite.EF6");

            // Initialize database and seed data
            DatabaseInitializer.Initialize();
        }

        protected void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }

        protected void Session_Start(object sender, EventArgs e)
        {
            // Code that runs when a new session is started
        }

        protected void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends
        }
    }

    public class DatabaseInitializer
    {
        public static void Initialize()
        {
            // Disable EF initializer
            Database.SetInitializer<DevBoardContext>(null);

            string dbPath = HttpContext.Current.Server.MapPath("~/App_Data/DevBoard.db");
            if (!System.IO.File.Exists(dbPath))
            {
                // Create database file
                System.Data.SQLite.SQLiteConnection.CreateFile(dbPath);
            }

                // Execute schema script - split by GO or ; to ensure all commands run
                string schemaPath = HttpContext.Current.Server.MapPath("~/App_Data/schema.sql");
                string schemaSql = System.IO.File.ReadAllText(schemaPath);
                var commands = schemaSql.Split(new[] { "GO", ";" }, StringSplitOptions.RemoveEmptyEntries);

                using (var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    foreach (var cmdText in commands)
                    {
                        if (string.IsNullOrWhiteSpace(cmdText)) continue;
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(cmdText, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

            // Seed Application Data
            SeedApplicationData();

            // Initialize Membership database and seed data
            SeedMembershipData();
        }

        private static void SeedMembershipData()
        {
            // Create roles if they don't exist
            string[] roleNames = { "Admin", "Dev", "QA", "Stakeholder" };
            foreach (var roleName in roleNames)
            {
                if (!Roles.RoleExists(roleName))
                {
                    Roles.CreateRole(roleName);
                }
            }

            // Create users if they don't exist
            var users = new[]
            {
                new { Email = "admin@devboard.com", Password = "Dev@123", Role = "Admin" },
                new { Email = "dev@devboard.com", Password = "Dev@123", Role = "Dev" },
                new { Email = "qa@devboard.com", Password = "QA@123", Role = "QA" },
                new { Email = "stake@devboard.com", Password = "Stake@123", Role = "Stakeholder" }
            };

            foreach (var userData in users)
            {
                if (Membership.GetUser(userData.Email) == null)
                {
                    MembershipUser user = Membership.CreateUser(
                        userData.Email,
                        userData.Password,
                        userData.Email
                    );

                    if (user != null)
                    {
                        Roles.AddUserToRole(userData.Email, userData.Role);
                    }
                }
            }
        }

        private static void SeedApplicationData()
        {
            using (var context = new DevBoardContext())
            {
                // Check if data already exists
                if (context.Projects.Any())
                    return;

                // Seed Projects
                var project = new Project
                {
                    Name = "BillingService",
                    Description = "Core billing and payment processing service",
                    RepoUrl = "https://github.com/user/billing-service",
                    ConfigPath = "devboard.modules.json"
                };

                context.Projects.Add(project);
                context.SaveChanges();

                // Seed Modules
                var modules = new[]
                {
                    new Module { ProjectId = project.Id, Name = "Auth", Path = "src/Auth" },
                    new Module { ProjectId = project.Id, Name = "Payment", Path = "src/Payment" },
                    new Module { ProjectId = project.Id, Name = "Invoicing", Path = "src/Invoicing" },
                    new Module { ProjectId = project.Id, Name = "Reporting", Path = "src/Reporting" },
                    new Module { ProjectId = project.Id, Name = "API", Path = "src/API" }
                };

                context.Modules.AddRange(modules);
                context.SaveChanges();

                // Get user IDs (using email as userId for simplicity)
                var devUserId = "dev@devboard.com";
                var qaUserId = "qa@devboard.com";

                // Seed Tickets
                var tickets = new[]
                {
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[0].Id,
                        Title = "Implement OAuth2 authentication",
                        Description = "Add OAuth2 support for third-party authentication",
                        Type = TicketType.Feature,
                        Status = Status.InProgress,
                        Priority = Priority.High,
                        CreatedById = devUserId,
                        AssignedToId = devUserId,
                        MissingTests = true,
                        EstimatedTestEffort = TestEffort.Medium
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[1].Id,
                        Title = "Payment gateway timeout issues",
                        Description = "Intermittent timeouts when processing payments",
                        Type = TicketType.Bug,
                        Status = Status.Todo,
                        Priority = Priority.High,
                        CreatedById = qaUserId,
                        Flaky = true,
                        MissingTests = true,
                        EstimatedTestEffort = TestEffort.High
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[1].Id,
                        Title = "Add unit tests for payment processing",
                        Description = "Payment module lacks comprehensive test coverage",
                        Type = TicketType.QADebt,
                        Status = Status.Todo,
                        Priority = Priority.Medium,
                        CreatedById = qaUserId,
                        MissingTests = true,
                        ManualHeavy = true,
                        EstimatedTestEffort = TestEffort.High,
                        AffectedPaths = "src/Payment/PaymentProcessor.cs,src/Payment/PaymentValidator.cs"
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[2].Id,
                        Title = "Invoice PDF generation",
                        Description = "Generate PDF invoices for completed transactions",
                        Type = TicketType.Feature,
                        Status = Status.Done,
                        Priority = Priority.Medium,
                        CreatedById = devUserId,
                        AssignedToId = devUserId
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[3].Id,
                        Title = "Flaky integration tests in reporting module",
                        Description = "Tests fail randomly on CI/CD pipeline",
                        Type = TicketType.QADebt,
                        Status = Status.InProgress,
                        Priority = Priority.High,
                        CreatedById = qaUserId,
                        Flaky = true,
                        ManualHeavy = false,
                        EstimatedTestEffort = TestEffort.Medium
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[4].Id,
                        Title = "API rate limiting",
                        Description = "Implement rate limiting for API endpoints",
                        Type = TicketType.Feature,
                        Status = Status.Todo,
                        Priority = Priority.Medium,
                        CreatedById = devUserId,
                        MissingTests = true,
                        EstimatedTestEffort = TestEffort.Small
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[0].Id,
                        Title = "Missing integration tests for auth flow",
                        Description = "Auth module needs end-to-end test coverage",
                        Type = TicketType.QADebt,
                        Status = Status.Todo,
                        Priority = Priority.Low,
                        CreatedById = qaUserId,
                        MissingTests = true,
                        ManualHeavy = true,
                        EstimatedTestEffort = TestEffort.Medium
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[2].Id,
                        Title = "Update invoice template",
                        Description = "Refresh invoice design to match new branding",
                        Type = TicketType.Chore,
                        Status = Status.Done,
                        Priority = Priority.Low,
                        CreatedById = devUserId
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[3].Id,
                        Title = "Dashboard performance optimization",
                        Description = "Reporting dashboard loads slowly with large datasets",
                        Type = TicketType.Bug,
                        Status = Status.InProgress,
                        Priority = Priority.Medium,
                        CreatedById = devUserId,
                        AssignedToId = devUserId,
                        Flaky = false,
                        MissingTests = false
                    },
                    new Ticket
                    {
                        ProjectId = project.Id,
                        ModuleId = modules[1].Id,
                        Title = "Refund processing workflow",
                        Description = "Implement automated refund processing",
                        Type = TicketType.Feature,
                        Status = Status.Todo,
                        Priority = Priority.High,
                        CreatedById = devUserId,
                        MissingTests = true,
                        EstimatedTestEffort = TestEffort.High
                    }
                };

                context.Tickets.AddRange(tickets);
                context.SaveChanges();

                // Seed Votes
                var votes = new[]
                {
                    new TicketVote { TicketId = tickets[1].Id, UserId = devUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[1].Id, UserId = qaUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[2].Id, UserId = devUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[2].Id, UserId = qaUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[4].Id, UserId = devUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[4].Id, UserId = qaUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[0].Id, UserId = qaUserId, Value = 1 },
                    new TicketVote { TicketId = tickets[5].Id, UserId = qaUserId, Value = -1 }
                };

                context.TicketVotes.AddRange(votes);
                context.SaveChanges();
            }
        }
    }
}
