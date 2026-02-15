-- Identity Tables
CREATE TABLE IF NOT EXISTS [Users] (
    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Username] NVARCHAR(128) NOT NULL UNIQUE,
    [Email] NVARCHAR(256),
    [PasswordHash] NVARCHAR(128) NOT NULL,
    [PasswordSalt] NVARCHAR(128) NOT NULL,
    [IsApproved] BOOLEAN NOT NULL DEFAULT 1,
    [CreatedDate] DATETIME NOT NULL
);

CREATE TABLE IF NOT EXISTS [Roles] (
    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [RoleName] NVARCHAR(128) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS [UsersInRoles] (
    [UserId] INTEGER NOT NULL,
    [RoleId] INTEGER NOT NULL,
    PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UsersInRoles_User] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UsersInRoles_Role] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
);

-- Projects Table
CREATE TABLE IF NOT EXISTS [Project] (
    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [Name] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(1024),
    [RepoUrl] NVARCHAR(512),
    [ConfigPath] NVARCHAR(255)
);

-- Modules Table
CREATE TABLE IF NOT EXISTS [Module] (
    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [ProjectId] INTEGER NOT NULL,
    [Name] NVARCHAR(255) NOT NULL,
    [Path] NVARCHAR(512),
    CONSTRAINT [FK_Module_Project] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]) ON DELETE CASCADE
);

-- Tickets Table
CREATE TABLE IF NOT EXISTS [Ticket] (
    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [ProjectId] INTEGER NOT NULL,
    [ModuleId] INTEGER,
    [Title] NVARCHAR(255) NOT NULL,
    [Description] NVARCHAR(2048),
    [Type] INTEGER NOT NULL,
    [Status] INTEGER NOT NULL,
    [Priority] INTEGER NOT NULL,
    [CreatedById] NVARCHAR(128),
    [AssignedToId] NVARCHAR(128),
    [CreatedAt] DATETIME NOT NULL,
    [UpdatedAt] DATETIME NOT NULL,
    [Flaky] BOOLEAN NOT NULL DEFAULT 0,
    [MissingTests] BOOLEAN NOT NULL DEFAULT 0,
    [ManualHeavy] BOOLEAN NOT NULL DEFAULT 0,
    [EstimatedTestEffort] INTEGER NOT NULL DEFAULT 0,
    [AffectedPaths] NVARCHAR(1024),
    CONSTRAINT [FK_Ticket_Project] FOREIGN KEY ([ProjectId]) REFERENCES [Project] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Ticket_Module] FOREIGN KEY ([ModuleId]) REFERENCES [Module] ([Id])
);

-- TicketVotes Table
CREATE TABLE IF NOT EXISTS [TicketVote] (
    [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    [TicketId] INTEGER NOT NULL,
    [UserId] NVARCHAR(128) NOT NULL,
    [Value] INTEGER NOT NULL,
    CONSTRAINT [FK_TicketVote_Ticket] FOREIGN KEY ([TicketId]) REFERENCES [Ticket] ([Id]) ON DELETE CASCADE
);

-- Indexes
CREATE INDEX IF NOT EXISTS [IX_Module_ProjectId] ON [Module] ([ProjectId]);
CREATE INDEX IF NOT EXISTS [IX_Ticket_ProjectId] ON [Ticket] ([ProjectId]);
CREATE INDEX IF NOT EXISTS [IX_Ticket_ModuleId] ON [Ticket] ([ModuleId]);
CREATE INDEX IF NOT EXISTS [IX_TicketVote_TicketId] ON [TicketVote] ([TicketId]);
CREATE UNIQUE INDEX IF NOT EXISTS [IX_TicketVote_TicketId_UserId] ON [TicketVote] ([TicketId], [UserId]);
