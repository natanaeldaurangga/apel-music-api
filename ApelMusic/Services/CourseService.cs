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

        private readonly CourseScheduleService _scheduleService;

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
            try
            {
                var course = new Course()
                {
                    Id = request.Id,
                    Name = request.Name,
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
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<PageQueryResponse<CourseSummaryResponse>?> PaginateCourseAsync(PageQueryRequest request, string column = "", string value = "")
        {
            var courses = await _courseRepo.CoursePagedAsync(request, column, value);

            List<CourseSummaryResponse> coursesSummary = courses.ConvertAll(course =>
            {
                return new CourseSummaryResponse()
                {
                    Id = course.Id,
                    Name = course.Name!,
                    Category = new CategorySummaryResponse() { Id = course.Category!.Id, Name = course.Category!.Name! },
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

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _courseRepo.FindAllCoursesAsync();
        }

        public async Task<List<Course>> FindCourseById(Guid courseId)
        {
            return await _courseRepo.FindCourseById(courseId);
        }

        public async Task<int> InsertCourseAsync(CreateCourseRequest request)
        {
            try
            {
                var imageName = await _imageServices.UploadImageAsync(request.Image!, folder: "Upload");

                var course = new Course()
                {
                    Id = Guid.NewGuid(),
                    Name = request.Name,
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
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}