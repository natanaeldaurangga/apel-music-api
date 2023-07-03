using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs.Courses;
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

        public async Task<List<ShoppingCartResponse>> FindCartByUserIdAsync(Guid userId)
        {
            var results = await _cartRepo.FindCartByUserIdAsync(userId);

            var cartsResponseAsync = results.ConvertAll(async (cart) =>
            {
                var courses = await _courseRepo.FindCourseByIdAsync(cart.CourseId, userId);
                if (courses.Count == 0)
                {
                    cart.Course = null;
                };
                var course = courses[0];
                var courseSum = new CourseSummaryResponse()
                {
                    Id = course.Id,
                    Name = course.Name,
                    Category = new CategorySummaryResponse()
                    {
                        Id = course.Id,
                        TagName = course.Category!.TagName
                    },
                    Price = course.Price,
                    ImageName = course.Image!
                };
                return new ShoppingCartResponse()
                {
                    Id = cart.Id,
                    CourseId = cart.CourseId,
                    CourseSchedule = cart.CourseSchedule,
                    UserId = userId,
                    Course = courseSum
                };
            });
            // Karena semua item di cartsResponse adalah Promise maka harus di wait dulu
            var cartsResponse = (await Task.WhenAll(cartsResponseAsync)).ToList();

            return cartsResponse;
        }

        public async Task<List<ShoppingCart>> FindCartByIdAsync(List<Guid> ids)
        {
            return await _cartRepo.FindCartByIdsAsync(ids);
        }

        public async Task<int> DeleteCartByIdsAsync(List<Guid> cartIds)
        {
            _ = await _cartRepo.DeleteCartAsync(cartIds);
            return 1;
        }

        public async Task<int> CountItemInCartAsync(Guid userId)
        {
            return await _cartRepo.CountItemInCartAsync(userId);
        }

        public async Task<int> IsScheduleConflictAsync(Guid userId, DateTime courseSchedule)
        {
            return await _cartRepo.IsScheduleConflictAsync(userId, courseSchedule);
        }

        public async Task<int> CheckAlreadyInCart(Guid userId, CreateCartRequest request)
        {
            return await _cartRepo.CheckAlreadyInCartAsync(userId, request);
        }
    }
}