using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Auth
{
    public class RefreshTokenResponse
    {
        public string? Token { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? Expires { get; set; }

        public static implicit operator string(RefreshTokenResponse v)
        {
            throw new NotImplementedException();
        }
    }
}