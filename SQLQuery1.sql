USE ApelMusicDB;

-- START: Drop semua constraint
IF (OBJECT_ID('dbo.fk_user_role', 'F') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.users DROP CONSTRAINT fk_user_role
END

IF (OBJECT_ID('dbo.fk_user_role', 'F') IS NOT NULL)
BEGIN
    ALTER TABLE dbo.users DROP CONSTRAINT fk_user_role
END

-- END: Drop semua constraint

-- START: Drop semua table
IF OBJECT_ID(N'dbo.roles', N'U') IS NOT NULL
	DROP TABLE dbo.roles

IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
	DROP TABLE dbo.users

IF OBJECT_ID(N'dbo.categories', N'U') IS NOT NULL
	DROP TABLE dbo.categories

IF OBJECT_ID(N'dbo.courses', N'U') IS NOT NULL
	DROP TABLE dbo.courses

-- END: Drop semua table

CREATE TABLE roles (
	id UNIQUEIDENTIFIER PRIMARY KEY,
	name VARCHAR(25) NOT NULL,
	created_at DATETIME DEFAULT GETDATE(),
	updated_at DATETIME DEFAULT GETDATE(),
	inactive DATETIME DEFAULT NULL
);
GO

CREATE TABLE users (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    full_name VARCHAR(100),
    email VARCHAR(100) UNIQUE,
    password_hash varbinary(32),
    password_salt varbinary(64),
    refresh_token VARCHAR(64),
    role_id UNIQUEIDENTIFIER NOT NULL,
    verification_token VARCHAR(255),
    verfied_at DATETIME DEFAULT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    inactive DATETIME DEFAULT NULL
);
GO


CREATE TABLE categories (
	id UNIQUEIDENTIFIER PRIMARY KEY,
	tag_name VARCHAR(50) NOT NULL,
	[name] VARCHAR(50) NOT NULL,
	image_path VARCHAR(255) NOT NULL,
	category_description TEXT,
	created_at DATETIME,
	udpated_at DATETIME,
	inactive DATETIME
);
GO

CREATE TABLE [courses] (
  [id] UNIQUEIDENTIFIER PRIMARY KEY,
  [name] varchar(255) NOT NULL,
  [category_id] int NOT NULL,
  [image_path] varchar(255),
  [description] text,
  [created_at] datetime DEFAULT GETDATE(),
  [updated_at] datetime DEFAULT GETDATE(),
  [inactive] datetime
)
GO

IF OBJECT_ID(N'dbo.roles', N'U') IS NOT NULL
	IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
		BEGIN
			ALTER TABLE users
			ADD CONSTRAINT fk_user_role
			FOREIGN KEY (role_id)
			REFERENCES roles(id)
		END


SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE' 
AND TABLE_CATALOG = 'ApelMusicDB';

EXEC sp_help 'users';

SELECT * FROM roles;
SELECT * FROM users;

SELECT u.id as user_id, 
	   full_name, 
	   email, 
	   password_hash, 
	   password_salt, 
	   r.id as role_id, 
	   r.name as role_name,
	   u.created_at as user_created_at,
	   u.updated_at as user_updated_at,
	   u.inactive as user_inactive,
	   r.created_at as role_created_at,
	   r.updated_at as role_updated_at,
	   r.inactive as role_inactive
FROM users u 
LEFT JOIN roles r ON u.role_id = r.id;

UPDATE users SET verfied_at = GETDATE() WHERE verification_token = '';

