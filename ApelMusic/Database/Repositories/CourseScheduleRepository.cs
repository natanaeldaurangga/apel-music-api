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