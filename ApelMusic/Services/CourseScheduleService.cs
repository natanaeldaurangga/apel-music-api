using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs.Courses;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class CourseScheduleService
    {
        private readonly CourseScheduleRepository _scheduleRepo;

        private readonly ILogger<CourseScheduleService> _logger;

        public CourseScheduleService(CourseScheduleRepository scheduleRepo, ILogger<CourseScheduleService> logger)
        {
            _scheduleRepo = scheduleRepo;
            _logger = logger;
        }

        public async Task BulkInsertSchedulesAsync(List<CreateCourseScheduleRequest> requests)
        {
            try
            {
                var schedules = requests.ConvertAll(req =>
                {
                    return new CourseSchedule()
                    {
                        Id = Guid.NewGuid(),
                        CourseId = req.CourseId,
                        CourseDate = req.CourseDate,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                });
                await _scheduleRepo.BulkInsertSchedulesAsync(schedules);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}