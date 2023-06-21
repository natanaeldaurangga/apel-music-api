using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Entities;

namespace ApelMusic.DTOs.Purchase
{
    public class CheckoutRequest
    {

        public Guid PaymentMethodId { get; set; }

        public DateTime PurchaseDate { get; set; }

        public List<Guid> ShoppingCartIds { get; set; } = new();
    }
}