using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class CourseSummaryResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string ImageName { get; set; } = "default.png";

        public CategorySummaryResponse? Category { get; set; }
    }
}