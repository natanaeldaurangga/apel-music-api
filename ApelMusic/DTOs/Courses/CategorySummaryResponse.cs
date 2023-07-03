using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class CategorySummaryResponse
    {
        public Guid Id { get; set; }

        public string? TagName { get; set; }

        public string? Image { get; set; }
    }
}