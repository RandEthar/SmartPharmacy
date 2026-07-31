using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    using System.Net;
    using System.Net.Mail;

    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var host = _config["EmailSettings:Host"];
            var port = int.Parse(_config["EmailSettings:Port"]!);
            var username = _config["EmailSettings:Username"];
            var password = _config["EmailSettings:Password"];
            var from = _config["EmailSettings:From"];

            var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(username, password)
            };

            var mailMessage = new MailMessage(
                from!,
                email,
                subject,
                message
            );

            mailMessage.IsBodyHtml = true;

            return client.SendMailAsync(mailMessage);
        }
    }
}
