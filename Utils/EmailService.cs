namespace EXE202.Utils
{
    public class EmailService
    {
        public string GetEmailTemplate(string templateName)
        {
            // Lấy đường dẫn tuyệt đối đến wwwroot
            var webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");

            // Đường dẫn đến file HTML
            var filePath = Path.Combine(webRootPath, templateName);

            // Đọc nội dung file HTML
            if (File.Exists(filePath))
            {
                return File.ReadAllText(filePath);
            }

            return "";
        }

        public static void SendEmailMultiThread(string email, string subject, string body)
        {
            EmailSender sender = new EmailSender();
            sender.SendEmailAsync(email, subject, body).Wait();
        }
    }
}
