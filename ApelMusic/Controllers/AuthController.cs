using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Auth;
using ApelMusic.Email;
using ApelMusic.Email.TemplateModel;
using ApelMusic.Entities;
using ApelMusic.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        private readonly EmailService _emailService;

        private readonly IConfiguration _config;

        public AuthController(AuthService authService, EmailService emailService, IConfiguration config)
        {
            _authService = authService;
            _emailService = emailService;
            _config = config;
        }

        private async Task<int> SetRefreshToken(RefreshTokenResponse newRefreshToken, User user)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };

            Response.Cookies.Append("refreshToken", newRefreshToken.Token!, cookieOptions);
            return 1;
        }

        [HttpPost("Registration")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _authService.IsUserAlreadyUsed(request.Email!))
            {
                return Conflict("Email Already Used.");
            }

            // Mengambil base url dari project
            var verificationUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/api/Auth/VerifyEmail/";

            try
            {
                var token = await _authService.RegisterNewUserAsync(request);

                if (!string.IsNullOrEmpty(token) && !string.IsNullOrWhiteSpace(token))
                {
                    var emailVerification = new EmailVerificationModel()
                    {
                        EmailAddress = request.Email,
                        Url = verificationUrl + token,
                        VerificationToken = token
                    };

                    var emailAddresses = new List<string>
                    {
                        request.Email!
                    };

                    var model = new EmailModel(emailAddresses, "Verifikasi Email",
                        _emailService.GetEmailTemplate("VerifyEmail", emailVerification)
                    );

                    bool sended = await _emailService.SendAsync(model, new CancellationToken());

                    return Ok("Akun anda sudah terdaftar, Silahkan cek email anda untuk melakukan verifikasi");
                }
                return NotFound();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("VerifyEmail/{token}")]
        public async Task<IActionResult> VerifyUser([FromRoute] string token)
        {
            try
            {
                bool result = await _authService.VerifyUserAsync(token);
                string baseUrl = _config.GetValue<string>("CORs:AllowedOrigin");
                string emailVerified = _config.GetValue<string>("CORs:EmailVerified");
                if (result)
                {
                    return Redirect(baseUrl + emailVerified);
                }
                else
                {
                    return BadRequest("Token sudah expire.");
                }
            }
            catch (System.Exception)
            {
                // return StatusCode(StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _authService.LoginAsync(request);
                if (result == null)
                {
                    return NotFound("Username atau password salah.");
                }

                if (string.Equals(result.Error, "UNVERIFIED"))
                {
                    return Unauthorized("Anda belum melakukan verifikasi email, silahkan cek email.");
                }

                if (string.Equals(result.Error, "INACTIVE"))
                {
                    return Unauthorized("Email atau password salah.");
                }

                return Ok(result);
            }
            catch (Exception)
            {
                // return StatusCode(StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        [HttpPost("RequestResetPassword")]
        public async Task<IActionResult> RequestResetPassword([FromBody] ResetPasswordEmailRequest request)
        {
            bool isUserExist = await _authService.IsUserAlreadyUsed(request.Email!);
            if (!isUserExist)
            {
                return NotFound("Email belum terdaftar.");
            }
            var resetPasswordUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}/api/Auth/GetResetPasswordToken/";

            // CODE: Jika user sudah terdaftar kirimkan email yang berisi token reset password
            try
            {
                var token = await _authService.ResetPasswordRequestAsync(request);
                if (!string.IsNullOrEmpty(token) && !string.IsNullOrWhiteSpace(token))
                {
                    var resetPasswordModel = new EmailResetPasswordModel()
                    {
                        EmailAddress = request.Email,
                        ResetPasswordUrl = resetPasswordUrl + token,
                        ResetPasswordToken = token
                    };

                    var emailAddresses = new List<string>
                    {
                        request.Email!
                    };

                    var model = new EmailModel(emailAddresses, "Reset Password",
                        _emailService.GetEmailTemplate("ResetPasswordEmail", resetPasswordModel)
                    );

                    bool sended = await _emailService.SendAsync(model, new CancellationToken());

                    return Ok("Silahkan cek email anda");
                }
            }
            catch (System.Exception)
            {
                throw;
            }

            return Ok("Silahkan cek email anda.");
        }

        [HttpGet("GetResetPasswordToken/{token}")]
        public IActionResult RedirectToResetPasswordForm([FromRoute] string token)
        {
            string baseUrl = _config.GetValue<string>("CORs:AllowedOrigin");
            string resetPassword = _config.GetValue<string>("CORs:ResetPassword");
            return Redirect(baseUrl + resetPassword + token);
        }

        [HttpPost("ResetPassword/{token}")]
        public async Task<IActionResult> ResetPassword([FromRoute] string token, [FromBody] ResetPasswordRequest request)
        {
            // CODE: Update password WHERE reset_token = token
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                _ = await _authService.ResetPasswordAsync(token, request);
                return Ok("Password berhasil direset");
            }
            catch (System.Exception)
            {
                // return StatusCode(StatusCodes.Status500InternalServerError);
                throw;
            }
        }

        [HttpPost("SetInactive/{id}")]
        public async Task<IActionResult> SetInactive([FromRoute] Guid id)
        {
            try
            {
                var result = await _authService.SetInactiveUser(id, true);
                return NoContent();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost("RestoreStatus/{id}")]
        public async Task<IActionResult> SetActive([FromRoute] Guid id)
        {
            try
            {
                var result = await _authService.SetInactiveUser(id, false);
                return NoContent();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var result = await _authService.FindAllUserAsync();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }


    }
}