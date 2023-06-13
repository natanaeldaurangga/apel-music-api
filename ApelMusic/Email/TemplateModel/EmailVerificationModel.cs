using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Email.TemplateModel
{
    public class EmailVerificationModel
    {
        public string? EmailAddress { get; set; }

        public string? VerificationToken { get; set; }

        public string? Url { get; set; }
    }
}