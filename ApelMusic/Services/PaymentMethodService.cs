using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Database.Repositories;
using ApelMusic.DTOs.Purchase;
using ApelMusic.Entities;

namespace ApelMusic.Services
{
    public class PaymentMethodService
    {
        private readonly PaymentMethodRepository _paymentRepo;

        private readonly ImageServices _imageServices;

        private readonly ILogger<PaymentMethodService> _logger;

        public PaymentMethodService(PaymentMethodRepository paymentRepo, ImageServices imageServices, ILogger<PaymentMethodService> logger)
        {
            _paymentRepo = paymentRepo;
            _imageServices = imageServices;
            _logger = logger;
        }

        public async Task<int> UpdatePaymentAsync(Guid paymentId, CreatePaymentRequest request)
        {
            var payments = await _paymentRepo.FindPaymentByIdAsync(paymentId);
            // _logger.LogInformation("payment diambil");
            if (payments.Count == 0) return 0;
            // _logger.LogInformation("payment ketemu");

            var payment = payments[0];
            if (request.Image != null)
            {
                var oldImageName = payment.Image!.Replace("%5C", "\\"); // Mengganti separator untuk url dengan \\
                _imageServices.DeleteImage(oldImageName);
                payment.Image = await _imageServices.UploadImageAsync(request.Image!, folder: "Upload");
            }
            payment.Name = request.Name ?? payment.Name;
            payment.UpdatedAt = DateTime.UtcNow;

            return await _paymentRepo.UpdatePaymentAsync(payment);
        }

        public async Task<int> SetInactivePaymentAsync(Guid paymentId, bool inactive = false)
        {
            var payments = await _paymentRepo.FindPaymentByIdAsync(paymentId);
            if (payments.Count == 0) return 0;

            var payment = payments[0];
            if (inactive) payment.Inactive = DateTime.UtcNow;
            else payment.Inactive = null;

            return await _paymentRepo.UpdatePaymentAsync(payment);
        }

        public async Task<int> InsertPaymentAsync(CreatePaymentRequest request)
        {
            var imageName = await _imageServices.UploadImageAsync(request.Image!, folder: "Upload");

            var paymentMethod = new PaymentMethod()
            {
                Id = Guid.NewGuid(),
                Name = request.Name!,
                Image = imageName,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            return await _paymentRepo.InsertPaymentAsync(paymentMethod);
        }

        public async Task<List<PaymentResponse>> FindAllPaymentAsync()
        {
            var result = await _paymentRepo.FindAllPaymentAsync();

            return result.ConvertAll(pmt =>
            {
                return new PaymentResponse()
                {
                    Id = pmt.Id,
                    Image = pmt.Image,
                    Name = pmt.Name
                };
            });
        }

        public async Task<PaymentResponse?> FindPaymentByIdAsync(Guid paymentId)
        {
            var results = await _paymentRepo.FindPaymentByIdAsync(paymentId);

            if (results.Count == 0) return null;
            var result = results[0];
            return new PaymentResponse()
            {
                Id = result.Id,
                Image = result.Image,
                Name = result.Name
            };
        }

    }
}