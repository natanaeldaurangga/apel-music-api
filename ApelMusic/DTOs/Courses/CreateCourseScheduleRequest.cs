using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Validations;

namespace ApelMusic.DTOs.Courses
{
    public class CreateCourseScheduleRequest
    {
        [Required(ErrorMessage = "Field course wajib diisi.")]
        public Guid? CourseId { get; set; }

        [Required(ErrorMessage = "Field jadwal course wajib diisi.")]
        [DateAfterToday(ErrorMessage = "Jadwal schedule paling cepat adalah setelah hari ini.")]
        public DateTime? CourseDate { get; set; }
    }
}