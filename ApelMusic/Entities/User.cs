using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public class User : BaseEntity
    {
        [MaxLength(255)]
        public string? FullName { get; set; }

        [MaxLength(255)]
        public string? Email { get; set; }

        [MaxLength(32)]
        public byte[]? PasswordHash { get; set; }

        [MaxLength(64)]
        public byte[]? PasswordSalt { get; set; }

        [MaxLength(255)]
        public string? RefreshToken { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? TokenCreated { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? TokenExpires { get; set; }

        public Guid? RoleId { get; set; }

        public Role? Role { get; set; }

        [MaxLength(255)]
        public string? VerificationToken { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? VerifiedAt { get; set; }

        [MaxLength(255)]
        public string? ResetPasswordToken { get; set; }

    }
}