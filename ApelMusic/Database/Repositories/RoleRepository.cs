using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;

namespace ApelMusic.Database.Repositories
{
    public class RoleRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        public RoleRepository(IConfiguration config)
        {
            _config = config;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<int> InsertRoleTaskAsync(SqlConnection conn, SqlTransaction transaction, Role role)
        {
            const string query = @"INSERT INTO roles (id, name, created_at, updated_at) 
                    VALUES (@Id, @Name, @CreatedAt, @UpdatedAt);";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", role.Id);
            cmd.Parameters.AddWithValue("@Name", role.Name);
            cmd.Parameters.AddWithValue("@CreatedAt", role.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", role.UpdatedAt);
            int rowAffected = await cmd.ExecuteNonQueryAsync();
            return rowAffected;
        }

        public async Task<int> InsertRoleAsync(Role role)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await InsertRoleTaskAsync(conn, transaction, role);
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


        // Get role by name
        public async Task<List<Role>?> GetByNameAsync(string name)
        {
            var roles = new List<Role>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                const string query = "SELECT * FROM roles WHERE inactive IS NULL AND name = @Name";
                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Name", $"{name}");
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var role = new Role()
                        {
                            Id = reader.GetGuid("id"),
                            Name = reader.GetString("name"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at")
                        };

                        roles.Add(role);
                    }
                }
                return roles;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}