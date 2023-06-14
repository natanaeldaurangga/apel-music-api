using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Email.TemplateModel
{
    public class EmailResetPasswordModel
    {
        public string? EmailAddress { get; set; }

        public string? ResetPasswordToken { get; set; }

        public string? ResetPasswordUrl { get; set; }
    }
}