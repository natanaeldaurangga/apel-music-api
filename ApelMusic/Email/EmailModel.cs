using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Email
{
    public class EmailModel
    {
        // Receiver
        public List<string> To { get; }
        public List<string> Bcc { get; }
        public List<string> Cc { get; }

        // Sender
        public string? From { get; }
        public string? FromDisplayName { get; }

        // Content
        public string Subject { get; }
        public string? Body { get; }

        public EmailModel(List<string> to, string subject, string? body = null, string? from = null, string? displayName = null, List<string>? bcc = null, List<string>? cc = null)
        {
            // Receiver
            To = to;
            Bcc = bcc ?? new List<string>();
            Cc = cc ?? new List<string>();

            // Sender
            From = from;
            FromDisplayName = displayName;

            // Content
            Subject = subject;
            Body = body;
        }
    }
}