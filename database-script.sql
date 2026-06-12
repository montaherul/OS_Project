IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [Roles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [Users] (
        [Id] nvarchar(450) NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [IsActive] bit NOT NULL,
        [ProfilePictureUrl] nvarchar(500) NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [RoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [SchedulingSessions] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NULL,
        [AlgorithmType] nvarchar(50) NOT NULL,
        [TimeQuantum] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [IsPreemptive] bit NOT NULL,
        [UserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SchedulingSessions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchedulingSessions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [UserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [UserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [UserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [UserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [ExecutionLogs] (
        [Id] int NOT NULL IDENTITY,
        [SchedulingSessionId] int NOT NULL,
        [TimeStep] int NOT NULL,
        [ExecutingProcessId] nvarchar(100) NOT NULL,
        [ExecutingProcessName] nvarchar(100) NOT NULL,
        [Action] nvarchar(50) NOT NULL,
        [Details] nvarchar(500) NULL,
        [QueueState] int NOT NULL,
        [ReadyQueueSnapshot] nvarchar(2000) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_ExecutionLogs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExecutionLogs_SchedulingSessions_SchedulingSessionId] FOREIGN KEY ([SchedulingSessionId]) REFERENCES [SchedulingSessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [Processes] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [ProcessId] nvarchar(50) NOT NULL,
        [ArrivalTime] int NOT NULL,
        [BurstTime] int NOT NULL,
        [Deadline] int NOT NULL,
        [Priority] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [SchedulingSessionId] int NOT NULL,
        [UserId] nvarchar(450) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Processes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Processes_SchedulingSessions_SchedulingSessionId] FOREIGN KEY ([SchedulingSessionId]) REFERENCES [SchedulingSessions] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Processes_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE TABLE [SchedulingResults] (
        [Id] int NOT NULL IDENTITY,
        [SchedulingSessionId] int NOT NULL,
        [ProcessId] nvarchar(100) NOT NULL,
        [ProcessName] nvarchar(100) NOT NULL,
        [ArrivalTime] int NOT NULL,
        [BurstTime] int NOT NULL,
        [Deadline] int NOT NULL,
        [CompletionTime] int NOT NULL,
        [TurnaroundTime] int NOT NULL,
        [WaitingTime] int NOT NULL,
        [ResponseTime] int NOT NULL,
        [IsMissedDeadline] bit NOT NULL,
        [StartTime] int NOT NULL,
        [EndTime] int NOT NULL,
        [GanttChartData] nvarchar(2000) NULL,
        [CpuUtilization] float NOT NULL,
        [Throughput] float NOT NULL,
        [ContextSwitchCount] int NOT NULL,
        [DeadlineMissRatio] float NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_SchedulingResults] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SchedulingResults_SchedulingSessions_SchedulingSessionId] FOREIGN KEY ([SchedulingSessionId]) REFERENCES [SchedulingSessions] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] ON;
    EXEC(N'INSERT INTO [Roles] ([Id], [ConcurrencyStamp], [Name], [NormalizedName])
    VALUES (N''admin-role-id-001'', N''admin-role-stamp'', N''Admin'', N''ADMIN''),
    (N''user-role-id-001'', N''user-role-stamp'', N''User'', N''USER'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ConcurrencyStamp', N'Name', N'NormalizedName') AND [object_id] = OBJECT_ID(N'[Roles]'))
        SET IDENTITY_INSERT [Roles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'CreatedAt', N'Email', N'EmailConfirmed', N'FirstName', N'IsActive', N'LastLoginAt', N'LastName', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'ProfilePictureUrl', N'SecurityStamp', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] ON;
    EXEC(N'INSERT INTO [Users] ([Id], [AccessFailedCount], [ConcurrencyStamp], [CreatedAt], [Email], [EmailConfirmed], [FirstName], [IsActive], [LastLoginAt], [LastName], [LockoutEnabled], [LockoutEnd], [NormalizedEmail], [NormalizedUserName], [PasswordHash], [PhoneNumber], [PhoneNumberConfirmed], [ProfilePictureUrl], [SecurityStamp], [TwoFactorEnabled], [UserName])
    VALUES (N''8e445865-e2ff-4350-84d0-4c83e07bf1f3'', 0, N''admin-concurrency-stamp'', ''2026-06-10T08:25:44.9993676Z'', N''admin@edfrr.com'', CAST(1 AS bit), N''Admin'', CAST(1 AS bit), NULL, N''User'', CAST(0 AS bit), NULL, N''ADMIN@EDFRR.COM'', N''ADMIN@EDFRR.COM'', N''AQAAAAIAAYagAAAAELtGVmMnKfBqKz0Vr3nL9qI1sH5Gx2y4a7b8c0d2e4f6g8h0i2j4k6l8m0n2o=='', NULL, CAST(0 AS bit), NULL, N''ADMIN_SECURITY_STAMP_001'', CAST(0 AS bit), N''admin@edfrr.com''),
    (N''a1b2c3d4-e5f6-7890-abcd-ef1234567890'', 0, N''user-concurrency-stamp'', ''2026-06-10T08:25:44.9993698Z'', N''user@edfrr.com'', CAST(1 AS bit), N''Test'', CAST(1 AS bit), NULL, N''User'', CAST(0 AS bit), NULL, N''USER@EDFRR.COM'', N''USER@EDFRR.COM'', N''AQAAAAIAAYagAAAAELtGVmMnKfBqKz0Vr3nL9qI1sH5Gx2y4a7b8c0d2e4f6g8h0i2j4k6l8m0n2o=='', NULL, CAST(0 AS bit), NULL, N''USER_SECURITY_STAMP_001'', CAST(0 AS bit), N''user@edfrr.com'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'AccessFailedCount', N'ConcurrencyStamp', N'CreatedAt', N'Email', N'EmailConfirmed', N'FirstName', N'IsActive', N'LastLoginAt', N'LastName', N'LockoutEnabled', N'LockoutEnd', N'NormalizedEmail', N'NormalizedUserName', N'PasswordHash', N'PhoneNumber', N'PhoneNumberConfirmed', N'ProfilePictureUrl', N'SecurityStamp', N'TwoFactorEnabled', N'UserName') AND [object_id] = OBJECT_ID(N'[Users]'))
        SET IDENTITY_INSERT [Users] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[UserRoles]'))
        SET IDENTITY_INSERT [UserRoles] ON;
    EXEC(N'INSERT INTO [UserRoles] ([RoleId], [UserId])
    VALUES (N''admin-role-id-001'', N''8e445865-e2ff-4350-84d0-4c83e07bf1f3''),
    (N''user-role-id-001'', N''a1b2c3d4-e5f6-7890-abcd-ef1234567890'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoleId', N'UserId') AND [object_id] = OBJECT_ID(N'[UserRoles]'))
        SET IDENTITY_INSERT [UserRoles] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_ExecutionLogs_SchedulingSessionId] ON [ExecutionLogs] ([SchedulingSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Processes_SchedulingSessionId_ProcessId] ON [Processes] ([SchedulingSessionId], [ProcessId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Processes_UserId] ON [Processes] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_RoleClaims_RoleId] ON [RoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SchedulingResults_SchedulingSessionId] ON [SchedulingResults] ([SchedulingSessionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_SchedulingSessions_UserId] ON [SchedulingSessions] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserClaims_UserId] ON [UserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserLogins_UserId] ON [UserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [Users] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260610082545_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260610082545_InitialCreate', N'8.0.6');
END;
GO

COMMIT;
GO

