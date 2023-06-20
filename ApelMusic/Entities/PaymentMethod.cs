using System.Net.Mime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class PaymentMethod : BaseEntity
    {
        public string? Image { get; set; }

        public string Name { get; set; } = string.Empty;

    }
}