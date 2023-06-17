using System.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;
using System.Data;
using ApelMusic.DTOs;

namespace ApelMusic.Database.Repositories
{
    public class CourseRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        private readonly CourseScheduleRepository _scheduleRepo;

        public CourseRepository(IConfiguration config, CourseScheduleRepository scheduleRepo)
        {
            _config = config;
            _scheduleRepo = scheduleRepo;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region Method untuk get course

        public async Task<int> GetRowCount()
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            const string query = "SELECT COUNT(*) FROM courses";

            using SqlCommand command = new(query, conn);
            // Execute the scalar query to get the row count
            return (int)command.ExecuteScalar();
        }

        // Fungsi ini mereturn 2 tipe data yaitu List courses dan juga totalRows dari query (int)
        public async Task<List<Course>> CoursePagedAsync(PageQueryRequest pageQuery, string column = "", string value = "")
        {
            var courses = new List<Course>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query = @"
                    SELECT c.id as id,
                           c.name as name,
                           c.category_id as category_id,
                           c.image as image,
                           c.description as description,
                           c.price as price,
                           c.created_at as created_at,
                           c.updated_at as updated_at,
                           c.inactive as inactive,
                           ct.tag_name as category_tag_name
                    FROM courses c 
                    LEFT JOIN categories ct ON ct.id = c.category_id 
                    WHERE (c.name LIKE @Name) 
                ";
                queryBuilder.Append(query);

                // Mengecek apakah pemanggilan fungsi menspesifikan column yang dicari
                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column))
                {
                    queryBuilder.Append("AND (").Append(column).Append(" = @Value)");
                }

                // Menentukan Ascending atau Descending
                string direction = string.Equals(pageQuery.Direction, "ASC", StringComparison.OrdinalIgnoreCase) ? "ASC" : "DESC"; // komparasi string tanpa melihat case nya

                // Menentukan kolom mana yang akan disorting
                string columnSorted = string.IsNullOrEmpty(pageQuery.SortBy) ? "c.name" : pageQuery.SortBy;
                string orderByQuery = $"ORDER BY {columnSorted} {direction} ";

                queryBuilder.Append(orderByQuery);

                string pagingQuery = @"
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY 
                ";

                queryBuilder.Append(pagingQuery);

                string finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                int offset = (pageQuery.CurrentPage - 1) * pageQuery.PageSize;
                string keyword = "%" + pageQuery.Keyword + "%";
                cmd.Parameters.AddWithValue("@Name", keyword);

                cmd.Parameters.AddWithValue("@OrderBy", pageQuery.SortBy);

                cmd.Parameters.AddWithValue("@Offset", offset);
                cmd.Parameters.AddWithValue("@PageSize", pageQuery.PageSize);

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrEmpty(column))
                {
                    cmd.Parameters.AddWithValue("@Value", string.IsNullOrEmpty(value) ? DBNull.Value : value);
                }

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var course = new Course()
                        {
                            Id = reader.GetGuid("id"),
                            Name = reader.GetString("name"),
                            CategoryId = reader.GetGuid("category_id"),
                            Image = reader.GetString("image"),
                            Price = reader.GetDecimal("price"),
                            Description = reader.GetString("description"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at")
                        };

                        var category = new Category()
                        {
                            Id = reader.GetGuid("category_id"),
                            TagName = reader.GetString("category_tag_name")
                        };

                        bool isActiveCourse = await reader.IsDBNullAsync(reader.GetOrdinal("inactive"));
                        if (!isActiveCourse)
                        {
                            course.Inactive = reader.GetDateTime("inactive");
                        }

                        course.Category = category;

                        courses.Add(course);
                    }
                }
                return courses;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private async Task<List<Course>> FindCoursesByAsync(string column = "", string value = "")
        {
            var courses = new List<Course>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query1 = @"
                    SELECT c.id as id,
                           c.name as name,
                           c.category_id as category_id,
                           c.image as image,
                           c.description as description,
                           c.price as price,
                           c.created_at as created_at,
                           c.updated_at as updated_at,
                           c.inactive as inactive,
                           ct.tag_name as category_tag_name
                    FROM courses c
                    LEFT JOIN categories ct ON category_id = ct.id 
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
                        var course = new Course()
                        {
                            Id = reader.GetGuid("id"),
                            Name = reader.GetString("name"),
                            CategoryId = reader.GetGuid("category_id"),
                            Image = reader.GetString("image"),
                            Description = reader.GetString("description"),
                            Price = reader.GetDecimal("price"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at")
                        };

                        course.CourseSchedules = await _scheduleRepo.GetScheduleByCourseIdAsync(course.Id);

                        var category = new Category()
                        {
                            Id = reader.GetGuid("category_id"),
                            TagName = reader.GetString("category_tag_name"),
                        };

                        bool isActiveCourse = await reader.IsDBNullAsync(reader.GetOrdinal("inactive"));
                        if (!isActiveCourse)
                        {
                            course.Inactive = reader.GetDateTime("inactive");
                        }

                        course.Category = category;

                        courses.Add(course);
                    }
                }
                return courses;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<Course>> FindAllCoursesAsync()
        {
            return await FindCoursesByAsync();
        }

        public async Task<List<Course>> FindCourseById(Guid courseId)
        {
            return await FindCoursesByAsync("c.id", courseId.ToString());
        }

        #endregion

        #region Method untuk insert course
        public async Task<int> InsertCourseTaskAsync(SqlConnection conn, SqlTransaction transaction, Course course)
        {
            const string query = @"
                INSERT INTO courses(id, name, category_id, image, description, price, created_at, updated_at) 
                VALUES (@Id, @Name, @CategoryId, @Image, @Description, @Price, @CreatedAt, @UpdatedAt);
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", course.Id);
            cmd.Parameters.AddWithValue("@Name", course.Name);
            cmd.Parameters.AddWithValue("@CategoryId", course.CategoryId);
            cmd.Parameters.AddWithValue("@Image", course.Image);
            cmd.Parameters.AddWithValue("@Price", course.Price);
            cmd.Parameters.AddWithValue("@CreatedAt", course.CreatedAt);
            cmd.Parameters.AddWithValue("@UpdatedAt", course.UpdatedAt);

            if (course.Description == null)
            {
                cmd.Parameters.AddWithValue("@Description", "");
            }
            else
            {
                cmd.Parameters.AddWithValue("@Description", course.Description);
            }

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
                _ = await _scheduleRepo.BulkInsertSchedulesTaskAsync(conn, transaction, course.CourseSchedules!);
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