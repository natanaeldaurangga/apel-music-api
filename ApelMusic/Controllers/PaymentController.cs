using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly PaymentMethodService _paymentService;

        private readonly ILogger<PaymentController> _logger;

        public PaymentController(PaymentMethodService paymentService, ILogger<PaymentController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> InsertPaymentMethod([FromForm] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _paymentService.InsertPaymentAsync(request);
                if (result == 0) return NotFound();
                return Ok();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPut("{paymentId}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdatePaymentMethod([FromRoute] Guid paymentId, [FromForm] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _paymentService.UpdatePaymentAsync(paymentId, request);
                if (result == 0) return NotFound();
                return Ok("Data berhasildi update");
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("ForAdmin")]
        public async Task<IActionResult> FindAllPaymentMethod()
        {
            try
            {
                var result = await _paymentService.FindAllPaymentAsync();
                if (result.Count == 0) return NotFound();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("ForUser")]
        public async Task<IActionResult> FindAllPaymentMethodUser()
        {
            try
            {
                var result = await _paymentService.FindActivePaymentAsync();
                if (result.Count == 0) return NotFound();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> FindPaymentById([FromRoute] Guid id)
        {
            try
            {
                var result = await _paymentService.FindPaymentByIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPut("Inactive/{id}")]
        public async Task<IActionResult> SetInactivePayment([FromRoute] Guid id, [FromQuery] bool inactive)
        {
            try
            {
                var result = await _paymentService.SetInactivePaymentAsync(id, inactive);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}