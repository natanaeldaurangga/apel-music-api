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

IF OBJECT_ID(N'dbo.course_schedules', N'U') IS NOT NULL
	DROP TABLE dbo.course_schedules

IF OBJECT_ID(N'payment_methods', N'U') IS NOT NULL
	DROP TABLE dbo.payment_methods

IF OBJECT_ID(N'dbo.shopping_cart', N'U') IS NOT NULL
	DROP TABLE dbo.shopping_cart

IF OBJECT_ID(N'dbo.invoices', N'U') IS NOT NULL
	DROP TABLE dbo.invoices

IF OBJECT_ID(N'dbo.users_courses', N'U') IS NOT NULL
	DROP TABLE dbo.users_courses

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
    full_name VARCHAR(100) NOT NULL,
    email VARCHAR(100) UNIQUE,
    password_hash varbinary(32) NOT NULL,
    password_salt varbinary(64) NOT NULL,
    refresh_token VARCHAR(255) DEFAULT NULL,
    token_created DATETIME DEFAULT NULL,
    token_expires DATETIME DEFAULT NULL,
    role_id UNIQUEIDENTIFIER NOT NULL,
    verification_token VARCHAR(255),
    verified_at DATETIME DEFAULT NULL,
    reset_password_token VARCHAR(255) DEFAULT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    inactive DATETIME DEFAULT NULL
);
GO

CREATE TABLE categories (
	[id] UNIQUEIDENTIFIER PRIMARY KEY,
	[tag_name] VARCHAR(50) NOT NULL,
	[name] VARCHAR(50) NOT NULL,
	[image] VARCHAR(255),
	[banner_image] VARCHAR(255),
	[category_description] TEXT,
	[created_at] DATETIME,
	[updated_at] DATETIME,
	[inactive] DATETIME
);
GO

CREATE TABLE [courses] (
  [id] UNIQUEIDENTIFIER PRIMARY KEY,
  [name] varchar(255) NOT NULL,
  [category_id] UNIQUEIDENTIFIER NOT NULL,
  [image] varchar(255),
  [description] text,
  [created_at] datetime DEFAULT GETDATE(),
  [updated_at] datetime DEFAULT GETDATE(),
  [inactive] datetime,
  [price] DECIMAL(10, 2)
)
GO

CREATE TABLE course_schedules(
	id UNIQUEIDENTIFIER PRIMARY KEY,
	course_id UNIQUEIDENTIFIER NOT NULL,
	course_date DATETIME NOT NULL
);


CREATE TABLE payment_methods (
	[id] UNIQUEIDENTIFIER PRIMARY KEY,
	[image] VARCHAR(255),
	[name] VARCHAR(100) NOT NULL,
	created_at DATETIME NOT NULL,
	updated_at DATETIME NOT NULL,
	inactive DATETIME
);

CREATE TABLE shopping_cart (
	[id] UNIQUEIDENTIFIER PRIMARY KEY,
	[user_id] UNIQUEIDENTIFIER,
	[course_id] UNIQUEIDENTIFIER,
	[course_schedule] DATETIME NOT NULL
);

CREATE TABLE invoices (
	id INT IDENTITY(1,1) PRIMARY KEY,
	user_id UNIQUEIDENTIFIER NOT NULL,
	invoice_number VARCHAR(10) NOT NULL UNIQUE,
	purchase_date DATETIME NOT NULL DEFAULT GETDATE(),
	payment_method_id UNIQUEIDENTIFIER
);

CREATE TABLE users_courses (
	[user_id] UNIQUEIDENTIFIER NOT NULL,
	course_id UNIQUEIDENTIFIER NOT NULL,
	course_schedule DATETIME NOT NULL,
	invoice_id INT NOT NULL,
	purchase_price DECIMAL(10, 2)
);

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

EXEC sp_help 'shopping_cart';

SELECT * FROM roles;
SELECT * FROM users;
SELECT c.* FROM categories c;
SELECT TOP(5) COUNT(*) FROM categories;
SELECT * FROM courses ORDER BY [name];
SELECT * FROM course_schedules ORDER BY id ASC;
SELECT COUNT(*) FROM course_schedules;
SELECT * FROM invoices;
SELECT * FROM users_courses;
SELECT i.id,
	   i.invoice_number,
	   i.user_id,
	   u.full_name as user_name,
	   i.purchase_date, 
	   t.quantity,
	   t.total_price,
	   pmt.id as payment_id,
	   pmt.name as payment_name
FROM invoices i
JOIN (
	SELECT COUNT(uc.course_id) AS quantity, SUM(uc.purchase_price) AS total_price, uc.invoice_id
	FROM users_courses uc
	GROUP BY uc.invoice_id
) t ON t.invoice_id = i.id
JOIN payment_methods pmt ON pmt.id = i.payment_method_id
JOIN users u ON u.id = i.user_id;

SELECT uc.course_id, c.name as course_name, uc.course_schedule, ct.name as category_name, uc.purchase_price 
FROM users_courses uc
LEFT JOIN courses c ON c.id = uc.course_id
LEFT JOIN categories ct ON ct.id = c.category_id;

SELECT * FROM shopping_cart;

SELECT COUNT(uc.course_id) AS quantity, SUM(uc.purchase_price) AS total_price, uc.invoice_id
FROM users_courses uc
GROUP BY uc.invoice_id;

SELECT * FROM courses ORDER BY [name] OFFSET (4 - 1) * 4 ROWS FETCH NEXT 4 ROWS ONLY;

-- SELECT @@VERSION;

SELECT * FROM categories WHERE id = '119CB73B-9BA2-4861-85C1-15B4FBE376E8';

UPDATE users SET verification_token = NULL WHERE verification_token = '188662381CCFCF7AE77A7BFBE09E2AAA6CABC00FB8B6B5C6D10C74B9D16E8ECEF5AAD2C6523D98262DAD3EFCDB3A01DCBCEC8E4841B677409E024C00438A4F25';

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

EXEC sp_help 'users_courses';

SELECT sc.id as id,
    sc.user_id as user_id,
    sc.course_id as course_id,
    sc.course_schedule as course_schedule,
    c.name as course_name,
    c.image as course_image,
    c.price as course_price
FROM shopping_cart sc 
LEFT JOIN courses c ON c.id = sc.course_id;

SELECT * FROM courses WHERE id != '7B85853C-3030-4188-A055-9C25BA60C1BE';
SELECT * FROM courses;

DROP TABLE coba_user;

CREATE TABLE coba_user(
	_ID int IDENTITY(1,1) not null,
	ID as RIGHT('0000' + CONVERT(varchar(5),_ID),5) PRIMARY KEY,
	NAMA VARCHAR(100)
);

INSERT INTO coba_user(NAMA) values ('Ipen'), ('Cupen'), ('Nael');

SELECT * FROM coba_user;

SELECT * FROM shopping_cart;

SELECT * FROM users;

DELETE FROM users WHERE id = 'A5406F9C-4807-4169-8FC1-CBC26BB8CD04';
