using System.Net;
using System.Net.Mail;

namespace EXE202.Utils
{
    public class EmailSender
    {
        string SenderMail = "sys.TamDaoStay@gmail.com";
        string SenderPassword = "bcsa tjvh fwdo qsxr";
        
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(SenderMail, SenderPassword)
            };
            var senderAddress = new MailAddress(SenderMail, "TamDaoStay");
            var mailMessage = new MailMessage
            {
                From = senderAddress,
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);

            return client.SendMailAsync(mailMessage);
        }
    }
}
