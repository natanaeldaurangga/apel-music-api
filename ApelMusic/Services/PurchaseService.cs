using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs;
using ApelMusic.DTOs.Courses;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class PurchaseService
    {
        private readonly InvoiceRepository _invoiceRepo;

        private readonly CourseRepository _courseRepo;

        private readonly ShoppingCartRepository _cartRepo;

        private readonly UsersCoursesRepository _userCourseRepo;

        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(InvoiceRepository invoiceRepo, CourseRepository courseRepo, ShoppingCartRepository cartRepo, UsersCoursesRepository userCourseRepo, ILogger<PurchaseService> logger)
        {
            _invoiceRepo = invoiceRepo;
            _courseRepo = courseRepo;
            _cartRepo = cartRepo;
            _userCourseRepo = userCourseRepo;
            _logger = logger;
        }

        public async Task<InvoiceDetailResponse?> DetailInvoiceAsync(int invoiceId)
        {
            // Karena cuman mau itemnya ajh, jadi ngasih request dummy
            var dummyPageQueryRequest = new PageQueryRequest();
            var wheres = new Dictionary<string, string>()
            {
                {"invoice_id", invoiceId.ToString()}
            };
            var invoices = await _invoiceRepo.InvoicesPagedAsync(dummyPageQueryRequest, wheres);
            if (invoices.Count == 0) return null;
            var invoice = invoices[0];
            var courses = await _invoiceRepo.GetInvoiceDetailAsync(invoiceId);
            return new InvoiceDetailResponse()
            {
                InvoiceId = invoice.Id,
                InvoiceNumber = invoice.InvoiceNumber,
                PurchaseDate = invoice.PurchaseDate,
                TotalPrice = invoice.TotalPrice,
                Courses = courses
            };
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

        public async Task<int> MakeDirectPurchaseAsync(Guid userId, DirectPurchaseRequest request)
        {
            var courses = await _courseRepo.FindCourseByIdAsync(request.CourseId, userId);
            if (courses.Count == 0) return 0;
            var course = courses[0];

            var userCourses = new List<UserCourses>()
            {
                new UserCourses()
                {
                    CourseId = course.Id,
                    CourseSchedule = request.CourseSchedule,
                    PurchasePrice = course.Price,
                    UserId = userId
                }
            };

            var invoice = new Invoice()
            {
                UserId = userId,
                PaymentMethodId = request.PaymentMethodId,
                PurchaseDate = DateTime.UtcNow
            };

            return await _invoiceRepo.MakeDirectPurchase(invoice, userCourses);
        }

        public async Task<PageQueryResponse<UserCourseResponse>> PurchasedCoursesPagedAsync(PageQueryRequest pageQuery, IDictionary<string, string>? fields = null)
        {
            var userCourses = await _userCourseRepo.PurchasedCoursesPagedAsync(pageQuery, fields);

            return new PageQueryResponse<UserCourseResponse>()
            {
                CurrentPage = pageQuery.CurrentPage,
                PageSize = pageQuery.PageSize,
                Items = userCourses
            };
        }

        public async Task<int> AlreadyPurchasedAsync(DirectPurchaseRequest request)
        {
            return await _userCourseRepo.AlreadyPurchasedAsync(request);
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