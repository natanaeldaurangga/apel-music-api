using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Admin
{
    public class UserEditRequest
    {
        public string FullName { get; set; } = string.Empty;

        public DateTime? Inactive { get; set; }
    }
}