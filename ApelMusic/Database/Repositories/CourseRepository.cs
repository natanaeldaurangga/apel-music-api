using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;

namespace ApelMusic.Database.Repositories
{
    public class CourseRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        public CourseRepository(IConfiguration config)
        {
            _config = config;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region Method untuk get all courses
        // public async Task<int> Insert

        #endregion

        #region Method untuk insert course
        public async Task<int> InsertCourseTaskAsync(SqlConnection conn, SqlTransaction transaction, Course course)
        {
            const string query = @"
                INSERT INTO courses(id, name, category_id, image, description, created_at, updated_at) 
                VALUES (@Id, @Name, @CategoryId, @Image, @Description, @CreatedAt, @UpdatedAt);
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", course.Id);
            cmd.Parameters.AddWithValue("@Name", course.Name);
            cmd.Parameters.AddWithValue("@CategoryId", course.CategoryId);
            cmd.Parameters.AddWithValue("@Image", course.Image);
            cmd.Parameters.AddWithValue("@Description", course.Description);
            cmd.Parameters.AddWithValue("@CreatedAt", course.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", course.UpdatedAt);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertCourseAsync(Course course)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _ = await InsertCourseTaskAsync(conn, transaction, course);
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