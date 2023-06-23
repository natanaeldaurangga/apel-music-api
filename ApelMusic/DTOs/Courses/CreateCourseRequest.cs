using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Validations;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.DTOs.Courses
{
    public class CreateCourseRequest
    {
        [MaxLength(255, ErrorMessage = "Maksimal jumlah karakter untuk field name adalah 255 karakter.")]
        public string? Name { get; set; }

        public Guid CategoryId { get; set; }

        [FromForm(Name = "Image")]
        [AppFileExtensions(AllowMimeTypes = new string[] { "image/png", "image/jpeg" }, ErrorMessage = "Ekstensi yang didukung hanya jpeg dan png.")]
        [FileSize(5 * 1024 * 1024, ErrorMessage = "Maksimal ukuran file adalah 5 MB.")]
        public IFormFile? Image { get; set; }

        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Harga tidak boleh di bawah 0.")]
        public decimal Price { get; set; }

        public List<DateTime> Schedules { get; set; } = new List<DateTime>();
    }
}