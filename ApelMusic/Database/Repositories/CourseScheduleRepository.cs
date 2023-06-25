using System.Data;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;

namespace ApelMusic.Database.Repositories
{
    public class CourseScheduleRepository
    {
        private readonly IConfiguration _config;

        private readonly ILogger<CourseScheduleRepository> _logger;

        private readonly string? ConnectionString;

        public CourseScheduleRepository(IConfiguration config, ILogger<CourseScheduleRepository> logger)
        {
            _config = config;
            _logger = logger;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<CourseSchedule>> GetScheduleByCourseIdAsync(Guid courseId, Guid? userId = null)
        {
            List<CourseSchedule> schedules = new();

            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                const string query = @"
                SELECT * FROM course_schedules cs 
                WHERE 
                    cs.course_id = @CourseId
                AND
                    cs.course_date  NOT IN (
                        SELECT course_schedule 
                        FROM users_courses cs 
                        WHERE 
                            cs.user_id = @UserId
                            AND cs.course_id = @CourseId
                    );
                ";

                var cmd = new SqlCommand(query, conn);
                if (userId == null)
                {
                    cmd.Parameters.AddWithValue("@UserId", "");
                }
                else
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                }
                cmd.Parameters.AddWithValue("@CourseId", courseId);
                using SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    CourseSchedule schedule = new()
                    {
                        Id = reader.GetGuid("id"),
                        CourseId = reader.GetGuid("course_id"),
                        CourseDate = reader.GetDateTime("course_date")
                    };
                    schedules.Add(schedule);
                }

                return schedules;
            }
            catch (System.Exception)
            {

                throw;
            }
        }

        public async Task<int> BulkInsertSchedulesTaskAsync(SqlConnection conn, SqlTransaction transaction, List<CourseSchedule> schedules)
        {
            DataTable table = new();
            table.Columns.Add("Id", typeof(Guid));
            table.Columns.Add("CourseId", typeof(Guid));
            table.Columns.Add("CourseDate", typeof(DateTime));

            schedules.ForEach(schedule => table.Rows.Add(schedule.Id, schedule.CourseId, schedule.CourseDate));

            using SqlBulkCopy bulkCopy = new(conn, SqlBulkCopyOptions.Default, transaction);
            bulkCopy.DestinationTableName = "course_schedules";
            bulkCopy.ColumnMappings.Add("Id", "id");
            bulkCopy.ColumnMappings.Add("CourseId", "course_id");
            bulkCopy.ColumnMappings.Add("CourseDate", "course_date");
            await bulkCopy.WriteToServerAsync(table);
            _logger.LogInformation("Schedules Insert: ", schedules.Count);
            return schedules.Count;
        }

        public async Task<int> BulkInsertSchedulesAsync(List<CourseSchedule> schedules)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                await BulkInsertSchedulesTaskAsync(conn, transaction, schedules);
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