using System.Text;
using System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;
using ApelMusic.DTOs;
using ApelMusic.DTOs.Courses;

namespace ApelMusic.Database.Repositories
{
    public class UsersCoursesRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        private readonly ILogger<UsersCoursesRepository> _logger;

        public UsersCoursesRepository(IConfiguration config, ILogger<UsersCoursesRepository> logger)
        {
            _config = config;
            _logger = logger;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<int> InsertUserCoursesTaskAsync(SqlConnection conn, SqlTransaction transaction, UserCourses userCourses)
        {
            const string query = @"
                INSERT INTO users_courses(user_id, course_id, course_schedule, invoice_id, purchase_price)
                VALUES(@UserId, @CourseId, @CourseSchedule, @InvoiceId, @PurchasePrice)
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@UserId", userCourses.UserId);
            cmd.Parameters.AddWithValue("@CourseId", userCourses.CourseId);
            cmd.Parameters.AddWithValue("@CourseSchedule", userCourses.CourseSchedule);
            cmd.Parameters.AddWithValue("@InvoiceId", userCourses.InvoiceId);
            cmd.Parameters.AddWithValue("@PurchasePrice", userCourses.PurchasePrice);
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<UserCourseResponse>> PurchasedCoursesPagedAsync(PageQueryRequest pageQuery, IDictionary<string, string>? fields = null)
        {
            var userCourses = new List<UserCourseResponse>();
            using (SqlConnection conn = new(ConnectionString))
            {
                try
                {
                    await conn.OpenAsync();
                    var queryBuilder = new StringBuilder();

                    const string query = @"
                        SELECT uc.course_id as course_id, 
                               uc.user_id as user_id, 
                               c.name as course_name,
                               c.image as course_image, 
                               uc.course_schedule as course_schedule, 
                               ct.id as category_id, 
                               ct.tag_name as category_name, 
                               uc.purchase_price as purchase_price 
                        FROM users_courses uc 
                        LEFT JOIN courses c ON c.id = uc.course_id
                        LEFT JOIN categories ct ON ct.id = c.category_id 
                        WHERE (
                            (UPPER(c.name) LIKE UPPER(@Keyword))
                            OR (UPPER(ct.name) LIKE UPPER(@Keyword))
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
                    string columnSorted = string.IsNullOrEmpty(pageQuery.SortBy) ? "c.name" : pageQuery.SortBy;
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
                            var userCourse = new UserCourseResponse()
                            {
                                CourseId = reader.GetGuid("course_id"),
                                CourseName = reader.GetString("course_name"),
                                CourseImage = reader.GetString("course_image"),
                                CategoryId = reader.GetGuid("category_id"),
                                Categoryname = reader.GetString("category_name"),
                                CourseSchedule = reader.GetDateTime("course_schedule"),
                                PurchasePrice = reader.GetDecimal("purchase_price")
                            };

                            userCourses.Add(userCourse);
                        }
                    }

                    return userCourses;
                }
                catch (System.Exception)
                {
                    throw;
                }
            }
        }

        public async Task<int> BulkInsertUserCoursesTaskAsync(SqlConnection conn, SqlTransaction transaction, List<UserCourses> userCourses)
        {
            DataTable dataTable = new();
            dataTable.Columns.Add("UserId", typeof(Guid));
            dataTable.Columns.Add("CourseId", typeof(Guid));
            dataTable.Columns.Add("CourseSchedule", typeof(DateTime));
            dataTable.Columns.Add("InvoiceId", typeof(int));
            dataTable.Columns.Add("PurchasePrice", typeof(decimal));

            foreach (var course in userCourses)
            {
                dataTable.Rows.Add(course.UserId, course.CourseId, course.CourseSchedule, course.InvoiceId, course.PurchasePrice);
            }

            using SqlBulkCopy bulkCopy = new(conn, SqlBulkCopyOptions.Default, transaction);
            bulkCopy.DestinationTableName = "users_courses";
            bulkCopy.ColumnMappings.Add("UserId", "user_id");
            bulkCopy.ColumnMappings.Add("CourseId", "course_id");
            bulkCopy.ColumnMappings.Add("CourseSchedule", "course_schedule");
            bulkCopy.ColumnMappings.Add("InvoiceId", "invoice_id");
            bulkCopy.ColumnMappings.Add("PurchasePrice", "purchase_price");
            await bulkCopy.WriteToServerAsync(dataTable);
            return 1;
        }

        // public async Task<List
    }
}