using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class Course : BaseEntity
    {
        public string Name { get; set; }

        public Guid CategoryId { get; set; }

        public Category? Category { get; set; }

        public string? Image { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public List<CourseSchedule>? CourseSchedules { get; set; } = new List<CourseSchedule>();
    }
}