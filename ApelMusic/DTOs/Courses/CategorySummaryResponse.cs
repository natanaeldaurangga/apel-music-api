using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class CategorySummaryResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}