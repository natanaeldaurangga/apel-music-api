using System.Text;
using System.Security.Cryptography;
using ApelMusic.Database.Repositories;
using ApelMusic.Entities;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using ApelMusic.DTOs.Auth;
using ApelMusic.DTOs.Admin;
using ApelMusic.DTOs;

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

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _config.GetSection("Jwt:Audience").Value,
                Issuer = _config.GetSection("Jwt:Issuer").Value
            };

            var emailClaim = new Claim(ClaimTypes.Email, user.Email!);
            var roleClaim = new Claim(ClaimTypes.Role, user.Role!.Name!);
            var idClaim = new Claim("id", user.Id.ToString());
            var nameClaim = new Claim(ClaimTypes.Name, user.FullName ?? "");

            tokenDescriptor.Subject = new ClaimsIdentity(new Claim[]{
                idClaim, nameClaim, emailClaim, roleClaim
            });

            tokenDescriptor.Expires = DateTime.Now.AddHours(6);

            tokenDescriptor.SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature);

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static RefreshTokenResponse GenerateRefreshToken()
        {
            return new RefreshTokenResponse
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                CreatedAt = DateTime.Now
            };
        }

        private static string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        #region GET USER OLEH ADMIN

        public async Task<PageQueryResponse<UserDataResponse>> UserPagedAsync(PageQueryRequest pageQuery)
        {
            var users = await _userRepo.UserPagedAsync(pageQuery);

            List<UserDataResponse> usersResponse = users.ConvertAll(u =>
            {
                return new UserDataResponse()
                {
                    Id = u.Id,
                    FullName = u.FullName!,
                    Email = u.Email!,
                    CreatedAt = u.CreatedAt,
                    VerifiedAt = u.VerifiedAt,
                    Inactive = u.Inactive
                };
            });

            return new PageQueryResponse<UserDataResponse>()
            {
                CurrentPage = pageQuery.CurrentPage,
                PageSize = pageQuery.PageSize,
                Items = usersResponse
            };
        }

        #endregion

        #region EDIT USER OLEH ADMIN
        public async Task<int> UpdateUserAsync(Guid userId, UserEditRequest request)
        {
            var users = await _userRepo.FindUserByIdAsync(userId);
            if (users.Count == 0) return 0;
            _logger.LogInformation("Length  User {}", users.Count);
            var user = users[0];

            user.FullName = request.FullName;
            user.Inactive = request.Inactive ? DateTime.UtcNow : null;

            return await _userRepo.UpdateUserAsync(user);
        }

        public async Task<int> ResetUserPassword(Guid userId, UserEditPasswordRequest request)
        {
            CreatePasswordHash(request.Password!, out byte[] passwordHash, out byte[] passwordSalt);

            var users = await _userRepo.FindUserByIdAsync(userId);
            if (users.Count == 0) return 0;
            var user = users[0];

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            return await _userRepo.UpdateUserAsync(user);
        }

        #endregion

        #region SERVICE RESET PASSWORD
        public async Task<string?> ResetPasswordRequestAsync(ResetPasswordEmailRequest request)
        {
            try
            {
                var resetToken = CreateRandomToken();
                _ = await _userRepo.UpdateResetTokenAsync(request.Email!, resetToken);
                return resetToken;
            }
            catch (Exception)
            {
                // return null;
                throw;
            }
        }

        public async Task<int> ResetPasswordAsync(string token, ResetPasswordRequest request)
        {
            CreatePasswordHash(request.Password!, out byte[] passwordHash, out byte[] passwordSalt);
            return await _userRepo.ResetPasswordAsync(token, passwordHash, passwordSalt);
        }
        #endregion

        #region Tambah user oleh admin

        public async Task<int> InsertUserByAdminAsync(CreateUserByAdminRequest request)
        {
            CreatePasswordHash(request.Password!, out byte[] passwordHash, out byte[] passwordSalt);

            var role = await _roleService.GetRoleByNameAsync("USER");

            if (role != null)
            {
                User user = new()
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    RoleId = role.Id,
                    Role = role,
                    VerifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _ = await _userRepo.InsertUserAsync(user);
                return 1;
            }

            return 0;
        }

        public async Task<int> SeedRegisterUser(SeedUserRequest request)
        {
            CreatePasswordHash(request.Password!, out byte[] passwordHash, out byte[] passwordSalt);

            var role = await _roleService.GetRoleByNameAsync(request.Role!);

            if (role != null)
            {
                User user = new()
                {
                    Id = request.Id,
                    Email = request.Email,
                    FullName = request.FullName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    RoleId = role.Id,
                    Role = role,
                    VerifiedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _ = await _userRepo.InsertUserAsync(user);
                return 1;
            }

            return 0;
        }

        public async Task<string?> RegisterNewUserAsync(RegistrationRequest request)
        {
            CreatePasswordHash(request.Password!, out byte[] passwordHash, out byte[] passwordSalt);

            var role = await _roleService.GetRoleByNameAsync("USER");
            // _logger.LogInformation("Role Id: {}", role?.Id);
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
        #endregion

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            var users = await _userRepo.FindUserByEmailAsync(request.Email!);
            if (users.Count < 1) return null; // JIka email tidak ditemukan
            var user = users[0];

            if (!VerifyPasswordHash(request.Password!, user.PasswordHash!, user.PasswordSalt!))
            {
                return null; // Jika password salah
            }

            if (user.Inactive != null)
            {
                return new LoginResponse()
                {
                    Error = "INACTIVE"
                };
            }

            if (user.VerifiedAt == null)
            {
                return new LoginResponse()
                {
                    Error = "UNVERIFIED"
                };
            }

            string token = GenerateJwtToken(user);

            var refreshToken = GenerateRefreshToken();

            await _userRepo.UpdateRefreshTokenAsync(user.Id, refreshToken);

            return new LoginResponse()
            {
                Id = user.Id,
                Name = user.FullName,
                Email = user.Email,
                JwtToken = token,
                Role = user.Role!.Name,
                RefreshToken = refreshToken,
                User = user
            };
        }

        public async Task<int> SetInactiveUser(Guid id, bool inactive = false)
        {
            return await _userRepo.SetInactiveAsync(id, inactive);
        }

        public async Task<bool> VerifyUserAsync(string token)
        {
            return await _userRepo.VerifyUserAsync(token);
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

        public async Task<UserEditResponse?> FindUserByIdAsync(Guid userId)
        {
            var results = await _userRepo.FindUserByIdAsync(userId);

            if (results.Count == 0) return null;

            var result = results[0];

            return new UserEditResponse()
            {
                Id = result.Id,
                Email = result.Email!,
                FullName = result.FullName!,
                Inactive = result.Inactive
            };
        }

    }
}