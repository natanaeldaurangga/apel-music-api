using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using RazorEngineCore;

namespace ApelMusic.Email
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        private readonly IWebHostEnvironment _environment;

        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger, IWebHostEnvironment environment)
        {
            _settings = settings.Value;
            _logger = logger;
            _environment = environment;
        }

        public async Task<bool> SendAsync(EmailModel model, CancellationToken ct = default)
        {
            try
            {
                var mail = new MimeMessage();

                // SENDER
                #region Sender
                mail.From.Add(new MailboxAddress(_settings.FromDisplayName, model.From ?? _settings.From));
                mail.Sender = new MailboxAddress(model.FromDisplayName ?? _settings.FromDisplayName, model.From ?? _settings.From);
                #endregion

                foreach (var mailAddress in model.To)
                {
                    // Mail to
                    mail.To.Add(MailboxAddress.Parse(mailAddress));
                }

                //BCC
                if (model.Bcc != null && model.Bcc!.Count > 0)
                {
                    foreach (string mailAddress in model.Bcc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        mail.Bcc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                    }
                }

                //CC
                if (model.Cc != null && model.Cc!.Count > 0)
                {
                    foreach (string mailAddress in model.Cc.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        mail.Cc.Add(MailboxAddress.Parse(mailAddress.Trim()));
                    }
                }

                // Body
                var body = new BodyBuilder();
                mail.Subject = model.Subject;
                body.HtmlBody = model.Body;
                mail.Body = body.ToMessageBody();

                // Mail sent procces
                using var smtp = new SmtpClient();

                await smtp.ConnectAsync(_settings.Host, _settings.Port, true, ct);

                await smtp.AuthenticateAsync(_settings.Username, _settings.Password, ct);

                await smtp.SendAsync(mail, ct);

                await smtp.DisconnectAsync(true, ct);

                return true;
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        public string GetEmailTemplate<T>(string emailTemplate, T emailTemplateModel)
        {
            string mailTemplate = LoadTemplate(emailTemplate);

            IRazorEngine razorEngine = new RazorEngine();
            IRazorEngineCompiledTemplate modifiedMailTemplate = razorEngine.Compile(mailTemplate);

            return modifiedMailTemplate.Run(emailTemplateModel);
        }

        public string LoadTemplate(string emailTemplate)
        {
            string baseDir = _environment.ContentRootPath;
            string templateDir = Path.Combine(baseDir, "Files/EmailTemplates");
            string templatePath = Path.Combine(templateDir, $"{emailTemplate}.cshtml");

            using FileStream fileStream = new(templatePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader streamReader = new(fileStream, Encoding.Default);

            string mailTemplate = streamReader.ReadToEnd();
            streamReader.Close();

            return mailTemplate;
        }

    }
}