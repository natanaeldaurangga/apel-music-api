using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class UserCourses
    {
        public Guid UserId { get; set; }

        public Guid CourseId { get; set; }

        public int InvoiceId { get; set; }

        public Invoice? Invoice { get; set; }

        public DateTime CourseSchedule { get; set; }

        public decimal PurchasePrice { get; set; }
    }
}