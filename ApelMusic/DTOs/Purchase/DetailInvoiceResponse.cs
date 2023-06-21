using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class DetailInvoiceResponse
    {
        public Guid CourseId { get; set; }

        public string CourseName { get; set; } = string.Empty;

        public DateTime? CourseSchedule { get; set; }

        public Guid CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public decimal PurchasePrice { get; set; }
    }
}