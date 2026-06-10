-- ============================================================
-- ERDRR Admin Dashboard - SQL Server Stored Procedures
-- ============================================================

-- 1. sp_GetUsersPaged - Paginated user listing
IF OBJECT_ID('dbo.sp_GetUsersPaged', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUsersPaged;
GO
CREATE PROCEDURE dbo.sp_GetUsersPaged
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(200) = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @RoleFilter NVARCHAR(50) = NULL,
    @StatusFilter NVARCHAR(20) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH UserCTE AS (
        SELECT
            u.Id AS UserId,
            u.UserName,
            u.Email,
            u.FirstName,
            u.LastName,
            u.CreatedAt,
            u.LastLoginAt,
            u.IsActive,
            u.LockoutEnd,
            u.AccessFailedCount,
            u.PhoneNumber,
            STRING_AGG(r.Name, ', ') AS Roles,
            CASE
                WHEN u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE() THEN 'Locked'
                WHEN u.IsActive = 0 THEN 'Inactive'
                ELSE 'Active'
            END AS Status,
            ROW_NUMBER() OVER (
                ORDER BY
                    CASE WHEN @SortColumn = 'UserName' AND @SortDirection = 'ASC' THEN u.UserName END ASC,
                    CASE WHEN @SortColumn = 'UserName' AND @SortDirection = 'DESC' THEN u.UserName END DESC,
                    CASE WHEN @SortColumn = 'Email' AND @SortDirection = 'ASC' THEN u.Email END ASC,
                    CASE WHEN @SortColumn = 'Email' AND @SortDirection = 'DESC' THEN u.Email END DESC,
                    CASE WHEN @SortColumn = 'CreatedAt' AND @SortDirection = 'ASC' THEN u.CreatedAt END ASC,
                    CASE WHEN @SortColumn = 'CreatedAt' AND @SortDirection = 'DESC' THEN u.CreatedAt END DESC,
                    CASE WHEN @SortColumn = 'LastLoginAt' AND @SortDirection = 'ASC' THEN u.LastLoginAt END ASC,
                    CASE WHEN @SortColumn = 'LastLoginAt' AND @SortDirection = 'DESC' THEN u.LastLoginAt END DESC,
                    u.CreatedAt DESC
            ) AS RowNum
        FROM [Users] u
        LEFT JOIN [UserRoles] ur ON u.Id = ur.UserId
        LEFT JOIN [Roles] r ON ur.RoleId = r.Id
        WHERE
            (@SearchTerm IS NULL OR @SearchTerm = ''
                OR u.UserName LIKE '%' + @SearchTerm + '%'
                OR u.Email LIKE '%' + @SearchTerm + '%'
                OR u.FirstName LIKE '%' + @SearchTerm + '%'
                OR u.LastName LIKE '%' + @SearchTerm + '%')
            AND (@RoleFilter IS NULL OR @RoleFilter = '' OR r.Name = @RoleFilter)
            AND (@StatusFilter IS NULL OR @StatusFilter = ''
                OR (@StatusFilter = 'Active' AND u.LockoutEnd IS NULL AND u.IsActive = 1)
                OR (@StatusFilter = 'Inactive' AND u.IsActive = 0)
                OR (@StatusFilter = 'Locked' AND u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE()))
        GROUP BY
            u.Id, u.UserName, u.Email, u.FirstName, u.LastName,
            u.CreatedAt, u.LastLoginAt, u.IsActive, u.LockoutEnd,
            u.AccessFailedCount, u.PhoneNumber
    )
    SELECT
        UserId, UserName, Email, FirstName, LastName,
        CreatedAt, LastLoginAt, IsActive, LockoutEnd,
        AccessFailedCount, PhoneNumber, Roles, Status
    FROM UserCTE
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
    ORDER BY RowNum;

    -- Total count
    SELECT COUNT(DISTINCT u.Id) AS TotalCount
    FROM [Users] u
    LEFT JOIN [UserRoles] ur ON u.Id = ur.UserId
    LEFT JOIN [Roles] r ON ur.RoleId = r.Id
    WHERE
        (@SearchTerm IS NULL OR @SearchTerm = ''
            OR u.UserName LIKE '%' + @SearchTerm + '%'
            OR u.Email LIKE '%' + @SearchTerm + '%'
            OR u.FirstName LIKE '%' + @SearchTerm + '%'
            OR u.LastName LIKE '%' + @SearchTerm + '%')
        AND (@RoleFilter IS NULL OR @RoleFilter = '' OR r.Name = @RoleFilter)
        AND (@StatusFilter IS NULL OR @StatusFilter = ''
            OR (@StatusFilter = 'Active' AND u.LockoutEnd IS NULL AND u.IsActive = 1)
            OR (@StatusFilter = 'Inactive' AND u.IsActive = 0)
            OR (@StatusFilter = 'Locked' AND u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE()));
END
GO

-- 2. sp_GetUserDetails - Full user details with stats
IF OBJECT_ID('dbo.sp_GetUserDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetUserDetails;
GO
CREATE PROCEDURE dbo.sp_GetUserDetails
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        u.Id AS UserId,
        u.UserName,
        u.Email,
        u.FirstName,
        u.LastName,
        u.CreatedAt,
        u.LastLoginAt,
        u.IsActive,
        u.LockoutEnd,
        u.AccessFailedCount,
        u.PhoneNumber,
        u.EmailConfirmed,
        u.PhoneNumberConfirmed,
        STRING_AGG(r.Name, ', ') AS Roles,
        CASE
            WHEN u.LockoutEnd IS NOT NULL AND u.LockoutEnd > GETUTCDATE() THEN 'Locked'
            WHEN u.IsActive = 0 THEN 'Inactive'
            ELSE 'Active'
        END AS Status,
        (SELECT COUNT(*) FROM [SchedulingSessions] WHERE UserId = u.Id AND IsDeleted = 0) AS TotalSessions,
        (SELECT COUNT(*) FROM [Processes] WHERE UserId = u.Id AND IsDeleted = 0) AS TotalProcesses,
        (SELECT COUNT(*) FROM [SchedulingResults] WHERE SchedulingSessionId IN
            (SELECT Id FROM [SchedulingSessions] WHERE UserId = u.Id AND IsDeleted = 0)) AS TotalSimulations,
        (SELECT COUNT(*) FROM [AlgorithmComparisons] WHERE UserId = u.Id AND IsDeleted = 0) AS TotalComparisons
    FROM [Users] u
    LEFT JOIN [UserRoles] ur ON u.Id = ur.UserId
    LEFT JOIN [Roles] r ON ur.RoleId = r.Id
    WHERE u.Id = @UserId
    GROUP BY
        u.Id, u.UserName, u.Email, u.FirstName, u.LastName,
        u.CreatedAt, u.LastLoginAt, u.IsActive, u.LockoutEnd,
        u.AccessFailedCount, u.PhoneNumber, u.EmailConfirmed, u.PhoneNumberConfirmed;

    -- Recent activities
    SELECT Activity, Details, Timestamp FROM (
        SELECT 'Session Created' AS Activity, Name AS Details, CreatedAt AS Timestamp
        FROM [SchedulingSessions]
        WHERE UserId = @UserId AND IsDeleted = 0
        UNION ALL
        SELECT 'Process Created' AS Activity, Name AS Details, CreatedAt AS Timestamp
        FROM [Processes]
        WHERE UserId = @UserId AND IsDeleted = 0
    ) AS Activities
    ORDER BY Timestamp DESC;
END
GO

-- 3. sp_GetDashboardStats - Admin dashboard statistics (enhanced)
IF OBJECT_ID('dbo.sp_GetDashboardStats', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetDashboardStats;
GO
CREATE PROCEDURE dbo.sp_GetDashboardStats
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        (SELECT COUNT(*) FROM [Users]) AS TotalUsers,
        (SELECT COUNT(*) FROM [Users] WHERE LockoutEnd IS NULL OR LockoutEnd <= GETUTCDATE()) AS ActiveUsers,
        (SELECT COUNT(*) FROM [Users] u INNER JOIN [UserRoles] ur ON u.Id = ur.UserId INNER JOIN [Roles] r ON ur.RoleId = r.Id WHERE r.Name = 'Admin') AS AdminCount,
        (SELECT COUNT(*) FROM [Users] u INNER JOIN [UserRoles] ur ON u.Id = ur.UserId INNER JOIN [Roles] r ON ur.RoleId = r.Id WHERE r.Name = 'User') AS UserCount,
        (SELECT COUNT(*) FROM [Users] WHERE CAST(CreatedAt AS DATE) = CAST(GETUTCDATE() AS DATE)) AS TodayLogins,
        (SELECT COUNT(*) FROM [Processes] WHERE IsDeleted = 0) AS TotalProcesses,
        (SELECT COUNT(*) FROM [SchedulingSessions] WHERE IsDeleted = 0) AS TotalSessions,
        (SELECT COUNT(*) FROM [SchedulingResults]) AS TotalSimulations,
        (SELECT COUNT(*) FROM [AlgorithmComparisons] WHERE IsDeleted = 0) AS TotalComparisons,
        0 AS TotalReports;

    -- User registrations per month (last 12 months)
    SELECT
        FORMAT(CreatedAt, 'yyyy-MM') AS MonthLabel,
        COUNT(*) AS Count
    FROM [Users]
    WHERE CreatedAt >= DATEADD(MONTH, -12, GETUTCDATE())
    GROUP BY FORMAT(CreatedAt, 'yyyy-MM')
    ORDER BY MonthLabel;

    -- Algorithm usage
    SELECT
        AlgorithmType,
        COUNT(*) AS Count
    FROM [SchedulingSessions]
    WHERE IsDeleted = 0
    GROUP BY AlgorithmType
    ORDER BY Count DESC;

    -- Process creation trends (last 30 days)
    SELECT
        CAST(CreatedAt AS DATE) AS Day,
        COUNT(*) AS Count
    FROM [Processes]
    WHERE CreatedAt >= DATEADD(DAY, -30, GETUTCDATE()) AND IsDeleted = 0
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY Day;

    -- Session creation trends (last 30 days)
    SELECT
        CAST(CreatedAt AS DATE) AS Day,
        COUNT(*) AS Count
    FROM [SchedulingSessions]
    WHERE CreatedAt >= DATEADD(DAY, -30, GETUTCDATE()) AND IsDeleted = 0
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY Day;

    -- Simulation activity (last 30 days)
    SELECT
        CAST(ss.CreatedAt AS DATE) AS Day,
        COUNT(*) AS Count
    FROM [SchedulingResults] sr
    INNER JOIN [SchedulingSessions] ss ON sr.SchedulingSessionId = ss.Id
    WHERE ss.CreatedAt >= DATEADD(DAY, -30, GETUTCDATE())
    GROUP BY CAST(ss.CreatedAt AS DATE)
    ORDER BY Day;

    -- Top users by process count
    SELECT TOP 10
        u.Id AS UserId,
        u.UserName,
        u.FirstName,
        u.LastName,
        COUNT(p.Id) AS ProcessCount
    FROM [Users] u
    INNER JOIN [Processes] p ON u.Id = p.UserId AND p.IsDeleted = 0
    GROUP BY u.Id, u.UserName, u.FirstName, u.LastName
    ORDER BY ProcessCount DESC;
END
GO

-- 4. sp_UpdateUser - Update user fields
IF OBJECT_ID('dbo.sp_UpdateUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UpdateUser;
GO
CREATE PROCEDURE dbo.sp_UpdateUser
    @UserId NVARCHAR(450),
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(256),
    @PhoneNumber NVARCHAR(20) = NULL,
    @IsActive BIT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Users]
    SET
        FirstName = @FirstName,
        LastName = @LastName,
        Email = @Email,
        NormalizedEmail = UPPER(@Email),
        UserName = @Email,
        NormalizedUserName = UPPER(@Email),
        PhoneNumber = @PhoneNumber,
        IsActive = @IsActive
    WHERE Id = @UserId;
END
GO

-- 5. sp_DeleteUser - Soft delete user
IF OBJECT_ID('dbo.sp_DeleteUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DeleteUser;
GO
CREATE PROCEDURE dbo.sp_DeleteUser
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Users]
    SET
        IsActive = 0,
        LockoutEnd = DATEADD(YEAR, 100, GETUTCDATE())
    WHERE Id = @UserId;
END
GO

-- 6. sp_LockUser
IF OBJECT_ID('dbo.sp_LockUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_LockUser;
GO
CREATE PROCEDURE dbo.sp_LockUser
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Users]
    SET
        LockoutEnd = DATEADD(YEAR, 100, GETUTCDATE()),
        LockoutEnabled = 1
    WHERE Id = @UserId;
END
GO

-- 7. sp_UnlockUser
IF OBJECT_ID('dbo.sp_UnlockUser', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_UnlockUser;
GO
CREATE PROCEDURE dbo.sp_UnlockUser
    @UserId NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Users]
    SET
        LockoutEnd = NULL,
        AccessFailedCount = 0,
        LockoutEnabled = 1
    WHERE Id = @UserId;
END
GO

-- 8. sp_BulkDeleteUsers
IF OBJECT_ID('dbo.sp_BulkDeleteUsers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BulkDeleteUsers;
GO
CREATE PROCEDURE dbo.sp_BulkDeleteUsers
    @UserIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ids TABLE (UserId NVARCHAR(450));

    INSERT INTO @Ids (UserId)
    SELECT value FROM STRING_SPLIT(@UserIds, ',');

    UPDATE [Users]
    SET
        IsActive = 0,
        LockoutEnd = DATEADD(YEAR, 100, GETUTCDATE())
    WHERE Id IN (SELECT UserId FROM @Ids);
END
GO

-- 9. sp_BulkLockUsers
IF OBJECT_ID('dbo.sp_BulkLockUsers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BulkLockUsers;
GO
CREATE PROCEDURE dbo.sp_BulkLockUsers
    @UserIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ids TABLE (UserId NVARCHAR(450));

    INSERT INTO @Ids (UserId)
    SELECT value FROM STRING_SPLIT(@UserIds, ',');

    UPDATE [Users]
    SET
        LockoutEnd = DATEADD(YEAR, 100, GETUTCDATE()),
        LockoutEnabled = 1
    WHERE Id IN (SELECT UserId FROM @Ids);
END
GO

-- 10. sp_BulkUnlockUsers
IF OBJECT_ID('dbo.sp_BulkUnlockUsers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BulkUnlockUsers;
GO
CREATE PROCEDURE dbo.sp_BulkUnlockUsers
    @UserIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ids TABLE (UserId NVARCHAR(450));

    INSERT INTO @Ids (UserId)
    SELECT value FROM STRING_SPLIT(@UserIds, ',');

    UPDATE [Users]
    SET
        LockoutEnd = NULL,
        AccessFailedCount = 0,
        LockoutEnabled = 1
    WHERE Id IN (SELECT UserId FROM @Ids);
END
GO

-- 11. sp_GetRecentUsers - Latest registered users with role
IF OBJECT_ID('dbo.sp_GetRecentUsers', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetRecentUsers;
GO
CREATE PROCEDURE dbo.sp_GetRecentUsers
    @Count INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        u.Id AS UserId,
        u.UserName,
        u.Email,
        u.FirstName,
        u.LastName,
        ISNULL(STRING_AGG(r.Name, ', '), 'User') AS Role,
        u.CreatedAt
    FROM [Users] u
    LEFT JOIN [UserRoles] ur ON u.Id = ur.UserId
    LEFT JOIN [Roles] r ON ur.RoleId = r.Id
    GROUP BY u.Id, u.UserName, u.Email, u.FirstName, u.LastName, u.CreatedAt
    ORDER BY u.CreatedAt DESC;
END
GO

-- 12. sp_GetRecentSessions - Latest sessions with process count and created by
IF OBJECT_ID('dbo.sp_GetRecentSessions', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetRecentSessions;
GO
CREATE PROCEDURE dbo.sp_GetRecentSessions
    @Count INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        ss.Id AS SessionId,
        ss.Name AS SessionName,
        ss.AlgorithmType,
        ISNULL(pc.ProcessCount, 0) AS ProcessCount,
        u.UserName AS CreatedBy,
        u.Email AS CreatedByEmail,
        ss.CreatedAt
    FROM [SchedulingSessions] ss
    LEFT JOIN [Users] u ON ss.UserId = u.Id
    LEFT JOIN (
        SELECT SchedulingSessionId, COUNT(*) AS ProcessCount
        FROM [Processes]
        WHERE IsDeleted = 0
        GROUP BY SchedulingSessionId
    ) pc ON ss.Id = pc.SchedulingSessionId
    WHERE ss.IsDeleted = 0
    ORDER BY ss.CreatedAt DESC;
END
GO

-- 13. sp_GetAlgorithmUsage - Algorithm usage breakdown
IF OBJECT_ID('dbo.sp_GetAlgorithmUsage', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetAlgorithmUsage;
GO
CREATE PROCEDURE dbo.sp_GetAlgorithmUsage
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AlgorithmType,
        COUNT(*) AS Count
    FROM [SchedulingSessions]
    WHERE IsDeleted = 0
    GROUP BY AlgorithmType
    ORDER BY Count DESC;
END
GO

-- 14. sp_GetSessionTrend - Session creation trend (last 30 days)
IF OBJECT_ID('dbo.sp_GetSessionTrend', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSessionTrend;
GO
CREATE PROCEDURE dbo.sp_GetSessionTrend
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        CAST(CreatedAt AS DATE) AS Day,
        COUNT(*) AS Count
    FROM [SchedulingSessions]
    WHERE CreatedAt >= DATEADD(DAY, -30, GETUTCDATE()) AND IsDeleted = 0
    GROUP BY CAST(CreatedAt AS DATE)
    ORDER BY Day;
END
GO

-- 15. sp_GetProcessesPaged - Paginated process listing for admin
IF OBJECT_ID('dbo.sp_GetProcessesPaged', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetProcessesPaged;
GO
CREATE PROCEDURE dbo.sp_GetProcessesPaged
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(200) = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @UserFilter NVARCHAR(450) = NULL,
    @StatusFilter NVARCHAR(50) = NULL,
    @DateFrom DATETIME2 = NULL,
    @DateTo DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH ProcessCTE AS (
        SELECT
            p.Id,
            p.Name AS ProcessName,
            p.ProcessId,
            p.ArrivalTime,
            p.BurstTime,
            p.Deadline,
            p.Priority,
            p.Status,
            u.UserName AS CreatedByName,
            u.Email AS CreatedByEmail,
            p.CreatedAt,
            p.SchedulingSessionId,
            ROW_NUMBER() OVER (
                ORDER BY
                    CASE WHEN @SortColumn = 'ProcessName' AND @SortDirection = 'ASC' THEN p.Name END ASC,
                    CASE WHEN @SortColumn = 'ProcessName' AND @SortDirection = 'DESC' THEN p.Name END DESC,
                    CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'ASC' THEN p.Status END ASC,
                    CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'DESC' THEN p.Status END DESC,
                    CASE WHEN @SortColumn = 'ArrivalTime' AND @SortDirection = 'ASC' THEN p.ArrivalTime END ASC,
                    CASE WHEN @SortColumn = 'ArrivalTime' AND @SortDirection = 'DESC' THEN p.ArrivalTime END DESC,
                    CASE WHEN @SortColumn = 'BurstTime' AND @SortDirection = 'ASC' THEN p.BurstTime END ASC,
                    CASE WHEN @SortColumn = 'BurstTime' AND @SortDirection = 'DESC' THEN p.BurstTime END DESC,
                    CASE WHEN @SortColumn = 'CreatedAt' AND @SortDirection = 'ASC' THEN p.CreatedAt END ASC,
                    CASE WHEN @SortColumn = 'CreatedAt' AND @SortDirection = 'DESC' THEN p.CreatedAt END DESC,
                    p.CreatedAt DESC
            ) AS RowNum
        FROM [Processes] p
        LEFT JOIN [Users] u ON p.UserId = u.Id
        WHERE p.IsDeleted = 0
            AND (@SearchTerm IS NULL OR @SearchTerm = ''
                OR p.Name LIKE '%' + @SearchTerm + '%'
                OR p.ProcessId LIKE '%' + @SearchTerm + '%')
            AND (@UserFilter IS NULL OR @UserFilter = '' OR p.UserId = @UserFilter)
            AND (@StatusFilter IS NULL OR @StatusFilter = '' OR p.Status = @StatusFilter)
            AND (@DateFrom IS NULL OR p.CreatedAt >= @DateFrom)
            AND (@DateTo IS NULL OR p.CreatedAt <= @DateTo)
    )
    SELECT
        Id, ProcessName, ProcessId, ArrivalTime, BurstTime,
        Deadline, Priority, Status, CreatedByName, CreatedByEmail,
        CreatedAt, SchedulingSessionId
    FROM ProcessCTE
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
    ORDER BY RowNum;

    SELECT COUNT(*) AS TotalCount
    FROM [Processes] p
    WHERE p.IsDeleted = 0
        AND (@SearchTerm IS NULL OR @SearchTerm = ''
            OR p.Name LIKE '%' + @SearchTerm + '%'
            OR p.ProcessId LIKE '%' + @SearchTerm + '%')
        AND (@UserFilter IS NULL OR @UserFilter = '' OR p.UserId = @UserFilter)
        AND (@StatusFilter IS NULL OR @StatusFilter = '' OR p.Status = @StatusFilter)
        AND (@DateFrom IS NULL OR p.CreatedAt >= @DateFrom)
        AND (@DateTo IS NULL OR p.CreatedAt <= @DateTo);
END
GO

-- 16. sp_GetProcessDetails - Full process details with session info and result metrics
IF OBJECT_ID('dbo.sp_GetProcessDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetProcessDetails;
GO
CREATE PROCEDURE dbo.sp_GetProcessDetails
    @ProcessId INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        p.Id,
        p.Name AS ProcessName,
        p.ProcessId,
        p.ArrivalTime,
        p.BurstTime,
        p.Deadline,
        p.Priority,
        p.Status,
        u.UserName AS CreatedByName,
        u.Email AS CreatedByEmail,
        p.UserId,
        p.CreatedAt,
        p.UpdatedAt,
        p.SchedulingSessionId,
        ss.Name AS SessionName,
        ss.AlgorithmType,
        sr.CompletionTime,
        sr.TurnaroundTime,
        sr.WaitingTime,
        sr.ResponseTime
    FROM [Processes] p
    LEFT JOIN [Users] u ON p.UserId = u.Id
    LEFT JOIN [SchedulingSessions] ss ON p.SchedulingSessionId = ss.Id
    LEFT JOIN [SchedulingResults] sr ON sr.SchedulingSessionId = p.SchedulingSessionId
        AND sr.ProcessId = p.ProcessId
    WHERE p.Id = @ProcessId;
END
GO

-- 17. sp_DeleteProcess - Soft delete a process
IF OBJECT_ID('dbo.sp_DeleteProcess', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DeleteProcess;
GO
CREATE PROCEDURE dbo.sp_DeleteProcess
    @ProcessId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [Processes]
    SET IsDeleted = 1
    WHERE Id = @ProcessId;
END
GO

-- 18. sp_BulkDeleteProcesses - Bulk soft delete processes
IF OBJECT_ID('dbo.sp_BulkDeleteProcesses', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BulkDeleteProcesses;
GO
CREATE PROCEDURE dbo.sp_BulkDeleteProcesses
    @ProcessIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ids TABLE (ProcessId INT);
    INSERT INTO @Ids (ProcessId)
    SELECT CAST(value AS INT) FROM STRING_SPLIT(@ProcessIds, ',');

    UPDATE [Processes]
    SET IsDeleted = 1
    WHERE Id IN (SELECT ProcessId FROM @Ids);
END
GO

-- 19. sp_GetSessionsPaged - Paginated session listing for admin
IF OBJECT_ID('dbo.sp_GetSessionsPaged', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSessionsPaged;
GO
CREATE PROCEDURE dbo.sp_GetSessionsPaged
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(200) = NULL,
    @SortColumn NVARCHAR(50) = 'CreatedAt',
    @SortDirection NVARCHAR(4) = 'DESC',
    @AlgorithmFilter NVARCHAR(50) = NULL,
    @UserFilter NVARCHAR(450) = NULL,
    @DateFrom DATETIME2 = NULL,
    @DateTo DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    ;WITH SessionCTE AS (
        SELECT
            ss.Id,
            ss.Name AS SessionName,
            ss.AlgorithmType,
            ss.TimeQuantum,
            ss.IsPreemptive,
            ISNULL(pc.ProcessCount, 0) AS ProcessCount,
            u.UserName AS CreatedByName,
            u.Email AS CreatedByEmail,
            ss.CreatedAt,
            ss.Status,
            ROW_NUMBER() OVER (
                ORDER BY
                    CASE WHEN @SortColumn = 'SessionName' AND @SortDirection = 'ASC' THEN ss.Name END ASC,
                    CASE WHEN @SortColumn = 'SessionName' AND @SortDirection = 'DESC' THEN ss.Name END DESC,
                    CASE WHEN @SortColumn = 'AlgorithmType' AND @SortDirection = 'ASC' THEN ss.AlgorithmType END ASC,
                    CASE WHEN @SortColumn = 'AlgorithmType' AND @SortDirection = 'DESC' THEN ss.AlgorithmType END DESC,
                    CASE WHEN @SortColumn = 'ProcessCount' AND @SortDirection = 'ASC' THEN ISNULL(pc.ProcessCount, 0) END ASC,
                    CASE WHEN @SortColumn = 'ProcessCount' AND @SortDirection = 'DESC' THEN ISNULL(pc.ProcessCount, 0) END DESC,
                    CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'ASC' THEN ss.Status END ASC,
                    CASE WHEN @SortColumn = 'Status' AND @SortDirection = 'DESC' THEN ss.Status END DESC,
                    CASE WHEN @SortColumn = 'CreatedAt' AND @SortDirection = 'ASC' THEN ss.CreatedAt END ASC,
                    CASE WHEN @SortColumn = 'CreatedAt' AND @SortDirection = 'DESC' THEN ss.CreatedAt END DESC,
                    ss.CreatedAt DESC
            ) AS RowNum
        FROM [SchedulingSessions] ss
        LEFT JOIN [Users] u ON ss.UserId = u.Id
        LEFT JOIN (
            SELECT SchedulingSessionId, COUNT(*) AS ProcessCount
            FROM [Processes] WHERE IsDeleted = 0
            GROUP BY SchedulingSessionId
        ) pc ON ss.Id = pc.SchedulingSessionId
        WHERE ss.IsDeleted = 0
            AND (@SearchTerm IS NULL OR @SearchTerm = ''
                OR ss.Name LIKE '%' + @SearchTerm + '%')
            AND (@AlgorithmFilter IS NULL OR @AlgorithmFilter = '' OR ss.AlgorithmType = @AlgorithmFilter)
            AND (@UserFilter IS NULL OR @UserFilter = '' OR ss.UserId = @UserFilter)
            AND (@DateFrom IS NULL OR ss.CreatedAt >= @DateFrom)
            AND (@DateTo IS NULL OR ss.CreatedAt <= @DateTo)
    )
    SELECT
        Id, SessionName, AlgorithmType, TimeQuantum, IsPreemptive,
        ProcessCount, CreatedByName, CreatedByEmail, CreatedAt, Status
    FROM SessionCTE
    WHERE RowNum > @Offset AND RowNum <= @Offset + @PageSize
    ORDER BY RowNum;

    SELECT COUNT(*) AS TotalCount
    FROM [SchedulingSessions] ss
    WHERE ss.IsDeleted = 0
        AND (@SearchTerm IS NULL OR @SearchTerm = ''
            OR ss.Name LIKE '%' + @SearchTerm + '%')
        AND (@AlgorithmFilter IS NULL OR @AlgorithmFilter = '' OR ss.AlgorithmType = @AlgorithmFilter)
        AND (@UserFilter IS NULL OR @UserFilter = '' OR ss.UserId = @UserFilter)
        AND (@DateFrom IS NULL OR ss.CreatedAt >= @DateFrom)
        AND (@DateTo IS NULL OR ss.CreatedAt <= @DateTo);
END
GO

-- 20. sp_GetSessionDetails - Full session details with processes, metrics, and gantt chart
IF OBJECT_ID('dbo.sp_GetSessionDetails', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetSessionDetails;
GO
CREATE PROCEDURE dbo.sp_GetSessionDetails
    @SessionId INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Session info with aggregate metrics
    SELECT
        ss.Id,
        ss.Name AS SessionName,
        ss.Description,
        ss.AlgorithmType,
        ss.TimeQuantum,
        ss.IsPreemptive,
        ss.Status,
        u.UserName AS CreatedByName,
        u.Email AS CreatedByEmail,
        ss.UserId,
        ss.CreatedAt,
        ss.UpdatedAt,
        ISNULL(AVG(sr.CpuUtilization), 0) AS CpuUtilization,
        ISNULL(AVG(sr.Throughput), 0) AS Throughput,
        ISNULL(AVG(sr.WaitingTime), 0) AS AverageWaitingTime,
        ISNULL(AVG(sr.TurnaroundTime), 0) AS AverageTurnaroundTime,
        ISNULL(AVG(sr.ResponseTime), 0) AS AverageResponseTime,
        ISNULL(MAX(sr.ContextSwitchCount), 0) AS ContextSwitchCount,
        CASE WHEN COUNT(sr.Id) > 0
            THEN CAST(SUM(CASE WHEN sr.IsMissedDeadline = 1 THEN 1 ELSE 0 END) AS FLOAT) / COUNT(sr.Id) * 100
            ELSE 0 END AS DeadlineMissRatio,
        COUNT(sr.Id) AS TotalProcesses,
        SUM(CASE WHEN sr.CompletionTime > 0 THEN 1 ELSE 0 END) AS CompletedProcesses
    FROM [SchedulingSessions] ss
    LEFT JOIN [Users] u ON ss.UserId = u.Id
    LEFT JOIN [SchedulingResults] sr ON ss.Id = sr.SchedulingSessionId
    WHERE ss.Id = @SessionId
    GROUP BY
        ss.Id, ss.Name, ss.Description, ss.AlgorithmType, ss.TimeQuantum,
        ss.IsPreemptive, ss.Status, u.UserName, u.Email, ss.UserId,
        ss.CreatedAt, ss.UpdatedAt;

    -- Process results
    SELECT
        sr.ProcessId,
        sr.ProcessName,
        sr.ArrivalTime,
        sr.BurstTime,
        sr.Deadline,
        0 AS Priority,
        sr.CompletionTime,
        sr.WaitingTime,
        sr.TurnaroundTime,
        sr.ResponseTime,
        sr.IsMissedDeadline,
        sr.StartTime,
        sr.EndTime
    FROM [SchedulingResults] sr
    WHERE sr.SchedulingSessionId = @SessionId
    ORDER BY sr.StartTime;

    -- Gantt chart entries
    SELECT
        sr.ProcessId,
        sr.ProcessName,
        sr.StartTime,
        sr.EndTime,
        CASE WHEN sr.StartTime = 0 THEN '#28a745'
             WHEN sr.IsMissedDeadline = 1 THEN '#dc3545'
             ELSE '#667eea' END AS Color,
        CAST(0 AS BIT) AS IsContextSwitch
    FROM [SchedulingResults] sr
    WHERE sr.SchedulingSessionId = @SessionId
    ORDER BY sr.StartTime;
END
GO

-- 21. sp_DeleteSession - Soft delete a session
IF OBJECT_ID('dbo.sp_DeleteSession', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_DeleteSession;
GO
CREATE PROCEDURE dbo.sp_DeleteSession
    @SessionId INT
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [SchedulingSessions]
    SET IsDeleted = 1
    WHERE Id = @SessionId;
END
GO

-- 22. sp_BulkDeleteSessions - Bulk soft delete sessions
IF OBJECT_ID('dbo.sp_BulkDeleteSessions', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_BulkDeleteSessions;
GO
CREATE PROCEDURE dbo.sp_BulkDeleteSessions
    @SessionIds NVARCHAR(MAX)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ids TABLE (SessionId INT);
    INSERT INTO @Ids (SessionId)
    SELECT CAST(value AS INT) FROM STRING_SPLIT(@SessionIds, ',');

    UPDATE [SchedulingSessions]
    SET IsDeleted = 1
    WHERE Id IN (SELECT SessionId FROM @Ids);
END
GO

-- 23. sp_GetRecentActivities - Latest activity logs with user names
IF OBJECT_ID('dbo.sp_GetRecentActivities', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_GetRecentActivities;
GO
CREATE PROCEDURE dbo.sp_GetRecentActivities
    @Count INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (@Count)
        al.Id,
        al.UserId,
        al.Action,
        al.Description,
        al.IPAddress,
        al.CreatedAt,
        u.UserName
    FROM [ActivityLogs] al
    LEFT JOIN [Users] u ON al.UserId = u.Id
    ORDER BY al.CreatedAt DESC;
END
GO
