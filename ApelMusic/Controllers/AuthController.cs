using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Auth;
using ApelMusic.Email;
using ApelMusic.Email.TemplateModel;
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
                    // TODO: Lanjut bikin login
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
            return Ok(request);
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