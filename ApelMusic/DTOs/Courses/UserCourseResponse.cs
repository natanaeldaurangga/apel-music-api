using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class UserCourseResponse
    {
        public Guid CourseId { get; set; }

        public string? CourseName { get; set; }

        public string? CourseImage { get; set; }

        public DateTime CourseSchedule { get; set; }

        public Guid CategoryId { get; set; }

        public string? Categoryname { get; set; }

        public decimal PurchasePrice { get; set; }

    }
}