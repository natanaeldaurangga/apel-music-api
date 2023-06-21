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

        private readonly CourseRepository _courseRepo;

        private readonly ILogger<ShoppingCartRepository> _logger;

        public ShoppingCartService(ShoppingCartRepository cartRepo, ILogger<ShoppingCartRepository> logger, CourseRepository courseRepo)
        {
            _cartRepo = cartRepo;
            _courseRepo = courseRepo;
            _logger = logger;
        }

        public async Task<int> InsertCartAsync(Guid userId, CreateCartRequest request)
        {
            var cart = new ShoppingCart()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CourseId = request.CourseId,
                CourseSchedule = request.CourseSchedule
            };

            return await _cartRepo.InsertCartAsync(cart);
        }

        public async Task<List<ShoppingCart>> FindCartByUserIdAsync(Guid userId)
        {
            var results = await _cartRepo.FindCartByUserIdAsync(userId);
            results.ForEach(async (shoppingCart) =>
            {
                var course = await _courseRepo.FindCourseById(shoppingCart.CourseId);
                if (course.Count == 0) shoppingCart.Course = null;
                else shoppingCart.Course = course[0];
            });

            return results;
        }

        public async Task<List<ShoppingCart>> FindCartByIdAsync(List<Guid> ids)
        {
            return await _cartRepo.FindCartByIdsAsync(ids);
        }

        public async Task<int> DeleteCartByIdAsync(List<Guid> cartIds)
        {
            _ = await _cartRepo.DeleteCartAsync(cartIds);
            return 1;
        }
    }
}