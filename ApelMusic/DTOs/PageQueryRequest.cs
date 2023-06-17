using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs
{
    public class PageQueryRequest
    {
        public int PageSize { get; set; } = 5;

        public int CurrentPage { get; set; } = 1;

        public string? Direction { get; set; } = "ASC";

        public string? SortBy { get; set; } = string.Empty;

        public string? Keyword { get; set; } = string.Empty;
    }
}