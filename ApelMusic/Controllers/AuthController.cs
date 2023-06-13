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
        public async Task<IActionResult> Register(RegistrationRequest request)
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
            var baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            try
            {
                var result = await _authService.RegisterNewUser(request);

                var emailVerification = new EmailVerificationModel()
                {
                    EmailAddress = request.Email,
                    Url = baseUrl,
                    VerificationToken = result
                };

                var emailAddresses = new List<string>
                    {
                        request.Email!
                    };

                var model = new EmailModel(emailAddresses, "Verifikasi Email",
                    _emailService.GetEmailTemplate("VerifyEmail", emailVerification)
                );

                return Ok("Silahkan cek email anda untuk melakukan verifikasi");

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