using System.Dynamic;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Database.Migrations
{
    public class MainMigrations
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        public MainMigrations(IConfiguration config)
        {
            _config = config;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        private static async Task<int> MigrationExecuteAsync(SqlConnection conn, SqlTransaction transaction, string query)
        {
            SqlCommand cmd = new(query, conn, transaction);
            return await cmd.ExecuteNonQueryAsync();
        }

        // START: Method for clearing database
        #region Method for clearing database
        private static async Task<int> ClearConstraintTaskAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                IF (OBJECT_ID('dbo.fk_user_role', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.users DROP CONSTRAINT fk_user_role
                    END

                IF (OBJECT_ID('dbo.fk_course_category', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.courses DROP CONSTRAINT fk_course_category
                    END

                IF (OBJECT_ID('dbo.fk_schedule_course', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.course_schedules DROP CONSTRAINT fk_schedule_course
                    END

                IF (OBJECT_ID('dbo.fk_invoice_payment_method', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.invoices DROP CONSTRAINT fk_invoice_payment_method
                    END

                IF (OBJECT_ID('dbo.fk_invoice_user', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.invoices DROP CONSTRAINT fk_invoice_user
                    END
                
                IF (OBJECT_ID('dbo.fk_shopping_cart_user', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.shopping_cart DROP CONSTRAINT fk_shopping_cart_user
                    END

                IF (OBJECT_ID('dbo.fk_shopping_cart_course', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.shopping_cart DROP CONSTRAINT fk_shopping_cart_course
                    END
                
                IF (OBJECT_ID('dbo.fk_user_course_course', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.users_courses DROP CONSTRAINT fk_user_course_course
                    END

                IF (OBJECT_ID('dbo.fk_users_courses_user', 'F') IS NOT NULL)
                    BEGIN
                        ALTER TABLE dbo.users_courses DROP CONSTRAINT fk_users_courses_user
                    END
            ";
            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> ClearTablesTaskAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
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
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }
        #endregion
        // END: Method for clearing database

        // START: Method for creating tables
        #region Method for creating tables
        private static async Task<int> CreateTableRolesAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE roles (
                    id UNIQUEIDENTIFIER PRIMARY KEY,
                    name VARCHAR(25) NOT NULL,
                    created_at DATETIME DEFAULT GETDATE(),
                    updated_at DATETIME DEFAULT GETDATE(),
                    inactive DATETIME DEFAULT NULL
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableUsersAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
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
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableCategoriesAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
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
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableCoursesAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE [courses] (
                    [id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [name] varchar(255) NOT NULL,
                    [category_id] UNIQUEIDENTIFIER NOT NULL,
                    [image] varchar(255),
                    [price] decimal(10, 2),
                    [description] text,
                    [created_at] datetime DEFAULT GETDATE(),
                    [updated_at] datetime DEFAULT GETDATE(),
                    [inactive] datetime
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableCourseSchedulesAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE course_schedules(
                    id UNIQUEIDENTIFIER PRIMARY KEY,
                    course_id UNIQUEIDENTIFIER NOT NULL,
                    course_date DATETIME NOT NULL
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTablePaymentMethodsAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE payment_methods (
                    [id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [image] VARCHAR(255),
                    [name] VARCHAR(100) NOT NULL,
                    [created_at] DATETIME NOT NULL,
                    [updated_at] DATETIME NOT NULL,
                    [inactive] DATETIME
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableShoppingCartAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE shopping_cart (
                    [id] UNIQUEIDENTIFIER PRIMARY KEY,
                    [user_id] UNIQUEIDENTIFIER,
                    [course_id] UNIQUEIDENTIFIER,
                    [course_schedule] DATETIME NOT NULL
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableInvoicesAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE invoices (
                    id UNIQUEIDENTIFIER PRIMARY KEY,
                    invoice_number VARCHAR(10) NOT NULL UNIQUE,
                    user_id UNIQUEIDENTIFIER NOT NULL,
                    purchase_date DATETIME NOT NULL DEFAULT GETDATE(),
                    payment_method_id UNIQUEIDENTIFIER,
                    total_price DECIMAL(10, 2)
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        private static async Task<int> CreateTableUsersCoursesAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                CREATE TABLE users_courses (
                    user_id UNIQUEIDENTIFIER NOT NULL,
                    course_id UNIQUEIDENTIFIER NOT NULL,
                    course_schedule DATETIME NOT NULL,
                    invoice_id UNIQUEIDENTIFIER NOT NULL,
                    purchase_price DECIMAL(10, 2)
                );
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }

        #endregion
        // END: Method for creating tables

        // START: Method for adding foreign key
        #region Method for adding foreign key
        public async Task<int> AddForeignKeyConstraintAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.roles', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE users
                            ADD CONSTRAINT fk_user_role
                            FOREIGN KEY (role_id)
                            REFERENCES roles(id)
                        END

                IF OBJECT_ID(N'dbo.categories', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.courses', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE courses
                            ADD CONSTRAINT fk_course_category
                            FOREIGN KEY (category_id)
                            REFERENCES categories(id)
                        END

                IF OBJECT_ID(N'dbo.courses', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.course_schedules', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE courses
                            ADD CONSTRAINT fk_schedule_course
                            FOREIGN KEY (course_id)
                            REFERENCES courses(id)
                        END


                IF OBJECT_ID(N'dbo.invoices', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.payment_methods', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE invoices
                            ADD CONSTRAINT fk_invoice_payment_method
                            FOREIGN KEY (payment_method_id)
                            REFERENCES payment_methods(id)
                        END

                    IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE invoices
                            ADD CONSTRAINT fk_invoice_user
                            FOREIGN KEY (user_id)
                            REFERENCES users(id)
                        END


                IF OBJECT_ID(N'dbo.shopping_cart', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE shopping_cart
                            ADD CONSTRAINT fk_shopping_cart_user
                            FOREIGN KEY (user_id)
                            REFERENCES users(id)
                        END

                    IF OBJECT_ID(N'dbo.courses', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE shopping_cart
                            ADD CONSTRAINT fk_shopping_cart_course
                            FOREIGN KEY (course_id)
                            REFERENCES courses(id)
                        END


                IF OBJECT_ID(N'dbo.users_courses', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.courses', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE users_courses
                            ADD CONSTRAINT fk_user_course_course
                            FOREIGN KEY (course_id)
                            REFERENCES courses(id)
                        END

                    IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE users_courses
                            ADD CONSTRAINT fk_user_course_user
                            FOREIGN KEY (user_id)
                            REFERENCES users(id)
                        END
            ";

            return await MigrationExecuteAsync(conn, transaction, query);
        }
        #endregion
        // END: Method for adding foreign key

        // START: Method for executing migrations
        #region Method for executing

        public async Task<int> ExecuteDeleteConstraints()
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await ClearConstraintTaskAsync(conn, transaction);
                transaction.Commit();
                return 1;
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
                await conn.CloseAsync();
            }
        }

        public async Task<int> ExecuteAddConstraintsAsync()
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await AddForeignKeyConstraintAsync(conn, transaction);
                transaction.Commit();
                return 1;
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
                await conn.CloseAsync();
            }
        }

        public async Task<int> ExecuteDeleteAllTableAsync()
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await ClearConstraintTaskAsync(conn, transaction);
                _ = await ClearTablesTaskAsync(conn, transaction);
                transaction.Commit();
                return 1;
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
                await conn.CloseAsync();
            }
        }

        public async Task<int> ExecuteMigrationsQueriesAsync()
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await ClearTablesTaskAsync(conn, transaction);
                _ = await CreateTableRolesAsync(conn, transaction);
                _ = await CreateTableUsersAsync(conn, transaction);
                _ = await CreateTableCategoriesAsync(conn, transaction);
                _ = await CreateTableCoursesAsync(conn, transaction);
                _ = await CreateTableCourseSchedulesAsync(conn, transaction);
                _ = await CreateTablePaymentMethodsAsync(conn, transaction);
                _ = await CreateTableShoppingCartAsync(conn, transaction);
                _ = await CreateTableInvoicesAsync(conn, transaction);
                _ = await CreateTableUsersCoursesAsync(conn, transaction);
                transaction.Commit();
                return 1;
            }
            catch (System.Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
            finally
            {
                await transaction.DisposeAsync();
                await conn.CloseAsync();
            }
        }
        #endregion
        // END: Method for executing migrations
    }
}