using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class ShoppingCartService
    {
        private readonly ShoppingCartRepository _cartRepo;

        private readonly ILogger<ShoppingCartRepository> _logger;

        public ShoppingCartService(ShoppingCartRepository cartRepo, ILogger<ShoppingCartRepository> logger)
        {
            _cartRepo = cartRepo;
            _logger = logger;
        }

        public async Task<int> InsertCartAsync(CreateCartRequest request)
        {
            var cart = new ShoppingCart()
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                CourseId = request.CourseId,
                CourseSchedule = request.CourseSchedule
            };

            return await _cartRepo.InsertCartAsync(cart);
        }

        public async Task<List<ShoppingCart>> FindCartByUserIdAsync(Guid userId)
        {
            return await _cartRepo.FindCartByUserIdAsync(userId);
        }

        public async Task<List<ShoppingCart>> FindCartByIdAsync(List<Guid> ids)
        {
            return await _cartRepo.FindCartByIdsAsync(ids);
        }

        public async Task<int> DeleteCartByIdAsync(List<Guid> cartIds)
        {
            cartIds.ForEach(async id => await _cartRepo.DeleteCartAsync(id));
            return 1;
        }
    }
}