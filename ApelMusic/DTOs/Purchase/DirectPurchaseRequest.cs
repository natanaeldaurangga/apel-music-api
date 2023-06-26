using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Purchase
{
    public class DirectPurchaseRequest
    {
        [Required(ErrorMessage = "PaymentMethodId is required.")]
        public Guid PaymentMethodId { get; set; }

        [Required(ErrorMessage = "Jadwal kelas wajib diisi")]
        public DateTime CourseSchedule { get; set; }

        [Required(ErrorMessage = "CourseId is required.")]
        public Guid CourseId { get; set; }
    }
}