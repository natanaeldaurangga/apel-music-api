using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class CreateCourseRequest
    {
        public string? Name { get; set; }

        public Guid? CategoryId { get; set; }

        public IFormFile? Image { get; set; }

        public string? Description { get; set; }
    }
}