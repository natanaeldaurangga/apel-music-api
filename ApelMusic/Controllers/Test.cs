using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Test : ControllerBase
    {

        private readonly RoleRepository _roleRepo;

        public Test(RoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
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
    }
}