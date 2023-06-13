using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ApelMusic.DTOs.Auth
{
    public class LoginRequest
    {
        [MaxLength(255, ErrorMessage = "Field 'email' tidak boleh lebih dari 255 karakter.")]
        [EmailAddress(ErrorMessage = "Field 'Email' tidak valid.")]
        [Required]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [StringLength(maximumLength: 255, MinimumLength = 10, ErrorMessage = "Field 'Password' tidak boleh kurang dari 10 karakter dan lebih dari 255 karakter.")]
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}