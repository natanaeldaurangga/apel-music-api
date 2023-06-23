using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class InvoiceDetailResponse
    {
        public int InvoiceId { get; set; }

        public string? InvoiceNumber { get; set; }

        public DateTime PurchaseDate { get; set; }

        public decimal TotalPrice { get; set; }

        public List<DetailInvoiceResponse> Courses { get; set; } = new();
    }
}