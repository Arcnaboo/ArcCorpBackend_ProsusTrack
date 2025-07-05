using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ArcCorpBackend.Services
{
    public class EmailService
    {
        private const string FromEmail = "fintagsoft@gmail.com";
        private const string FromName = "ArcCorp";
        private const string FromPassword = "Arc2020.!";

        public static void SendEmail(string toEmail, string subject, string body, bool isHtml)
        {
            var fromAddress = new MailAddress(FromEmail, FromName);
            var toAddress = new MailAddress(toEmail);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, FromPassword)
            };

            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            })
            {
                smtp.Send(message);
            }
        }

        public async static Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml)
        {
            var fromAddress = new MailAddress(FromEmail, FromName);
            var toAddress = new MailAddress(toEmail);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, FromPassword)
            };

            var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            await smtp.SendMailAsync(message);

            message.Dispose();
            smtp.Dispose();
        }
    }
}
