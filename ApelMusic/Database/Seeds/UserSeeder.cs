using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Auth;
using ApelMusic.Services;

namespace ApelMusic.Database.Seeds
{
    public class UserSeeder
    {
        private readonly AuthService _authService;

        private readonly ILogger<UserSeeder> _logger;

        public UserSeeder(AuthService authService, ILogger<UserSeeder> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        public async Task<int> Run()
        {
            var users = new List<SeedUserRequest>()
            {
                new SeedUserRequest(){ Id = Guid.NewGuid(), Email = "admin@apelmusic.com",  FullName = "Admin", Password = "ApelMusic1033", Role = "ADMIN"},
                new SeedUserRequest(){ Id = Guid.NewGuid(), Email = "user1@hosting.com",  FullName = "User1", Password = "ApelMusic1033", Role = "USER"},
                new SeedUserRequest(){ Id = Guid.NewGuid(), Email = "user2@hosting.com",  FullName = "User2", Password = "ApelMusic1033", Role = "USER"},
                new SeedUserRequest(){ Id = Guid.NewGuid(), Email = "user3@hosting.com",  FullName = "User3", Password = "ApelMusic1033", Role = "USER"},
                new SeedUserRequest(){ Id = Guid.NewGuid(), Email = "user4@hosting.com",  FullName = "User4", Password = "ApelMusic1033", Role = "USER"},
            };
            try
            {
                users.ForEach(async user => await _authService.SeedRegisterUser(user));
                return 1;
            }
            catch (System.Exception)
            {

                throw;
            }
        }
    }
}