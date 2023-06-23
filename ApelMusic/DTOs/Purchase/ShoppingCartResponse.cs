using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.DTOs.Courses;

namespace ApelMusic.DTOs.Purchase
{
    public class ShoppingCartResponse
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public Guid CourseId { get; set; }

        public DateTime CourseSchedule { get; set; }

        public CourseSummaryResponse Course { get; set; } = new();
    }
}