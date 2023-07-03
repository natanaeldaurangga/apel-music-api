using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Validations;

namespace ApelMusic.DTOs.Courses
{
    public class CourseScheduleRequest
    {
        [DateAfterToday]
        public DateTime Schedule { get; set; }
    }
}