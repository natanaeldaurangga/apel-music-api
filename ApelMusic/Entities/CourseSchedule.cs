using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class CourseSchedule : BaseEntity
    {
        public Guid? CourseId { get; set; }

        public DateTime CourseDate { get; set; }
    }
}