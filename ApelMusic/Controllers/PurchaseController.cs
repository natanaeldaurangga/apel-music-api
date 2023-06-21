using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApelMusic.DTOs;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly PurchaseService _purchaseService;

        private readonly ILogger<PurchaseController> _logger;

        public PurchaseController(PurchaseService purchaseService, ILogger<PurchaseController> logger)
        {
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> MakePurchase([FromBody] CheckoutRequest request)
        {
            // Memvalidasi request
            if (!ModelState.IsValid) return BadRequest(ModelState);
            ClaimsPrincipal user = HttpContext.User;
            Guid userId = Guid.Parse(user.FindFirstValue("id"));
            try
            {
                var result = await _purchaseService.MakePurchaseAsync(userId, request);
                if (result == 0) return NotFound();
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("Invoice/User"), Authorize]
        public async Task<IActionResult> GetInvoicesUser([FromQuery] PageQueryRequest request)
        {
            // Memvalidasi request
            if (!ModelState.IsValid) return BadRequest(ModelState);
            ClaimsPrincipal user = HttpContext.User;
            Guid userId = Guid.Parse(user.FindFirstValue("id"));
            try
            {
                var wheres = new Dictionary<string, string>() { { "user_id", userId.ToString() } };
                var result = await _purchaseService.InvoicesPagedAsync(request, wheres);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("Invoice/Detail/{invoiceId}"), Authorize]
        public async Task<IActionResult> GetInvoiceDetail([FromRoute] int invoiceId)
        {
            try
            {
                var result = await _purchaseService.DetailInvoiceAsync(invoiceId);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}