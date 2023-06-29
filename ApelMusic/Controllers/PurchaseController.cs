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

        [HttpPost("Direct"), Authorize]
        public async Task<IActionResult> DirectMakePurchase([FromBody] DirectPurchaseRequest request)
        {
            // Memvalidasi request
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ClaimsPrincipal user = HttpContext.User;
            Guid userId = Guid.Parse(user.FindFirstValue("id"));
            int alreadyPurchased = await _purchaseService.AlreadyPurchasedAsync(userId, request);
            if (alreadyPurchased > 0)
            {
                return Conflict("Kelas yang sama dengan jadwal yang sama sudah pernah anda beli.");
            }

            try
            {
                var result = await _purchaseService.MakeDirectPurchaseAsync(userId, request);
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

        [HttpGet("Invoice/Admin"), Authorize("ADMIN")]
        public async Task<IActionResult> GetInvoicesAdmin([FromQuery] PageQueryRequest request)
        {
            try
            {
                var result = await _purchaseService.InvoicesPagedAsync(request);
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

        [HttpGet("PurchasedCourse"), Authorize]
        public async Task<IActionResult> GetPurchasedCourse([FromQuery] PageQueryRequest request)
        {
            ClaimsPrincipal user = HttpContext.User;
            Guid userId = Guid.Parse(user.FindFirstValue("id"));
            try
            {
                var wheres = new Dictionary<string, string>() { { "user_id", userId.ToString() } };
                var result = await _purchaseService.PurchasedCoursesPagedAsync(request, wheres);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}