using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class PaymentResponse
    {
        public Guid Id { get; set; }

        public string? Image { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime? Inactive { get; set; }
    }
}