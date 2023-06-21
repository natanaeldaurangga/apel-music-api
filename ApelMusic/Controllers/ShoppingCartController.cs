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

        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ShoppingCartService cartService, ILogger<ShoppingCartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpPost, Authorize]
        public async Task<IActionResult> InsertCart([FromBody] CreateCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                ClaimsPrincipal user = HttpContext.User;
                Guid userId = Guid.Parse(user.FindFirstValue("id"));
                var result = await _cartService.InsertCartAsync(userId, request);
                return Ok("Course berhasil dimasukkan ke dalam keranjang.");
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

        [HttpPost("FindByIds")]
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