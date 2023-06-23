using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Admin
{
    public class UserDataResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime? Inactive { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

    }
}