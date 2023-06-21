using System.Security.Claims;
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

        private readonly PaymentMethodRepository _paymentRepo;

        private readonly IWebHostEnvironment _env;

        public Test(RoleRepository roleRepo, PaymentMethodRepository paymentRepo, IWebHostEnvironment env)
        {
            _roleRepo = roleRepo;
            _paymentRepo = paymentRepo;
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

        [HttpGet("TestDoang")]
        public async Task<IActionResult> GetEnv()
        {
            // var inp = new Dictionary<string, string>()
            // {
            //     {"id", Guid.NewGuid().ToString()}
            // };
            var num = 10;
            var formattedNum = "AP" + num.ToString("D6");
            return Ok(formattedNum);
        }

        [HttpGet("OnlyUser"), Authorize("USER")]
        public IActionResult GetUser()
        {
            ClaimsPrincipal user = HttpContext.User;
            string userId = user.FindFirstValue("id");
            string name = user.FindFirstValue(ClaimTypes.Name);
            string email = user.FindFirstValue(ClaimTypes.Email);
            string role = user.FindFirstValue(ClaimTypes.Role);
            return Ok(new Dictionary<string, string>() {
                {"userId", userId},
                {"name", name},
                {"email", email},
                {"role", role},
            });
        }

        [HttpGet("OnlyAdmin"), Authorize("ADMIN")]
        public IActionResult GetAdmin()
        {
            return Ok("Endpoint ini hanya bisa diakses oleh admin.");
        }

    }
}