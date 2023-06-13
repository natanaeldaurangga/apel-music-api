using System.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;
using ApelMusic.DTOs.Auth;

namespace ApelMusic.Database.Repositories
{
    public class UserRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IConfiguration config, ILogger<UserRepository> logger)
        {
            _config = config;
            _logger = logger;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        private static async Task<int> ExecuteQueryAsync(SqlConnection conn, SqlTransaction transaction, string query)
        {
            SqlCommand cmd = new(query, conn, transaction);
            return await cmd.ExecuteNonQueryAsync();
        }

        // Method multi fungsi, kita bisa mencari user berdasarkan semua kolom yang ada di table user
        public async Task<List<User>> FindUserByAsync(string column = "", string value = "")
        {
            var users = new List<User>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query1 = @"
                    SELECT u.id as user_id, 
                            full_name, 
                            email, 
                            password_hash, 
                            password_salt, 
                            verification_token,
                            verified_at,
                            r.id as role_id, 
                            r.name as role_name, 
                            u.created_at as user_created_at, 
                            u.updated_at as user_updated_at, 
                            u.inactive as user_inactive, 
                            r.created_at as role_created_at, 
                            r.updated_at as role_updated_at, 
                            r.inactive as role_inactive
                    FROM users u 
                    LEFT JOIN roles r ON u.role_id = r.id 
                ";

                queryBuilder.Append(query1);

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column))
                {
                    // WHERE <inputColumn> = <value>
                    queryBuilder.Append("WHERE ").Append(column).Append(" = @Value");
                }

                queryBuilder.Append(';');

                string finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column))
                {
                    cmd.Parameters.AddWithValue("@Value", value);
                }

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var user = new User()
                        {
                            Id = reader.GetGuid("user_id"),
                            FullName = reader.GetString("full_name"),
                            Email = reader.GetString("email"),
                            PasswordHash = (byte[])reader["password_hash"],
                            PasswordSalt = (byte[])reader["password_salt"],
                            VerificationToken = reader.GetString("verification_token"),
                            RoleId = reader.GetGuid("role_id"),
                            Role = new()
                            {
                                Id = reader.GetGuid("role_id"),
                                Name = reader.GetString("role_name"),
                                CreatedAt = reader.GetDateTime("role_created_at"),
                                UpdatedAt = reader.GetDateTime("role_updated_at")
                            },
                            CreatedAt = reader.GetDateTime("user_created_at"),
                            UpdatedAt = reader.GetDateTime("user_updated_at")
                        };

                        // Ngecek apakah user sudah verify email
                        bool isVerifiedNull = !await reader.IsDBNullAsync(reader.GetOrdinal("verified_at"));

                        if (isVerifiedNull)
                        {
                            user.VerifiedAt = reader.GetDateTime("verified_at");
                        }

                        users.Add(user);
                    }
                }
                return users;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<User>> FindAllUserAsync()
        {
            return await FindUserByAsync();
        }

        public async Task<List<User>> FindUserByEmailAsync(string email)
        {
            return await FindUserByAsync("email", email);
        }

        public async Task<int> UpdateRefreshTaskTokenAsync(SqlConnection connection, SqlTransaction transaction, Guid userId, RefreshTokenResponse response)
        {
            const string query = @"
                UPDATE users 
                SET refresh_token = @RefreshToken,
                    token_created = @CreatedAt,
                    token_expires = @ExpiredAt,
                    updated_at = @CreatedAt
                WHERE id = @Id;
            ";

            SqlCommand cmd = new(query, connection, transaction);
            cmd.Parameters.AddWithValue("@RefreshToken", response.Token);
            cmd.Parameters.AddWithValue("@CreatedAt", response.CreatedAt);
            cmd.Parameters.AddWithValue("@ExpiredAt", response.Expires);
            cmd.Parameters.AddWithValue("@Id", userId);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdateRefreshTokenAsync(Guid userId, RefreshTokenResponse response)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await UpdateRefreshTaskTokenAsync(conn, transaction, userId, response);
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

        // Task Untuk Verify User
        public async Task<int> VerifyUserTaskAsync(SqlConnection conn, SqlTransaction transaction, string token)
        {
            // Pada query di bawah ini terdapat verified_at IS NULL, untuk mengecek apakah token sudah pernah diverifikasi sebelumnya
            const string query = @"
                UPDATE users SET verified_at = @VerifiedAt WHERE verification_token = @Token AND verified_at IS NULL;
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@VerifiedAt", DateTime.Now);
            cmd.Parameters.AddWithValue("@Token", token);

            return await cmd.ExecuteNonQueryAsync(); // Melihat jumlah row affected minimal 1 jika 0 maka tidak ada row affected
        }

        public async Task<int> InsertUserTaskAsync(SqlConnection conn, SqlTransaction transaction, User user)
        {
            const string query = @"
                INSERT INTO users (id, full_name, email, password_hash, password_salt, role_id, verification_token, created_at, updated_at)
                VALUES (@Id, @FullName, @Email, @PasswordHash, @PasswordSalt, @RoleId, @VerificationToken, @CreatedAt, @UpdatedAt);
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", user.Id);
            cmd.Parameters.AddWithValue("@FullName", user.FullName);
            cmd.Parameters.AddWithValue("@Email", user.Email);
            cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", user.PasswordSalt);
            cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
            cmd.Parameters.AddWithValue("@VerificationToken", user.VerificationToken);
            cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", user.UpdatedAt);
            return await cmd.ExecuteNonQueryAsync();
        }

        // Eksekusi Task Verify User
        public async Task<bool> VerifyUserAsync(string token)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                var result = await VerifyUserTaskAsync(conn, transaction, token);
                transaction.Commit();
                return result > 0; // Jika ada row yang affected berarti return true (1 > 0)
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

        public async Task<bool> InsertUserAsync(User user)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await InsertUserTaskAsync(conn, transaction, user);
                transaction.Commit();
                return true;
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

    }
}