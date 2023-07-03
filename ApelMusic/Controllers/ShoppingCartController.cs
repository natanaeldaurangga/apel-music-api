using System.Net;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ApelMusic.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShoppingCartController : ControllerBase
    {
        private readonly ShoppingCartService _cartService;

        private readonly PurchaseService _purchaseService;

        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ShoppingCartService cartService, PurchaseService purchaseService, ILogger<ShoppingCartController> logger)
        {
            _cartService = cartService;
            _purchaseService = purchaseService;
            _logger = logger;
        }

        [HttpPost, Authorize, Authorize("USER")]
        public async Task<IActionResult> InsertCart([FromBody] CreateCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ClaimsPrincipal user = HttpContext.User;
            Guid userId = Guid.Parse(user.FindFirstValue("id"));

            int checkInCart = await _cartService.CheckAlreadyInCart(userId, request);
            if (checkInCart > 0)
            {
                return Conflict("Produk yang sama dengan schedule yang sama sudah ada pada cart");
            }

            int isScheduleConflict = await _cartService.IsScheduleConflictAsync(userId, request.CourseSchedule);
            if (isScheduleConflict > 0)
            {
                return Conflict("Kelas dengan jadwal yang sama sudah ada pada keranjang.");
            }

            var dummyDirectPurchase = new DirectPurchaseRequest()
            {
                CourseSchedule = request.CourseSchedule,
                CourseId = request.CourseId
            };

            int checkInClass = await _purchaseService.AlreadyPurchasedAsync(userId, dummyDirectPurchase);
            if (checkInClass > 0)
            {
                return Conflict("Produk yang sama dengan schedule yang sama sudah pernah anda beli");
            }

            try
            {
                var result = await _cartService.InsertCartAsync(userId, request);
                return Ok("Course berhasil dimasukkan ke dalam keranjang.");
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost("DeleteCart"), Authorize, Authorize]
        public async Task<IActionResult> DeleteCart([FromBody] DeleteCartRequest request)
        {
            try
            {
                var result = await _cartService.DeleteCartByIdsAsync(request.Ids);
                return Ok("Item berhasil dihapus");
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public async Task<IActionResult> FindCartByUserId()
        {
            try
            {
                ClaimsPrincipal user = HttpContext.User;
                Guid userId = Guid.Parse(user.FindFirstValue("id"));
                var result = await _cartService.FindCartByUserIdAsync(userId);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("Count"), Authorize]
        public async Task<IActionResult> CountItems()
        {
            try
            {
                ClaimsPrincipal user = HttpContext.User;
                Guid userId = Guid.Parse(user.FindFirstValue("id"));
                var result = await _cartService.CountItemInCartAsync(userId);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpPost("FindByIds"), Authorize]
        public async Task<IActionResult> FindCartById([FromBody] List<Guid> ids)
        {
            try
            {
                var result = await _cartService.FindCartByIdAsync(ids);
                return Ok(result);
            }
            catch (System.Exception)
            {
                throw;
            }
        }

    }
}