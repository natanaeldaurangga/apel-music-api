using System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;

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
    }
}