using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class Invoice
    {
        public int Id { get; set; }

        public Guid UserId { get; set; }

        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public Guid PaymentMethodId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public decimal TotalPrice { get; set; }
    }
}