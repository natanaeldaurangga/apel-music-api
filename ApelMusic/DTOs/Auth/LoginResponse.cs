using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApelMusic.DTOs.Auth
{
    public class LoginResponse
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? JwtToken { get; set; }

        [JsonIgnore]
        public RefreshTokenResponse? RefreshToken { get; set; }
    }
}