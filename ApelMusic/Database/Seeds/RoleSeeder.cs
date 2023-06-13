using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Services;

namespace ApelMusic.Database.Seeds
{
    public class RoleSeeder
    {
        private readonly RoleService _roleService;

        private ILogger<RoleSeeder> _logger;

        public RoleSeeder(RoleService roleService, ILogger<RoleSeeder> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        // TODO: Lanjut bikin seeders
        public async Task<int> Run()
        {
            try
            {
                _ = await _roleService.InsertNewRoleAsync("ADMIN");
                _ = await _roleService.InsertNewRoleAsync("USER");
                return 1;
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}