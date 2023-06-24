using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Admin
{
    public class UserEditRequest
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public bool Inactive { get; set; }
    }
}