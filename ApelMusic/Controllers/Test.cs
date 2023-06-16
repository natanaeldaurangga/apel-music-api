using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Test : ControllerBase
    {
        private readonly RoleRepository _roleRepo;

        private readonly IWebHostEnvironment _env;

        public Test(RoleRepository roleRepo, IWebHostEnvironment env)
        {
            _roleRepo = roleRepo;
            _env = env;

        }

        [HttpGet]
        public async Task<IActionResult> GetData()
        {
            // var result = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            var result = await _roleRepo.GetByNameAsync("USER");
            if (result?.Count > 0)
            {
                return Ok(result[0]);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("GetEnv")]
        public async Task<IActionResult> GetEnv()
        {
            return Ok(_env.ContentRootPath);
        }

        [HttpGet("OnlyUser"), Authorize("USER")]
        public IActionResult GetUser()
        {
            return Ok("Endpoint ini hanya bisa diakses oleh user.");
        }

        [HttpGet("OnlyAdmin"), Authorize("ADMIN")]
        public IActionResult GetAdmin()
        {
            return Ok("Endpoint ini hanya bisa diakses oleh admin.");
        }

    }
}