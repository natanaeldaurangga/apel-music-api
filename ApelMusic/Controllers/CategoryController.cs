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
    public class CategoryController : ControllerBase
    {
        private readonly CategoryService _categoryService;

        private readonly ILogger<CategoryController> _logger;

        public CategoryController(CategoryService categoryService, ILogger<CategoryController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> InsertCategory([FromForm] CreateCategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _categoryService.InsertCategoryAsync(request);
                return Ok();
            }
            catch (System.Exception)
            {
                // return StatusCode(StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var result = await _categoryService.GetAllCategoriesAsync();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}