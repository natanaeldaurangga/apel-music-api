using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs;
using ApelMusic.DTOs.Courses;
using ApelMusic.Entities;
using ApelMusic.Utility;

namespace ApelMusic.Services
{
    public class CourseService
    {
        private readonly CourseRepository _courseRepo;

        private readonly ImageServices _imageServices;

        private readonly ILogger<CourseService> _logger;

        public CourseService(CourseRepository courseRepo, ImageServices imageServices, ILogger<CourseService> logger)
        {
            _courseRepo = courseRepo;
            _imageServices = imageServices;
            _logger = logger;
        }

        public async Task<int> InsertSeedCourseAsync(SeedCourseRequest request)
        {
            var course = new Course()
            {
                Id = request.Id,
                Name = request.Name!,
                CategoryId = request.CategoryId,
                Image = request.Image,
                Description = request.Description,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            course.CourseSchedules = request.Schedules.ConvertAll(schedule =>
            {
                return new CourseSchedule()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    CourseDate = schedule,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
            });

            _logger.LogInformation("Schedule Length: ", course.CourseSchedules.Count);

            return await _courseRepo.InsertCourseAsync(course);
        }

        public async Task<PageQueryResponse<CourseSummaryResponse>?> PaginateCourseAsync(PageQueryRequest request, IDictionary<string, string>? fields = null, IDictionary<string, string>? exceptedFields = null)
        {
            var courses = await _courseRepo.CoursePagedAsync(request, fields, exceptedFields);

            List<CourseSummaryResponse> coursesSummary = courses.ConvertAll(course =>
            {
                return new CourseSummaryResponse()
                {
                    Id = course.Id,
                    Name = course.Name!,
                    Category = new CategorySummaryResponse() { Id = course.Category!.Id, TagName = course.Category!.TagName! },
                    ImageName = course.Image!,
                    Price = course.Price
                };
            });

            return new PageQueryResponse<CourseSummaryResponse>()
            {
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                Items = coursesSummary
            };
        }

        public async Task<List<Course>> FindCourseById(Guid courseId, Guid? userId = null)
        {
            return await _courseRepo.FindCourseByIdAsync(courseId, userId);
        }

        public async Task<List<CourseSummaryResponse>> FindCourseByIds(List<Guid> ids)
        {
            var results = await _courseRepo.FindCourseByIdInAsync(ids);
            return results.ConvertAll(course =>
            {
                return new CourseSummaryResponse()
                {
                    Id = course.Id,
                    Name = course.Name!,
                    ImageName = course.Image!,
                    Price = course.Price,
                    Category = new CategorySummaryResponse()
                    {
                        Id = course.Category!.Id,
                        TagName = course.Category!.Name!
                    }
                };
            });
        }

        public async Task<int> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request)
        {
            try
            {
                var courses = await _courseRepo.FindCourseByIdAsync(courseId);
                if (courses.Count == 0) return 0;
                var course = courses[0];
                course.Name = request.Name!;
                course.CategoryId = request.CategoryId;
                course.Price = request.Price;

                // handle image
                if (request.Image != null)
                {
                    var oldImage = course.Image!.Replace("%5C", "\\"); // Mengganti separator untuk url dengan separator biasa untuk mencari folder
                    _imageServices.DeleteImage(oldImage);
                    course.Image = await _imageServices.UploadImageAsync(request.Image!, folder: "Upload");
                }

                return await _courseRepo.UpdateCourseAsync(course);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> InsertCourseAsync(CreateCourseRequest request)
        {
            var imageName = await _imageServices.UploadImageAsync(request.Image!, folder: "Upload");

            var course = new Course()
            {
                Id = Guid.NewGuid(),
                Name = request.Name!,
                Image = imageName,
                CategoryId = request.CategoryId,
                Description = request.Description,
                Price = request.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            course.CourseSchedules = request.Schedules.ConvertAll(schedule =>
            {
                return new CourseSchedule()
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    CourseDate = schedule,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
            });

            return await _courseRepo.InsertCourseAsync(course);
        }
    }
}