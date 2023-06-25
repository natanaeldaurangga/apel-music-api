using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs.Courses;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class CategoryService
    {
        private readonly CategoryRepository _categoryRepo;

        private readonly ImageServices _imageServices;

        private readonly ILogger<CategoryService> _logger;

        public CategoryService(CategoryRepository categoryRepo, ImageServices imageServices, ILogger<CategoryService> logger)
        {
            _categoryRepo = categoryRepo;
            _imageServices = imageServices;
            _logger = logger;
        }

        public async Task<int> InsertSeedCategoryAsync(SeedCategoryRequest request)
        {
            try
            {
                var category = new Category()
                {
                    Id = request.Id,
                    TagName = request.TagName ?? "",
                    Name = request.Name,
                    Image = request.Image,
                    BannerImage = request.BannerImage,
                    CategoryDescription = request.CategoryDescription,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return await _categoryRepo.InsertCategoryAsync(category);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<Category?> FindCategoryByIdAsync(Guid id)
        {
            try
            {
                var result = await _categoryRepo.FindCategoryByIdAsync(id);
                if (result.Count == 0) return null;
                return result[0];
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _categoryRepo.FindAllCategoriesAsync();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<int> InsertCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                var imageName = await _imageServices.UploadImageAsync(request.Image!, folder: "Upload");
                var bannerName = await _imageServices.UploadImageAsync(request.BannerImage!, folder: "Upload");
                var category = new Category()
                {
                    Id = Guid.NewGuid(),
                    TagName = request.TagName!,
                    Name = request.Name,
                    Image = imageName,
                    BannerImage = bannerName,
                    CategoryDescription = request.CategoryDescription,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return await _categoryRepo.InsertCategoryAsync(category);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}