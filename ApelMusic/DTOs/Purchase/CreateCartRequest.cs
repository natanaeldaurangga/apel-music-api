using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class CreateCartRequest
    {
        [Required]
        public Guid CourseId { get; set; }

        [Required]
        public DateTime CourseSchedule { get; set; }
    }
}