using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Admin
{
    public class UserEditPasswordRequest
    {
        [StringLength(maximumLength: 255, MinimumLength = 10, ErrorMessage = "Field 'Password' tidak boleh kurang dari 10 karakter dan lebih dari 255 karakter.")]
        [Required]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare(nameof(UserEditPasswordRequest.Password))]
        [Required]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}