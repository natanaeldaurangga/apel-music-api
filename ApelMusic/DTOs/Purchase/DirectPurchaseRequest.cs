using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class DirectPurchaseRequest
    {
        public Guid PaymentMethodId { get; set; }

        public DateTime CourseSchedule { get; set; }

        public Guid CourseId { get; set; }
    }
}