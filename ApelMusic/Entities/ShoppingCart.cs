using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class ShoppingCart
    {
        public Guid? Id { get; set; }

        public Guid UserId { get; set; }

        public Guid CourseId { get; set; }

        public Course? Course { get; set; }

        public DateTime CourseSchedule { get; set; }
    }
}