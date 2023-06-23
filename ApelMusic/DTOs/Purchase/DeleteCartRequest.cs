using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class DeleteCartRequest
    {
        public List<Guid> Ids { get; set; } = new();
    }
}