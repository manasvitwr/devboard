using DevBoard.Core.Models;
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

        protected void Application_End(object sender, EventArgs e) { }
        protected void Application_Error(object sender, EventArgs e) { }
        protected void Session_Start(object sender, EventArgs e) { }
        protected void Session_End(object sender, EventArgs e) { }
    }

    public class DatabaseInitializer
    {
        public static void Initialize()
        {
            Database.SetInitializer<DevBoardContext>(null);

            string dbPath = HttpContext.Current.Server.MapPath("~/App_Data/DevBoard.db");

            if (System.IO.File.Exists(dbPath) && !IsValidSQLiteFile(dbPath))
                System.IO.File.Delete(dbPath);

            // Run schema if any required table is missing (CREATE IF NOT EXISTS = safe/idempotent)
            if (!System.IO.File.Exists(dbPath) || !CheckSchemaExists(dbPath))
                ApplySchema(dbPath);

            SeedApplicationData();
            SeedMembershipData();
        }

        private static bool IsValidSQLiteFile(string dbPath)
        {
            try
            {
                byte[] header = new byte[16];
                using (var fs = System.IO.File.OpenRead(dbPath))
                {
                    if (fs.Length < 100) return false;
                    fs.Read(header, 0, 16);
                }
                return System.Text.Encoding.ASCII.GetString(header).StartsWith("SQLite format 3");
            }
            catch { return false; }
        }

        private static void ApplySchema(string dbPath)
        {
            string schemaPath = HttpContext.Current.Server.MapPath("~/App_Data/schema.sql");
            string schemaSql = System.IO.File.ReadAllText(schemaPath);
            var commands = schemaSql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            using (var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath};Version=3;"))
            {
                conn.Open();
                foreach (var cmdText in commands)
                {
                    var trimmed = cmdText.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed)) continue;
                    using (var cmd = new System.Data.SQLite.SQLiteCommand(trimmed, conn))
                        cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Returns false if Project table OR CategoryVote table is missing — triggers schema re-run.
        /// Since all statements use CREATE IF NOT EXISTS, re-running is fully idempotent.
        /// </summary>
        private static bool CheckSchemaExists(string dbPath)
        {
            try
            {
                using (var conn = new System.Data.SQLite.SQLiteConnection($"Data Source={dbPath};Version=3;"))
                {
                    conn.Open();
                    string[] requiredTables = { "Project", "CategoryVote" };
                    foreach (var table in requiredTables)
                    {
                        using (var cmd = new System.Data.SQLite.SQLiteCommand(
                            $"SELECT name FROM sqlite_master WHERE type='table' AND name='{table}';", conn))
                        {
                            var result = cmd.ExecuteScalar();
                            if (result == null || result.ToString() != table) return false;
                        }
                    }
                    return true;
                }
            }
            catch { return false; }
        }

        private static void SeedMembershipData()
        {
            string[] roleNames = { "Admin", "Dev", "QA", "Stakeholder" };
            foreach (var roleName in roleNames)
                if (!Roles.RoleExists(roleName)) Roles.CreateRole(roleName);

            var users = new[]
            {
                new { Email = "admin@devboard.com", Password = "Dev@123",   Role = "Admin" },
                new { Email = "dev@devboard.com",   Password = "Dev@123",   Role = "Dev" },
                new { Email = "qa@devboard.com",    Password = "QA@123",    Role = "QA" },
                new { Email = "stake@devboard.com", Password = "Stake@123", Role = "Stakeholder" }
            };

            foreach (var u in users)
            {
                if (Membership.GetUser(u.Email) == null)
                {
                    var user = Membership.CreateUser(u.Email, u.Password, u.Email);
                    if (user != null) Roles.AddUserToRole(u.Email, u.Role);
                }
            }
        }

        private static void SeedApplicationData()
        {
            using (var ctx = new DevBoardContext())
            {
                if (ctx.Projects.Any()) return;

                // ── Project ──────────────────────────────────────────────────────────
                var project = new Project
                {
                    Name = "BillingService",
                    Description = "Core billing and payment processing service",
                    RepoUrl = "https://github.com/user/billing-service",
                    ConfigPath = "devboard.modules.json"
                };
                ctx.Projects.Add(project);
                ctx.SaveChanges();

                // ── Modules ──────────────────────────────────────────────────────────
                var mAuth = new Module { ProjectId = project.Id, Name = "Authentication",       Path = "src/Auth",      IsCritical = true,  Description = "Login, session, OAuth flows" };
                var mPay  = new Module { ProjectId = project.Id, Name = "Payment Processing",   Path = "src/Payment",   IsCritical = true,  Description = "Stripe, refunds, webhooks" };
                var mInv  = new Module { ProjectId = project.Id, Name = "Invoicing",            Path = "src/Invoicing", IsCritical = false, Description = "PDF generation, email delivery" };
                var mRep  = new Module { ProjectId = project.Id, Name = "Reporting",            Path = "src/Reporting", IsCritical = false, Description = "Analytics, exports, dashboards" };
                var mApi  = new Module { ProjectId = project.Id, Name = "API Gateway",          Path = "src/API",       IsCritical = true,  Description = "Rate limiting, routing, auth headers" };

                ctx.Modules.AddRange(new[] { mAuth, mPay, mInv, mRep, mApi });
                ctx.SaveChanges();

                // ── Categories ───────────────────────────────────────────────────────
                // Auth
                var cAuthCrash   = new Category { ModuleId = mAuth.Id, Name = "Crash",               SeverityMultiplier = 3.0m, BaseScore = 100m };
                var cAuthSec     = new Category { ModuleId = mAuth.Id, Name = "Security",             SeverityMultiplier = 2.5m, BaseScore = 100m };
                var cAuthPerf    = new Category { ModuleId = mAuth.Id, Name = "Performance",          SeverityMultiplier = 1.5m, BaseScore = 100m };
                var cAuthLogic   = new Category { ModuleId = mAuth.Id, Name = "Logic Errors",         SeverityMultiplier = 2.0m, BaseScore = 100m };

                // Payment
                var cPayCrash    = new Category { ModuleId = mPay.Id,  Name = "Crash",               SeverityMultiplier = 3.0m, BaseScore = 100m };
                var cPayTxFail   = new Category { ModuleId = mPay.Id,  Name = "Transaction Failures", SeverityMultiplier = 3.0m, BaseScore = 100m };
                var cPayPerf     = new Category { ModuleId = mPay.Id,  Name = "Performance",          SeverityMultiplier = 2.0m, BaseScore = 100m };
                var cPayUX       = new Category { ModuleId = mPay.Id,  Name = "UX",                   SeverityMultiplier = 1.0m, BaseScore = 100m };

                // Invoicing
                var cInvCrash    = new Category { ModuleId = mInv.Id,  Name = "Crash",               SeverityMultiplier = 2.0m, BaseScore = 100m };
                var cInvPDF      = new Category { ModuleId = mInv.Id,  Name = "PDF Generation",       SeverityMultiplier = 1.5m, BaseScore = 100m };
                var cInvUX       = new Category { ModuleId = mInv.Id,  Name = "UX",                   SeverityMultiplier = 1.0m, BaseScore = 100m };

                // Reporting
                var cRepPerf     = new Category { ModuleId = mRep.Id,  Name = "Performance",          SeverityMultiplier = 2.0m, BaseScore = 100m };
                var cRepData     = new Category { ModuleId = mRep.Id,  Name = "Data Accuracy",        SeverityMultiplier = 2.5m, BaseScore = 100m };
                var cRepUX       = new Category { ModuleId = mRep.Id,  Name = "UX",                   SeverityMultiplier = 1.0m, BaseScore = 100m };

                // API
                var cApiCrash    = new Category { ModuleId = mApi.Id,  Name = "Crash",               SeverityMultiplier = 2.5m, BaseScore = 100m };
                var cApiRate     = new Category { ModuleId = mApi.Id,  Name = "Rate Limiting",        SeverityMultiplier = 2.0m, BaseScore = 100m };
                var cApiDocs     = new Category { ModuleId = mApi.Id,  Name = "Documentation",        SeverityMultiplier = 0.5m, BaseScore = 100m };

                ctx.Categories.AddRange(new[] {
                    cAuthCrash, cAuthSec, cAuthPerf, cAuthLogic,
                    cPayCrash, cPayTxFail, cPayPerf, cPayUX,
                    cInvCrash, cInvPDF, cInvUX,
                    cRepPerf, cRepData, cRepUX,
                    cApiCrash, cApiRate, cApiDocs
                });
                ctx.SaveChanges();

                // ── Tickets ──────────────────────────────────────────────────────────
                var dev = "dev@devboard.com";
                var qa  = "qa@devboard.com";

                var tickets = new[]
                {
                    // Auth — Crash
                    new Ticket { ProjectId=project.Id, ModuleId=mAuth.Id, CategoryId=cAuthCrash.Id,
                        Title="OAuth2 callback causes 500 on token expiry", Type=TicketType.Bug, Status=Status.InProgress,
                        Priority=Priority.High, CreatedById=qa, MissingTests=true, Flaky=true, EstimatedTestEffort=TestEffort.High },
                    // Auth — Security
                    new Ticket { ProjectId=project.Id, ModuleId=mAuth.Id, CategoryId=cAuthSec.Id,
                        Title="JWT secret hardcoded in config", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.High, CreatedById=qa, MissingTests=true, EstimatedTestEffort=TestEffort.Medium },
                    // Auth — Performance
                    new Ticket { ProjectId=project.Id, ModuleId=mAuth.Id, CategoryId=cAuthPerf.Id,
                        Title="Login page takes 3s on cold start", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.Medium, CreatedById=dev, EstimatedTestEffort=TestEffort.Small },
                    // Auth — Logic
                    new Ticket { ProjectId=project.Id, ModuleId=mAuth.Id, CategoryId=cAuthLogic.Id,
                        Title="MFA bypass via remember-me cookie", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.High, CreatedById=qa, MissingTests=true, EstimatedTestEffort=TestEffort.High },

                    // Payment — Crash
                    new Ticket { ProjectId=project.Id, ModuleId=mPay.Id, CategoryId=cPayCrash.Id,
                        Title="Stripe webhook handler throws NullRef on dispute", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.High, CreatedById=qa, Flaky=false, MissingTests=true, EstimatedTestEffort=TestEffort.High },
                    // Payment — Transaction
                    new Ticket { ProjectId=project.Id, ModuleId=mPay.Id, CategoryId=cPayTxFail.Id,
                        Title="Duplicate charge on network retry", Type=TicketType.Bug, Status=Status.InProgress,
                        Priority=Priority.High, CreatedById=qa, MissingTests=true, EstimatedTestEffort=TestEffort.High },
                    new Ticket { ProjectId=project.Id, ModuleId=mPay.Id, CategoryId=cPayTxFail.Id,
                        Title="Refund not reflected in statement until next cycle", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.Medium, CreatedById=dev, EstimatedTestEffort=TestEffort.Medium },
                    // Payment — Perf
                    new Ticket { ProjectId=project.Id, ModuleId=mPay.Id, CategoryId=cPayPerf.Id,
                        Title="Payment gateway timeout after 5s under load", Type=TicketType.Bug, Status=Status.InProgress,
                        Priority=Priority.High, CreatedById=qa, Flaky=true, EstimatedTestEffort=TestEffort.Medium },

                    // Invoicing — PDF
                    new Ticket { ProjectId=project.Id, ModuleId=mInv.Id, CategoryId=cInvPDF.Id,
                        Title="PDF invoice corrupted for orders > 50 line items", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.Medium, CreatedById=dev, MissingTests=true, EstimatedTestEffort=TestEffort.Medium },
                    new Ticket { ProjectId=project.Id, ModuleId=mInv.Id, CategoryId=cInvPDF.Id,
                        Title="Add unit tests for PDF generation", Type=TicketType.QADebt, Status=Status.Todo,
                        Priority=Priority.Medium, CreatedById=qa, MissingTests=true, ManualHeavy=true, EstimatedTestEffort=TestEffort.High },
                    // Invoicing — UX
                    new Ticket { ProjectId=project.Id, ModuleId=mInv.Id, CategoryId=cInvUX.Id,
                        Title="Invoice email template refresh for new branding", Type=TicketType.Chore, Status=Status.Done,
                        Priority=Priority.Low, CreatedById=dev },

                    // Reporting — Perf
                    new Ticket { ProjectId=project.Id, ModuleId=mRep.Id, CategoryId=cRepPerf.Id,
                        Title="Flaky integration tests in reporting module", Type=TicketType.QADebt, Status=Status.InProgress,
                        Priority=Priority.High, CreatedById=qa, Flaky=true, EstimatedTestEffort=TestEffort.Medium },
                    new Ticket { ProjectId=project.Id, ModuleId=mRep.Id, CategoryId=cRepPerf.Id,
                        Title="Dashboard loads slowly with large dataset (>100k rows)", Type=TicketType.Bug, Status=Status.InProgress,
                        Priority=Priority.Medium, CreatedById=dev, EstimatedTestEffort=TestEffort.Medium },
                    // Reporting — Data
                    new Ticket { ProjectId=project.Id, ModuleId=mRep.Id, CategoryId=cRepData.Id,
                        Title="Monthly revenue total off by rounding error", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.High, CreatedById=qa, MissingTests=true, EstimatedTestEffort=TestEffort.Medium },

                    // API — Crash
                    new Ticket { ProjectId=project.Id, ModuleId=mApi.Id, CategoryId=cApiCrash.Id,
                        Title="Gateway panics on malformed Content-Type header", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.High, CreatedById=qa, MissingTests=true, EstimatedTestEffort=TestEffort.Medium },
                    // API — Rate Limit
                    new Ticket { ProjectId=project.Id, ModuleId=mApi.Id, CategoryId=cApiRate.Id,
                        Title="Implement per-key rate limiting", Type=TicketType.Feature, Status=Status.Todo,
                        Priority=Priority.Medium, CreatedById=dev, MissingTests=true, EstimatedTestEffort=TestEffort.Small },
                    new Ticket { ProjectId=project.Id, ModuleId=mApi.Id, CategoryId=cApiRate.Id,
                        Title="Rate limit headers missing in 429 response", Type=TicketType.Bug, Status=Status.Todo,
                        Priority=Priority.Low, CreatedById=dev, EstimatedTestEffort=TestEffort.Small },
                };

                ctx.Tickets.AddRange(tickets);
                ctx.SaveChanges();

                // ── TicketVotes ───────────────────────────────────────────────────────
                var ticketVotes = new[]
                {
                    new TicketVote { TicketId=tickets[0].Id, UserId=dev, Value=1 },
                    new TicketVote { TicketId=tickets[0].Id, UserId=qa,  Value=1 },
                    new TicketVote { TicketId=tickets[1].Id, UserId=qa,  Value=1 },
                    new TicketVote { TicketId=tickets[4].Id, UserId=dev, Value=1 },
                    new TicketVote { TicketId=tickets[4].Id, UserId=qa,  Value=1 },
                    new TicketVote { TicketId=tickets[5].Id, UserId=qa,  Value=1 },
                    new TicketVote { TicketId=tickets[7].Id, UserId=dev, Value=1 },
                    new TicketVote { TicketId=tickets[7].Id, UserId=qa,  Value=1 },
                    new TicketVote { TicketId=tickets[13].Id, UserId=qa, Value=1 },
                };
                ctx.TicketVotes.AddRange(ticketVotes);
                ctx.SaveChanges();

                // ── CategoryVotes ─────────────────────────────────────────────────────
                // Simulate a realistic spread: security/crash categories get lots of upvotes,
                // docs/ux get mostly neutral or down. The admin and stake users also vote.
                var admin = "admin@devboard.com";
                var stake = "stake@devboard.com";
                var now   = DateTime.UtcNow;

                var catVotes = new[]
                {
                    // Auth — Crash: highly upvoted (critical, known issues)
                    new CategoryVote { CategoryId=cAuthCrash.Id, UserId=dev,   Value=1,  CreatedAt=now.AddHours(-5) },
                    new CategoryVote { CategoryId=cAuthCrash.Id, UserId=qa,    Value=1,  CreatedAt=now.AddHours(-4) },
                    new CategoryVote { CategoryId=cAuthCrash.Id, UserId=admin, Value=1,  CreatedAt=now.AddHours(-3) },
                    // Auth — Security: polarised
                    new CategoryVote { CategoryId=cAuthSec.Id,   UserId=dev,   Value=1,  CreatedAt=now.AddHours(-6) },
                    new CategoryVote { CategoryId=cAuthSec.Id,   UserId=qa,    Value=1,  CreatedAt=now.AddHours(-5) },
                    new CategoryVote { CategoryId=cAuthSec.Id,   UserId=stake, Value=1,  CreatedAt=now.AddHours(-2) },
                    // Auth — Performance: mixed
                    new CategoryVote { CategoryId=cAuthPerf.Id,  UserId=dev,   Value=1,  CreatedAt=now.AddHours(-8) },
                    new CategoryVote { CategoryId=cAuthPerf.Id,  UserId=qa,    Value=-1, CreatedAt=now.AddHours(-7) },
                    // Auth — Logic: moderate upvotes
                    new CategoryVote { CategoryId=cAuthLogic.Id, UserId=qa,    Value=1,  CreatedAt=now.AddHours(-3) },
                    new CategoryVote { CategoryId=cAuthLogic.Id, UserId=admin, Value=1,  CreatedAt=now.AddHours(-2) },

                    // Payment — Crash: everyone alarmed
                    new CategoryVote { CategoryId=cPayCrash.Id,  UserId=dev,   Value=1,  CreatedAt=now.AddHours(-10) },
                    new CategoryVote { CategoryId=cPayCrash.Id,  UserId=qa,    Value=1,  CreatedAt=now.AddHours(-9) },
                    new CategoryVote { CategoryId=cPayCrash.Id,  UserId=admin, Value=1,  CreatedAt=now.AddHours(-8) },
                    new CategoryVote { CategoryId=cPayCrash.Id,  UserId=stake, Value=1,  CreatedAt=now.AddHours(-7) },
                    // Payment — Transaction Failures: extremely urgent
                    new CategoryVote { CategoryId=cPayTxFail.Id, UserId=dev,   Value=1,  CreatedAt=now.AddHours(-12) },
                    new CategoryVote { CategoryId=cPayTxFail.Id, UserId=qa,    Value=1,  CreatedAt=now.AddHours(-11) },
                    new CategoryVote { CategoryId=cPayTxFail.Id, UserId=admin, Value=1,  CreatedAt=now.AddHours(-10) },
                    new CategoryVote { CategoryId=cPayTxFail.Id, UserId=stake, Value=1,  CreatedAt=now.AddHours(-9) },
                    // Payment — Performance
                    new CategoryVote { CategoryId=cPayPerf.Id,   UserId=dev,   Value=1,  CreatedAt=now.AddHours(-6) },
                    new CategoryVote { CategoryId=cPayPerf.Id,   UserId=qa,    Value=1,  CreatedAt=now.AddHours(-5) },
                    // Payment — UX: low priority
                    new CategoryVote { CategoryId=cPayUX.Id,     UserId=dev,   Value=-1, CreatedAt=now.AddHours(-4) },

                    // Invoicing — Crash
                    new CategoryVote { CategoryId=cInvCrash.Id,  UserId=qa,    Value=1,  CreatedAt=now.AddHours(-15) },
                    new CategoryVote { CategoryId=cInvCrash.Id,  UserId=dev,   Value=1,  CreatedAt=now.AddHours(-14) },
                    // Invoicing — PDF
                    new CategoryVote { CategoryId=cInvPDF.Id,    UserId=dev,   Value=1,  CreatedAt=now.AddHours(-13) },
                    new CategoryVote { CategoryId=cInvPDF.Id,    UserId=qa,    Value=-1, CreatedAt=now.AddHours(-12) },
                    // Invoicing — UX: mostly downvoted (low priority)
                    new CategoryVote { CategoryId=cInvUX.Id,     UserId=stake, Value=-1, CreatedAt=now.AddHours(-11) },

                    // Reporting — Perf
                    new CategoryVote { CategoryId=cRepPerf.Id,   UserId=dev,   Value=1,  CreatedAt=now.AddHours(-20) },
                    new CategoryVote { CategoryId=cRepPerf.Id,   UserId=qa,    Value=1,  CreatedAt=now.AddHours(-19) },
                    // Reporting — Data Accuracy
                    new CategoryVote { CategoryId=cRepData.Id,   UserId=qa,    Value=1,  CreatedAt=now.AddHours(-18) },
                    new CategoryVote { CategoryId=cRepData.Id,   UserId=admin, Value=1,  CreatedAt=now.AddHours(-17) },
                    new CategoryVote { CategoryId=cRepData.Id,   UserId=stake, Value=1,  CreatedAt=now.AddHours(-16) },
                    // Reporting — UX
                    new CategoryVote { CategoryId=cRepUX.Id,     UserId=dev,   Value=-1, CreatedAt=now.AddHours(-10) },

                    // API — Crash
                    new CategoryVote { CategoryId=cApiCrash.Id,  UserId=dev,   Value=1,  CreatedAt=now.AddHours(-8) },
                    new CategoryVote { CategoryId=cApiCrash.Id,  UserId=qa,    Value=1,  CreatedAt=now.AddHours(-7) },
                    new CategoryVote { CategoryId=cApiCrash.Id,  UserId=admin, Value=1,  CreatedAt=now.AddHours(-6) },
                    // API — Rate Limiting
                    new CategoryVote { CategoryId=cApiRate.Id,   UserId=dev,   Value=1,  CreatedAt=now.AddHours(-5) },
                    new CategoryVote { CategoryId=cApiRate.Id,   UserId=qa,    Value=1,  CreatedAt=now.AddHours(-4) },
                    // API — Docs: very low priority
                    new CategoryVote { CategoryId=cApiDocs.Id,   UserId=dev,   Value=-1, CreatedAt=now.AddHours(-3) },
                    new CategoryVote { CategoryId=cApiDocs.Id,   UserId=stake, Value=-1, CreatedAt=now.AddHours(-2) },
                };

                ctx.CategoryVotes.AddRange(catVotes);
                ctx.SaveChanges();
            }
        }
    }
}
