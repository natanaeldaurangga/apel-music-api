using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;
using System.Data;

namespace ApelMusic.Database.Repositories
{
    public class ShoppingCartRepository
    {
        private readonly IConfiguration _config;

        private readonly ILogger<ShoppingCartRepository> _logger;

        private readonly string? ConnectionString;

        public ShoppingCartRepository(IConfiguration config, ILogger<ShoppingCartRepository> logger)
        {
            _config = config;
            _logger = logger;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region METHODs untuk find shopping cart

        // Mencari cart berdasarkan satu column
        public async Task<List<ShoppingCart>> FindCartByAsync(string column = "", string value = "", List<Guid>? values = null)
        {
            var carts = new List<ShoppingCart>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query1 = @"
                    SELECT sc.id as id,
                           sc.user_id as user_id,
                           sc.course_id as course_id,
                           sc.course_schedule as course_schedule,
                           c.name as course_name,
                           c.image as course_image,
                           c.price as course_price
                    FROM shopping_cart sc 
                    LEFT JOIN courses c ON c.id = sc.course_id
                ";

                queryBuilder.Append(query1);

                // WHERE column = value
                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column) && !string.IsNullOrEmpty(value) && values == null)
                {
                    queryBuilder.Append(" WHERE ").Append(column).Append(" = @Value");
                }

                // WHERE column in (values)
                if (values != null && string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(column))
                {
                    queryBuilder.Append(" WHERE ").Append(column).Append(" IN (");
                    for (int i = 0; i < values.Count; i++)
                    {
                        queryBuilder.Append("@Value").Append(i);
                        if (i < values.Count - 1)
                        {
                            queryBuilder.Append(", ");
                        }
                    }
                    queryBuilder.Append(") ");
                }

                queryBuilder.Append(';');

                string finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column) && !string.IsNullOrEmpty(value) && values == null)
                {
                    cmd.Parameters.AddWithValue("@Value", value);
                }

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column) && string.IsNullOrEmpty(value) && values != null)
                {
                    for (int i = 0; i < values.Count; i++)
                    {
                        cmd.Parameters.AddWithValue("@Value" + i, values[i]);
                    }
                }

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var shoppingCart = new ShoppingCart
                        {
                            Id = reader.GetGuid("id"),
                            UserId = reader.GetGuid("user_id"),
                            CourseId = reader.GetGuid("course_id"),
                            CourseSchedule = reader.GetDateTime("course_schedule"),
                            Course = new Course()
                            {
                                Id = reader.GetGuid("course_id"),
                                Price = reader.GetDecimal("course_price")
                            }
                        };

                        carts.Add(shoppingCart);
                    }
                }

                return carts;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<ShoppingCart>> FindCartByUserIdAsync(Guid userId)
        {
            return await FindCartByAsync("user_id", userId.ToString());
        }

        public async Task<List<ShoppingCart>> FindCartByIdsAsync(List<Guid> ids)
        {
            return await FindCartByAsync("sc.id", values: ids);
        }

        #endregion

        #region METHODs untuk delete shopping cart
        public async Task<int> DeleteCartTaskAsync(SqlConnection conn, SqlTransaction transaction, List<Guid> ids)
        {
            var queryBuilder = new StringBuilder();
            const string query = @"
                DELETE FROM shopping_cart WHERE id IN (
            ";

            queryBuilder.Append(query);

            for (int i = 0; i < ids.Count; i++)
            {
                queryBuilder.Append("@Value").Append(i);
                if (i < ids.Count - 1) queryBuilder.Append(", ");
                else queryBuilder.Append(");");
            }

            var finalQuery = queryBuilder.ToString();

            SqlCommand cmd = new(finalQuery, conn, transaction);

            for (int i = 0; i < ids.Count; i++)
            {
                cmd.Parameters.AddWithValue("@Value" + i, ids[i]);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteCartAsync(List<Guid> ids)
        {
            using SqlConnection conn = new(this.ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await DeleteCartTaskAsync(conn, transaction, ids);
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

        #region METHODs untuk insert shopping cart
        public async Task<int> InsertCartTaskAsync(SqlConnection conn, SqlTransaction transaction, ShoppingCart shoppingCart)
        {
            const string query = @"
                INSERT INTO shopping_cart(id, user_id, course_id, course_schedule)
                VALUES (@Id, @UserId, @CourseId, @CourseSchedule)
            ";

            using SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", shoppingCart.Id);
            cmd.Parameters.AddWithValue("@UserId", shoppingCart.UserId);
            cmd.Parameters.AddWithValue("@CourseId", shoppingCart.CourseId);
            cmd.Parameters.AddWithValue("@CourseSchedule", shoppingCart.CourseSchedule);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertCartAsync(ShoppingCart shoppingCart)
        {
            using SqlConnection conn = new(this.ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await InsertCartTaskAsync(conn, transaction, shoppingCart);
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

    }
}