using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Courses
{
    public class SeedCategoryRequest
    {
        public Guid Id { get; set; }

        public string? TagName { get; set; }

        public string? Name { get; set; }

        public string? Image { get; set; }

        public string? BannerImage { get; set; }

        public string? CategoryDescription { get; set; }
    }
}