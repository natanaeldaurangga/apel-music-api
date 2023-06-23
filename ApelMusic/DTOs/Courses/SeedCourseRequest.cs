using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class SeedCourseRequest
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public Guid CategoryId { get; set; }

        public string? Image { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public List<DateTime> Schedules { get; set; } = new List<DateTime>();

    }
}