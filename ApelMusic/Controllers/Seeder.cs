using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Seeds;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Seeder : ControllerBase
    {
        private readonly RoleSeeder _roleSeed;

        private readonly CourseSeeder _courseSeeder;

        private readonly ILogger<Seeder> _logger;

        public Seeder(RoleSeeder roleSeed, CourseSeeder courseSeeder, ILogger<Seeder> logger)
        {
            _roleSeed = roleSeed;
            _courseSeeder = courseSeeder;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Seed()
        {
            try
            {
                _ = await _roleSeed.Run();
                _ = await _courseSeeder.Run();
                return NoContent();
            }
            catch (System.Exception)
            {
                // _logger.LogError(e.Message);
                // _logger.LogError(e.StackTrace);
                // return StatusCode(500);
                throw;
            }
        }
    }
}