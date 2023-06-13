using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Migrations;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Migration : ControllerBase
    {
        private readonly MainMigrations _migrations;

        public Migration(MainMigrations migrations)
        {
            _migrations = migrations;
        }

        [HttpPost]
        public async Task<IActionResult> Migrate()
        {
            try
            {
                var result = await _migrations.ExecuteMigrationsQueriesAsync();
                return Ok();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPut("AddConstraint")]
        public async Task<IActionResult> AddCosntraints()
        {
            try
            {
                var result = await _migrations.ExecuteAddConstraintsAsync();
                return Ok();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        // TODO: Bikin repo, service, sama controller buat user
        [HttpDelete]
        public async Task<IActionResult> DropAllTables()
        {
            try
            {
                var result = await _migrations.ExecuteDeleteAllTableAsync();
                return NoContent();
            }
            catch (System.Exception)
            {
                // return StatusCode(500);
                throw;
            }
        }

        [HttpDelete("DropAllConstraints")]
        public async Task<IActionResult> DropAllConstraints()
        {
            try
            {
                var result = await _migrations.ExecuteDeleteConstraints();
                return NoContent();
            }
            catch (System.Exception)
            {
                // return StatusCode(500);
                throw;
            }
        }

    }
}