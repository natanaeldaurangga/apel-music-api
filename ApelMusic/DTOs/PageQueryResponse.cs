using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs
{
    public class PageQueryResponse<T>
    {
        public int TotalRowsInTable { get; set; } // TOTAL DATA

        public int CurrentPage { get; set; } // CURRENT PAGE

        public int PageSize { get; set; } // DATA PERPAGE

        public int TotalPages { get; set; } // TOTAL PAGE

        public List<T> Items { get; set; } = new();
    }
}