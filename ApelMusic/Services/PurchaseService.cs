using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class PurchaseService
    {
        private readonly InvoiceRepository _invoiceRepo;

        private readonly CourseRepository _courseRepo;

        private readonly ShoppingCartRepository _cartRepo;

        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(InvoiceRepository invoiceRepo, CourseRepository courseRepo, ShoppingCartRepository cartRepo, ILogger<PurchaseService> logger)
        {
            _invoiceRepo = invoiceRepo;
            _courseRepo = courseRepo;
            _cartRepo = cartRepo;
            _logger = logger;
        }

        public async Task<List<DetailInvoiceResponse>> DetailInvoiceAsync(int invoiceId)
        {
            return await _invoiceRepo.GetInvoiceDetailAsync(invoiceId);
        }

        public async Task<PageQueryResponse<InvoiceResponse>> InvoicesPagedAsync(PageQueryRequest request, IDictionary<string, string>? fields = null)
        {
            var invoices = await _invoiceRepo.InvoicesPagedAsync(request, fields);

            return new PageQueryResponse<InvoiceResponse>()
            {
                CurrentPage = request.CurrentPage,
                PageSize = request.PageSize,
                Items = invoices
            };
        }

        public async Task<int> MakePurchaseAsync(Guid userId, CheckoutRequest request)
        {
            // TODO: tambahkan user id pada where clause di cart repo, supaya makin secure
            // Get semua data dari keranjang berdasarkan ids yang ada di request
            var carts = await _cartRepo.FindCartByIdsAsync(request.ShoppingCartIds);

            if (carts.Count == 0) return 0;

            // Memfilter hanya berdasarkan userId, jaga-jaga
            carts = carts.Where(cart => cart.UserId == userId).ToList();

            // Mengambil semau course id dari keranjang yang sudah difilter tadi
            var courseIds = carts.ConvertAll(cart => cart.CourseId);

            // Mengambil semua course yang akan ditotalkan harganya
            var courses = await _courseRepo.FindCourseByIdInAsync(courseIds);

            List<UserCourses> userCourses = carts.ConvertAll(cart =>
            {
                var course = courses.First(c => c.Id == cart.CourseId);
                return new UserCourses()
                {
                    UserId = userId,
                    CourseId = course.Id,
                    PurchasePrice = course.Price,
                    CourseSchedule = cart.CourseSchedule
                };
            });

            var invoice = new Invoice()
            {
                UserId = userId,
                PaymentMethodId = request.PaymentMethodId,
                PurchaseDate = request.PurchaseDate
            };

            return await _invoiceRepo.MakePurchaseAsync(invoice, userCourses, carts);
        }
    }
}