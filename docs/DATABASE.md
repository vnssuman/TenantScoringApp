# Tenant Scoring Database

The backend uses SQL Server LocalDB with the following connection string:

```text
Server=(localdb)\MSSQLLocalDB;Database=TenantScoreApp;Trusted_Connection=True;TrustServerCertificate=True;
```

## Create Database and Schema

Run this script in SQL Server Management Studio, Azure Data Studio, or `sqlcmd`.

```sql
IF DB_ID(N'TenantScoreApp') IS NULL
BEGIN
	CREATE DATABASE [TenantScoreApp];
END;
GO

USE [TenantScoreApp];
GO

IF OBJECT_ID(N'dbo.TenantApplications', N'U') IS NULL
BEGIN
	CREATE TABLE [dbo].[TenantApplications]
	(
		[TenantId] nvarchar(12) NOT NULL,
		[CountryCode] nvarchar(10) NOT NULL,
		[MobileNumber] nvarchar(10) NOT NULL,
		[FirstName] nvarchar(50) NOT NULL,
		[LastName] nvarchar(50) NOT NULL,
		[Email] nvarchar(100) NOT NULL,
		[PinHash] nvarchar(max) NOT NULL,
		[CreatedAt] datetimeoffset(7) NOT NULL
			CONSTRAINT [DF_TenantApplications_CreatedAt]
			DEFAULT (CONVERT(datetimeoffset(7), SYSUTCDATETIME())),
		CONSTRAINT [PK_TenantApplications] PRIMARY KEY CLUSTERED ([TenantId])
	);
END;
GO

IF NOT EXISTS
(
	SELECT 1
	FROM sys.indexes
	WHERE name = N'UX_TenantApplications_TenantId'
	  AND object_id = OBJECT_ID(N'dbo.TenantApplications')
)
BEGIN
	CREATE UNIQUE INDEX [UX_TenantApplications_TenantId]
		ON [dbo].[TenantApplications] ([TenantId]);
END;
GO

IF NOT EXISTS
(
	SELECT 1
	FROM sys.indexes
	WHERE name = N'UX_TenantApplications_CountryCode_MobileNumber'
	  AND object_id = OBJECT_ID(N'dbo.TenantApplications')
)
BEGIN
	CREATE UNIQUE INDEX [UX_TenantApplications_CountryCode_MobileNumber]
		ON [dbo].[TenantApplications] ([CountryCode], [MobileNumber]);
END;
GO
```

`TenantId` is the primary key and also has an explicit unique index, as required by the application schema. Email is intentionally not unique, so multiple applications may use the same email address.

## Verify Schema

```sql
USE [TenantScoreApp];
GO

SELECT
	[name] AS [ColumnName],
	TYPE_NAME([user_type_id]) AS [SqlType],
	[max_length] AS [MaxLengthBytes],
	[is_nullable] AS [IsNullable]
FROM sys.columns
WHERE [object_id] = OBJECT_ID(N'dbo.TenantApplications')
ORDER BY [column_id];
GO

SELECT
	[name] AS [IndexName],
	[is_unique] AS [IsUnique],
	[type_desc] AS [IndexType]
FROM sys.indexes
WHERE [object_id] = OBJECT_ID(N'dbo.TenantApplications')
  AND [index_id] > 0;
GO
```

PIN values must never be inserted directly. The API stores only BCrypt hashes in `[PinHash]`.
