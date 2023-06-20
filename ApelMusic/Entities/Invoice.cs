using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class Invoice
    {
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime PurchaseDate { get; set; }

        public Guid PaymentMethodId { get; set; }

        public PaymentMethod? PaymentMethod { get; set; }

        public decimal TotalPrice { get; set; }

        public List<UserCourses> UserCourses { get; set; } = new List<UserCourses>();

    }
}