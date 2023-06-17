using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Auth
{
    public class UserSummaryResponse
    {
        public Guid? Id { get; set; }

        public string? FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public DateTime? VerifiedAt { get; set; }

        public DateTime? Inactive { get; set; }
    }
}