using ApelMusic.DTOs.Courses;
using ApelMusic.Services;
using Microsoft.AspNetCore.Mvc;
using ApelMusic.DTOs;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

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

        // TODO: Lanjut tes update course
        [HttpPut("{courseId}"), Authorize("ADMIN")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateCourse([FromRoute] Guid courseId, [FromForm] UpdateCourseRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _courseService.UpdateCourseAsync(courseId, request);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data"), Authorize("ADMIN")]
        public async Task<IActionResult> InsertCourse([FromForm] CreateCourseRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _courseService.InsertCourseAsync(request);
                return Ok();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses([FromQuery] PageQueryRequest request)
        {
            try
            {
                var result = await _courseService.PaginateCourseAsync(request);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("GroupByCategory/{categoryId}")]
        public async Task<IActionResult> GetCoursesByCategory([FromQuery] PageQueryRequest request, [FromRoute] Guid categoryId, [FromQuery] Guid exceptedCourseId)
        {
            try
            {
                var fields = new Dictionary<string, string>()
                {
                    {"category_id", categoryId.ToString()}
                };

                var exceptedFields = new Dictionary<string, string>()
                {
                    {"c.id", exceptedCourseId.ToString()}
                };

                var result = await _courseService.PaginateCourseAsync(request, fields, exceptedFields);
                // if (result!.Items.Count == 0)
                // {
                //     return NotFound();
                // }
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{courseId}"), Authorize]
        public async Task<IActionResult> GetCourseById([FromRoute] Guid courseId)
        {
            try
            {
                ClaimsPrincipal user = HttpContext.User;
                Guid userId = Guid.Parse(user.FindFirstValue("id"));
                var result = await _courseService.FindCourseById(courseId, userId);
                if (result.Count == 0) return NotFound();
                return Ok(result[0]);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost("GetByIds"), Authorize]
        public async Task<IActionResult> GetCourseByIds([FromBody] List<Guid> courseIds)
        {
            try
            {
                var result = await _courseService.FindCourseByIds(courseIds);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}