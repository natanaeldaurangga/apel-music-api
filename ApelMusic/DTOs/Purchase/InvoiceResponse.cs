using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class InvoiceResponse
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public int Quantity { get; set; }

        public decimal TotalPrice { get; set; }

        public Guid PaymentId { get; set; }

        public PaymentResponse? Payment { get; set; }
    }
}