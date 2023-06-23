using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class Category : BaseEntity
    {
        public string TagName { get; set; }

        public string? Name { get; set; }

        public string? Image { get; set; }

        public string? BannerImage { get; set; }

        public string? CategoryDescription { get; set; }

    }
}