using System.Text;
using System.Security.Cryptography;
using ApelMusic.Database.Repositories;
using ApelMusic.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using ApelMusic.DTOs.Auth;

namespace ApelMusic.Services
{
    public class AuthService
    {
        private readonly UserRepository _userRepo;
        private readonly RoleService _roleService;
        private readonly IConfiguration _config;
        private readonly ILogger<AuthService> _logger;

        public AuthService(UserRepository userRepo, RoleService roleService, IConfiguration config, ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _config = config;
            _roleService = roleService;
            _logger = logger;
        }

        // Generate PasswordHash dan juga PasswordSalt
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA256();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        // Fungsi untuk mencocokan input password dari login request dengan passwordHash yang diambil dari database
        private static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA256(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection("Jwt:Key").Value);

            var tokenDescriptor = new SecurityTokenDescriptor();
            tokenDescriptor.Audience = _config.GetSection("Jwt:Audience").Value;
            tokenDescriptor.Issuer = _config.GetSection("Jwt:Issuer").Value;

            var emailClaim = new Claim(ClaimTypes.Email, user.Email!);
            var roleClaim = new Claim(ClaimTypes.Role, user.Role!.Name!);

            tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]{
                emailClaim, roleClaim
            });

            tokenDescriptor.Expires = DateTime.Now.AddHours(5);

            tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static RefreshTokenResponse GenerateRefreshToken()
        {
            var refreshToken = new RefreshTokenResponse
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
            };

            return refreshToken;
        }

        private static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        public async Task<string?> RegisterNewUser(RegistrationRequest request)
        {
            CreatePasswordHash(request.Password!, out byte[] passwordHash, out byte[] passwordSalt);

            var role = await _roleService.GetRoleByNameAsync("USER");
            _logger.LogInformation("Role Id: {}", role?.Id);
            if (role != null)
            {
                var verificationToken = CreateRandomToken();
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    RoleId = role?.Id,
                    Role = role,
                    VerificationToken = verificationToken,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                var result = await _userRepo.InsertUserAsync(user);
                _logger.LogInformation("Verification token: ", verificationToken);
                if (result) return verificationToken;
            }
            return null;
        }

        public async Task<List<User>> FindAllUserAsync()
        {
            return await _userRepo.FindAllUserAsync();
        }

        public async Task<bool> IsUserAlreadyUsed(string email)
        {
            var users = await _userRepo.FindUserByEmailAsync(email);
            return users.Count > 0;
        }

        public async Task<List<User>> FindUserByEmail(string email)
        {
            var users = await _userRepo.FindUserByEmailAsync(email);
            return users;
        }

    }
}