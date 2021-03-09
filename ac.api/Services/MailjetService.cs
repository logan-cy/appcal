using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using ac.api.Interfaces;
using ac.api.Models.DTO;

namespace ac.api.Services
{
    public class MailjetService : IEmailService
    {
        public async Task SendAsync(string emailTo, string body, string subject, EmailOptionsDTO options)
        {
            var message = new MailMessage(options.SenderEmail, emailTo)
            {
                Body = body,
                IsBodyHtml = true,
                Subject = subject
            };

            using (var client = new SmtpClient(options.Host, options.Port))
            {
                client.Credentials = new NetworkCredential(options.ApiKey, options.ApiKeySecret);

                await client.SendMailAsync(message);
            };
        }
    }
}