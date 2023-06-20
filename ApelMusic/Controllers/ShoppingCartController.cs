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
    public class ShoppingCartController : ControllerBase
    {
        private readonly ShoppingCartService _cartService;

        private readonly ILogger<ShoppingCartController> _logger;

        public ShoppingCartController(ShoppingCartService cartService, ILogger<ShoppingCartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> InsertCart([FromBody] CreateCartRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _cartService.InsertCartAsync(request);
                return Ok("Course berhasil dimasukkan ke dalam keranjang.");
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> FindCartByUserId([FromRoute] Guid userId)
        {
            try
            {
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