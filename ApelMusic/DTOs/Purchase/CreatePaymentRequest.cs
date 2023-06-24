using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Validations;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.DTOs.Purchase
{
    public class CreatePaymentRequest
    {
        [FromForm(Name = "Image")]
        [AppFileExtensions(AllowMimeTypes = new string[] { "image/png", "image/jpeg" }, ErrorMessage = "Ekstensi yang didukung hanya jpeg dan png.")]
        [FileSize(5 * 1024 * 1024, ErrorMessage = "Maksimal ukuran file adalah 5 MB.")]
        public IFormFile? Image { get; set; }

        [Required(ErrorMessage = "Nama payment wajib diisi.")]
        [MaxLength(100, ErrorMessage = "Panjang karakter pada field nama tidak boleh melebihi 100 Karakter.")]
        public string? Name { get; set; }

        public bool Inactive { get; set; }
    }
}