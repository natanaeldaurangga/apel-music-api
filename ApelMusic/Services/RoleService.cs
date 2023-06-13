using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class RoleService
    {
        private readonly RoleRepository _roleRepo;

        public RoleService(RoleRepository roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<int> InsertNewRoleAsync(string name)
        {
            try
            {
                var role = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };
                return await _roleRepo.InsertRoleAsync(role);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            try
            {
                var result = await _roleRepo.GetByNameAsync(name);
                if (result?.Count > 0)
                {
                    return result[0];
                }
                else
                {
                    return null;
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}