using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Courses;
using ApelMusic.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CourseController : ControllerBase
    {
        private readonly CourseService _courseService;

        private readonly ILogger<CourseController> _logger;

        public CourseController(CourseService courseService, ILogger<CourseController> logger)
        {
            _courseService = courseService;
            _logger = logger;
            
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> InsertCourse([FromForm] CreateCourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _courseService.InsertCourseAsync(request);
                return Ok("Data berhasil ditambahkan.");
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses()
        {
            try
            {
                return Ok(await _courseService.GetAllCoursesAsync());
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}