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

        #endregion
        // END: Method for creating tables

        // START: Method for adding foreign key
        #region Method for adding foreign key
        public async Task<int> AddForeignKeyConstraintAsync(SqlConnection conn, SqlTransaction transaction)
        {
            const string query = @"
                IF OBJECT_ID(N'dbo.roles', N'U') IS NOT NULL
                    IF OBJECT_ID(N'dbo.users', N'U') IS NOT NULL
                        BEGIN
                            ALTER TABLE users
                            ADD CONSTRAINT fk_user_role
                            FOREIGN KEY (role_id)
                            REFERENCES roles(id)
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