using System.Collections;
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
        public async Task<List<Course>> FindCourseByIdInAsync(List<Guid> ids)
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
                    LEFT JOIN categories ct ON ct.id = c.category_id 
                ";

                queryBuilder.Append(query1);

                if (ids != null)
                {
                    queryBuilder.Append(" WHERE ").Append("c.id").Append(" IN (");
                    for (int i = 0; i < ids.Count; i++)
                    {
                        queryBuilder.Append("@Value").Append(i);
                        if (i < ids.Count - 1)
                        {
                            queryBuilder.Append(", ");
                        }
                    }
                    queryBuilder.Append(") ");
                }

                queryBuilder.Append(';');

                var finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                if (ids != null)
                {
                    for (int i = 0; i < ids.Count; i++)
                    {
                        cmd.Parameters.AddWithValue("@Value" + i, ids[i]);
                    }
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

        // Method untuk paginasi course dengan menggunakan
        public async Task<List<Course>> CoursePagedAsync(PageQueryRequest pageQuery, IDictionary<string, string>? fields = null, IDictionary<string, string>? exceptedFields = null)
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
                    WHERE (UPPER(c.name) LIKE UPPER(@Name)) 
                ";
                queryBuilder.Append(query);

                // Build Query untuk pasangan column dan value (EQUAL)
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

                // Build Query untuk pasangan column dan value (NOT EQUAL)
                if (exceptedFields != null)
                {
                    queryBuilder.Append("AND (");
                    for (int i = 0; i < exceptedFields.Count; i++)
                    {
                        KeyValuePair<string, string> colVal = exceptedFields.ElementAt(i);
                        queryBuilder.Append(colVal.Key).Append(" != @ExcValue").Append(i);

                        if (i != exceptedFields.Count - 1) queryBuilder.Append(" AND ");
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
                cmd.Parameters.AddWithValue("@Name", keyword ?? "");

                cmd.Parameters.AddWithValue("@OrderBy", pageQuery.SortBy ?? "name");

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

                // Memasang parameter pada tiap @ExcValue
                if (exceptedFields != null)
                {
                    for (int i = 0; i < exceptedFields.Count; i++)
                    {
                        KeyValuePair<string, string> colVal = exceptedFields.ElementAt(i);
                        cmd.Parameters.AddWithValue("@ExcValue" + i, colVal.Value);
                    }
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

        // Method untuk find course dengan menggunakan column field pair (WHERE column = field)
        public async Task<List<Course>> FindCourseByIdAsync(Guid courseId, Guid? userId = null)
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
                    WHERE c.id = @CourseId
                ";

                queryBuilder.Append(query1);

                queryBuilder.Append(';');

                string finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                cmd.Parameters.AddWithValue("@CourseId", courseId);

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

                        course.CourseSchedules = await _scheduleRepo.GetScheduleByCourseIdAsync(course.Id, userId);

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