using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs
{
    public class PageQueryResponse<T>
    {
        public int TotalCount { get; set; } // TOTAL DATA

        public int PageNumber { get; set; } // CURRENT PAGE

        public int PageSize { get; set; } // DATA PERPAGE

        public int TotalPage { get; set; } // TOTAL PAGE

        public List<T> Items { get; set; } = new();
    }
}