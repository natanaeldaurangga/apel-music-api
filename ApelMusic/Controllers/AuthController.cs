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

        public AuthController(AuthService authService, EmailService emailService)
        {
            _authService = authService;
            _emailService = emailService;
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
                if (result)
                {
                    return Ok("Email berhasil diverifikasi silahkan login.");
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
                    return BadRequest("Username atau password salah.");
                }

                if (result.User!.VerifiedAt == null)
                {
                    return Unauthorized("Email anda belum diverifikasi, silahkan cek email anda.");
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
        public async Task<IActionResult> RequestChangePassword([FromBody] ResetPasswordEmailRequest request)
        {
            bool isUserExist = await _authService.IsUserAlreadyUsed(request.Email!);
            if (!isUserExist)
            {
                return BadRequest("Email belum terdaftar.");
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
                // return StatusCode(StatusCodes.Status500InternalServerError);
                throw;
            }

            return Ok("Silahkan cek email anda.");
        }


        // METHOD di bawah ini hanya percobaan
        [HttpGet("GetResetPasswordToken/{token}")]
        public async Task<IActionResult> TestGetPasswordToken([FromRoute] string token)
        {
            return Ok(token);
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
                return Ok("Password berhasil diperbaharui");
            }
            catch (System.Exception)
            {
                // return StatusCode(StatusCodes.Status500InternalServerError);
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