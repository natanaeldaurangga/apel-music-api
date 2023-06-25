using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ApelMusic.Validations;
using Microsoft.AspNetCore.Mvc;

namespace ApelMusic.DTOs.Courses
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Field tag name wajib diisi.")]
        public string? TagName { get; set; }

        [Required(ErrorMessage = "Field name wajib diisi.")]
        public string? Name { get; set; }

        [FromForm(Name = "Image")]
        [AppFileExtensions(AllowMimeTypes = new string[] { "image/png", "image/jpeg" }, ErrorMessage = "Ekstensi yang didukung hanya jpeg dan png.")]
        [FileSize(1 * 1024 * 1024, ErrorMessage = "Maksimal ukuran file adalah 1 MB.")]
        public IFormFile? Image { get; set; }

        [FromForm(Name = "BannerImage")]
        [AppFileExtensions(AllowMimeTypes = new string[] { "image/png", "image/jpeg" }, ErrorMessage = "Ekstensi yang didukung hanya jpeg dan png.")]
        [FileSize(5 * 1024 * 1024, ErrorMessage = "Maksimal ukuran file adalah 5 MB.")]
        public IFormFile? BannerImage { get; set; }

        public string? CategoryDescription { get; set; }
    }
}