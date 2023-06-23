using System.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Entities;
using System.Data;

namespace ApelMusic.Database.Repositories
{
    public class InvoiceRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        private readonly ILogger<InvoiceRepository> _logger;

        private readonly UsersCoursesRepository _userCourseRepo;

        private readonly ShoppingCartRepository _cartRepo;

        public InvoiceRepository(IConfiguration config, ILogger<InvoiceRepository> logger, UsersCoursesRepository userCourseRepo, ShoppingCartRepository cartRepo)
        {
            _config = config;
            _logger = logger;
            _userCourseRepo = userCourseRepo;
            _cartRepo = cartRepo;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<DetailInvoiceResponse>> GetInvoiceDetailAsync(int id)
        {
            var items = new List<DetailInvoiceResponse>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query = @"
                        SELECT uc.course_id as course_id, 
                               c.name as course_name, 
                               uc.course_schedule as course_schedule, 
                               ct.id as category_id, 
                               ct.name as category_name, 
                               uc.purchase_price as purchase_price 
                        FROM users_courses uc
                        LEFT JOIN courses c ON c.id = uc.course_id
                        LEFT JOIN categories ct ON ct.id = c.category_id 
                        WHERE uc.invoice_id = @Id;
                    ";

                var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                // TODO: Lanjut untuk integrasi checkout ke FE 
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var details = new DetailInvoiceResponse()
                        {
                            CourseId = reader.GetGuid("course_id"),
                            CategoryId = reader.GetGuid("category_id"),
                            CategoryName = reader.GetString("category_name"),
                            CourseName = reader.GetString("course_name"),
                            CourseSchedule = reader.GetDateTime("course_schedule"),
                            PurchasePrice = reader.GetDecimal("purchase_price")
                        };

                        items.Add(details);
                    }

                }
                return items;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<InvoiceResponse>> InvoicesPagedAsync(PageQueryRequest pageQuery, IDictionary<string, string>? fields = null)
        {
            var invoices = new List<InvoiceResponse>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query = @"
                        SELECT i.id as invoice_id,
                            i.invoice_number as invoice_number,
                            i.user_id as user_id,
                            u.full_name as user_name,
                            i.purchase_date as purchase_date, 
                            t.quantity as quantity,
                            t.total_price as total_price,
                            i.payment_method_id as payment_id,
                            pmt.name as payment_name
                        FROM invoices i
                        JOIN (
                            SELECT COUNT(uc.course_id) AS quantity, SUM(uc.purchase_price) AS total_price, uc.invoice_id
                            FROM users_courses uc
                            GROUP BY uc.invoice_id
                        ) t ON t.invoice_id = i.id
                        JOIN payment_methods pmt ON pmt.id = i.payment_method_id 
                        JOIN users u ON u.id = i.user_id 
                        WHERE (
                            (UPPER(i.invoice_number) LIKE UPPER(@Keyword))
                            OR (UPPER(pmt.name) LIKE UPPER(@Keyword))
                            ) 
                    ";

                queryBuilder.Append(query);

                if (fields != null)
                {
                    queryBuilder.Append("AND (");
                    for (int i = 0; i < fields.Count; i++)
                    {
                        KeyValuePair<string, string> colVal = fields.ElementAt(i);
                        queryBuilder.Append(colVal.Key).Append(" = @Value").Append(i);

                        if (i != fields.Count - 1) queryBuilder.Append(" AND ");
                        else queryBuilder.Append(") ");
                    }
                }

                // Menentukan Ascending atau Descending
                string direction = string.Equals(pageQuery.Direction, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC"; // komparasi string tanpa melihat case nya

                // Menentukan kolom mana yang akan disorting
                string columnSorted = string.IsNullOrEmpty(pageQuery.SortBy) ? "i.invoice_number" : pageQuery.SortBy;
                string orderByQuery = $"ORDER BY {columnSorted} {direction} ";

                queryBuilder.Append(orderByQuery);

                const string pagingQuery = @"
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY 
                    ";

                queryBuilder.Append(pagingQuery);

                string finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                int offset = (pageQuery.CurrentPage - 1) * pageQuery.PageSize;
                string keyword = "%" + pageQuery.Keyword + "%";
                cmd.Parameters.AddWithValue("@Keyword", keyword ?? "");

                cmd.Parameters.AddWithValue("@OrderBy", pageQuery.SortBy ?? "i.id");

                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageQuery.PageSize);

                // Memasang parameter pada tiap @Value
                if (fields != null)
                {
                    for (int i = 0; i < fields.Count; i++)
                    {
                        KeyValuePair<string, string> colVal = fields.ElementAt(i);
                        cmd.Parameters.AddWithValue("@Value" + i, colVal.Value);
                    }
                }

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var invoice = new InvoiceResponse()
                        {
                            Id = reader.GetInt32("invoice_id"),
                            InvoiceNumber = reader.GetString("invoice_number"),
                            UserId = reader.GetGuid("user_id"),
                            PaymentId = reader.GetGuid("payment_id"),
                            Payment = new PaymentResponse()
                            {
                                Id = reader.GetGuid("payment_id"),
                                Name = reader.GetString("payment_name")
                            },
                            PurchaseDate = reader.GetDateTime("purchase_date"),
                            Quantity = reader.GetInt32("quantity"),
                            TotalPrice = reader.GetDecimal("total_price")
                        };

                        invoices.Add(invoice);
                    }
                }

                return invoices;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<int> InsertInvoiceTaskAsync(SqlConnection conn, SqlTransaction transaction, Invoice invoice)
        {
            const string query = @"
                INSERT INTO invoices(user_id, purchase_date, payment_method_id) 
                VALUES (@UserId, @PurchaseDate, @PaymentMethodId);
                SELECT SCOPE_IDENTITY();
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@UserId", invoice.UserId);
            cmd.Parameters.AddWithValue("@PurchaseDate", invoice.PurchaseDate);
            cmd.Parameters.AddWithValue("@PaymentMethodId", invoice.PaymentMethodId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()); // Mengembalikan invoice id supaya disimpan ke dalam tiap user course
        }

        public async Task<int> MakePurchaseAsync(Invoice invoice, List<UserCourses> userCourses, List<ShoppingCart> carts)
        {
            using SqlConnection conn = new(this.ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                // Insert invoice, dan mengambil idnya
                var invoiceId = await InsertInvoiceTaskAsync(conn, transaction, invoice);
                userCourses.ForEach(course => course.InvoiceId = invoiceId); // Menyimpan invoice id pada tiap user courses

                // Bulk insert user course
                _ = await _userCourseRepo.BulkInsertUserCoursesTaskAsync(conn, transaction, userCourses);

                // Mendelete cart yang sudah dicheckout
                List<Guid> cartIds = carts.ConvertAll(cart => cart.Id);

                _ = await _cartRepo.DeleteCartTaskAsync(conn, transaction, cartIds);
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

        public async Task<int> MakeDirectPurchase(Invoice invoice, List<UserCourses> userCourses)
        {
            using SqlConnection conn = new(this.ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                // Insert invoice, dan mengambil idnya
                var invoiceId = await InsertInvoiceTaskAsync(conn, transaction, invoice);
                userCourses.ForEach(course => course.InvoiceId = invoiceId); // Menyimpan invoice id pada tiap user courses

                // Bulk insert user course
                _ = await _userCourseRepo.BulkInsertUserCoursesTaskAsync(conn, transaction, userCourses);
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

    }
}