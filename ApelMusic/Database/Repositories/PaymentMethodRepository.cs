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
    public class PaymentMethodRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        private readonly ILogger<PaymentMethodRepository> _logger;

        public PaymentMethodRepository(IConfiguration config, ILogger<PaymentMethodRepository> logger)
        {
            _config = config;
            _logger = logger;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region METHODs untuk find payment
        public async Task<List<PaymentMethod>> FindPaymentByAsync(string column = "", string value = "")
        {
            var payments = new List<PaymentMethod>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query1 = @"
                        SELECT pm.id as id,
                               pm.image as image,
                               pm.name as name,
                               pm.created_at as created_at,
                               pm.updated_at as updated_at,
                               pm.inactive as inactive
                        FROM payment_methods pm 
                    ";

                queryBuilder.Append(query1);

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column))
                {
                    queryBuilder.Append(" WHERE ").Append(column).Append(" = @Value");
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
                        var paymentMethod = new PaymentMethod()
                        {
                            Id = reader.GetGuid("id"),
                            Image = reader.GetString("image"),
                            Name = reader.GetString("name"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        };

                        bool isActiveCourse = await reader.IsDBNullAsync(reader.GetOrdinal("inactive"));
                        if (!isActiveCourse)
                        {
                            paymentMethod.Inactive = reader.GetDateTime("inactive");
                        }

                        payments.Add(paymentMethod);
                    }
                }
                return payments;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<PaymentMethod>> FindAllPaymentAsync()
        {
            return await FindPaymentByAsync();
        }

        public async Task<List<PaymentMethod>> FindPaymentByIdAsync(Guid paymentId)
        {
            return await FindPaymentByAsync("id", paymentId.ToString());
        }

        #endregion

        #region METHODs untuk update payment
        public async Task<int> UpdatePaymentTaskAsync(SqlConnection conn, SqlTransaction transaction, PaymentMethod paymentMethod)
        {
            const string query = @"
                UPDATE payment_methods
                SET image = @Image,
                    name = @Name,
                    updated_at = @UpdatedAt,
                    inactive = @Inactive
                WHERE id = @Id
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", paymentMethod.Id);
            cmd.Parameters.AddWithValue("@Image", paymentMethod.Image);
            cmd.Parameters.AddWithValue("@Name", paymentMethod.Name);
            cmd.Parameters.AddWithValue("@UpdatedAt", paymentMethod.UpdatedAt);

            if (paymentMethod.Inactive == null)
            {
                cmd.Parameters.AddWithValue("@Inactive", DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@Inactive", paymentMethod.Inactive);
            }

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdatePaymentAsync(PaymentMethod paymentMethod)
        {
            using SqlConnection conn = new(this.ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await UpdatePaymentTaskAsync(conn, transaction, paymentMethod);
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

        #region METHOD UNTUK Insert payment
        public async Task<int> InsertPaymentTaskAsync(SqlConnection conn, SqlTransaction transaction, PaymentMethod paymentMethod)
        {
            const string query = @"
                INSERT INTO payment_methods(id, image, name, created_at, updated_at)
                VALUES (@Id, @Image, @Name, @CreatedAt, @UpdatedAt);
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", paymentMethod.Id);
            cmd.Parameters.AddWithValue("@Image", paymentMethod.Image);
            cmd.Parameters.AddWithValue("@Name", paymentMethod.Name);
            cmd.Parameters.AddWithValue("@CreatedAt", paymentMethod.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", paymentMethod.UpdatedAt);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertPaymentAsync(PaymentMethod paymentMethod)
        {
            using SqlConnection conn = new(this.ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await InsertPaymentTaskAsync(conn, transaction, paymentMethod);
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