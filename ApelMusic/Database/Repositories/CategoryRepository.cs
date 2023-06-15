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
    public class CategoryRepository
    {
        private readonly IConfiguration _config;

        private readonly string ConnectionString;

        private readonly ILogger<CategoryRepository> _logger;

        public CategoryRepository(IConfiguration config, ILogger<CategoryRepository> logger)
        {
            _config = config;
            _logger = logger;
            this.ConnectionString = _config.GetConnectionString("DefaultConnection");
        }

        #region Method method untuk get category
        public async Task<List<Category>> FindCategoriesByAsync(string column = "", string value = "")
        {
            var categories = new List<Category>();
            using SqlConnection conn = new(ConnectionString);
            try
            {
                await conn.OpenAsync();
                var queryBuilder = new StringBuilder();

                const string query1 = @"
                    SELECT id,
                           tag_name,
                           name,
                           image,
                           banner_image,
                           category_description,
                           created_at,
                           updated_at,
                           inactive
                    FROM categories
                ";

                queryBuilder.Append(query1);

                if (!string.IsNullOrEmpty(column) && !string.IsNullOrWhiteSpace(column))
                {
                    queryBuilder.Append("WHERE ").Append(column).Append(" = @Value");
                }

                queryBuilder.Append(';');

                string finalQuery = queryBuilder.ToString();

                var cmd = new SqlCommand(finalQuery, conn);

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        var category = new Category()
                        {
                            Id = reader.GetGuid("id"),
                            TagName = reader.GetString("tag_name"),
                            Name = reader.GetString("name"),
                            Image = reader.GetString("image"),
                            BannerImage = reader.GetString("banner_image"),
                            CategoryDescription = reader.GetString("category_description"),
                            CreatedAt = reader.GetDateTime("created_at"),
                            UpdatedAt = reader.GetDateTime("updated_at"),
                        };

                        // Ngecek apakah category active/inactive
                        bool isActive = await reader.IsDBNullAsync(reader.GetOrdinal("inactive"));
                        if (!isActive)
                        {
                            category.Inactive = reader.GetDateTime("inactive");
                        }

                        categories.Add(category);
                    }
                }

                return categories;
            }
            catch (System.Exception)
            {
                await conn.CloseAsync();
                throw;
            }
        }

        public async Task<List<Category>> FindAllCategoriesAsync()
        {
            return await FindCategoriesByAsync();
        }

        public async Task<List<Category>> FindCategoryByIdAsync(Guid id)
        {
            return await FindCategoriesByAsync("id", id.ToString());
        }

        public async Task<List<Category>> FindCategoryByTagNameAsync(string tagName)
        {
            return await FindCategoriesByAsync("tag_name", tagName);
        }

        #endregion

        #region Method Untuk Insert Category
        public async Task<int> InsertCategoryTaskAsync(SqlConnection conn, SqlTransaction transaction, Category category)
        {
            const string query = @"
                INSERT INTO categories(id, tag_name, name, image, banner_image, category_description, created_at, updated_at) 
                VALUES (@Id, @TagName, @Name, @Image, @BannerImage, @CategoryDescription, @CreatedAt, @UpdatedAt)
            ";

            SqlCommand cmd = new(query, conn, transaction);
            cmd.Parameters.AddWithValue("@Id", category.Id);
            cmd.Parameters.AddWithValue("@TagName", category.TagName);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Image", category.Image);
            cmd.Parameters.AddWithValue("@BannerImage", category.BannerImage);
            cmd.Parameters.AddWithValue("@CategoryDescription", category.CategoryDescription ?? "");
            cmd.Parameters.AddWithValue("@CreatedAt", category.CreatedAt);
            cmd.Parameters.AddWithValue("@updatedAt", category.UpdatedAt);

            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> InsertCategoryAsync(Category category)
        {
            using SqlConnection conn = new(ConnectionString);
            await conn.OpenAsync();
            SqlTransaction transaction = (SqlTransaction)await conn.BeginTransactionAsync();
            try
            {
                _logger.LogInformation("Ini dari InsertCategoryAsync");
                _ = await InsertCategoryTaskAsync(conn, transaction, category);
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