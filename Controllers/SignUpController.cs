using Microsoft.AspNetCore.Mvc;
using EXE202.DAO;
using EXE202.Models;
using System.Net.Mail;
using System.Net;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Newtonsoft.Json.Linq;
using EXE202.Utils;
namespace EXE202.Controllers
{
    public class SignUpController : Controller
    {
        private readonly AccountDAO dao_user;
        public SignUpController(AccountDAO accountDAO)
        {
            dao_user = accountDAO;
        }

        public IActionResult SignUp()
        {
            return View("SignUpForm");
        }

        [HttpPost]

        public async Task<IActionResult> ValidateEmailAsync(string fname, string lname, string email, string password, string repassword, DateTime dob, string phone, string address)
        {
            int age = DateTime.Today.Year - dob.Year;
            if (dob > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            SignUp signUp = new SignUp()
            {
                firstName = fname,
                lastName = lname,
                email = email,
                password = password,
                dob = dob,
                phone = phone,
                address = address
            };
            ViewBag.signup = signUp;
            HttpContext.Session.SetObject("signup", signUp);

            if (!repassword.Equals(password))
            {
                ViewBag.error = "Nhập lại mật khẩu không chính xác";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (!Regex.IsMatch(email, "^[A-Za-z0-9+_.-]+@(.+)$"))
            {
                ViewBag.error = "Vui lòng nhập đúng định dạng email";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (Regex.IsMatch(password, ".*\\s.*"))
            {
                ViewBag.error = "Mật khẩu không được có dấu cách";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (Regex.IsMatch(password, ".*\\s{2,}.*"))
            {
                ViewBag.error = "Mật khẩu không được chứa nhiều khoảng trắng liên tiếp.";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (!Regex.IsMatch(password, "^(?=.*[A-Z])(?=.*[a-z])(?=.*\\d)(?=.*[@#$%^&+=!]).{8,}$"))
            {
                string passError = "Mật khẩu không đáp ứng được yêu cầu về độ phức tạp. Bạn đang thiếu: \n";
                if (!Regex.IsMatch(password, ".*[A-Z].*"))
                {
                    passError += "  - ít nhất một chữ cái viết hoa\n";
                }
                if (!Regex.IsMatch(password, ".*[a-z].*"))
                {
                    passError += "  - ít nhất một chữ cái thường\n";
                }
                if (!Regex.IsMatch(password, ".*\\d.*"))
                {
                    passError += "  - ít nhất một chữ số\n";
                }
                if (!Regex.IsMatch(password, ".*[@#$%^&+=!].*"))
                {
                    passError += "  - ít nhất một ký tự đặc biệt\n";
                }
                if (password.Length < 8)
                {
                    passError += "  - độ dài tối thiểu là 8 ký tự\n";
                }
                ViewBag.error = passError;
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (!Regex.IsMatch(phone, "^\\d{10,}$"))
            {
                ViewBag.error = "Số điện thoại phải có ít nhất 10 chữ số!";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (dao_user.CheckAccountExist(email) != null)
            {
                ViewBag.error = "Email đã tồn tại! Vui lòng sử dụng email khác và thử lại.";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else if (age < 18)
            {
                ViewBag.error = "Bạn chưa đủ 18 tuổi!";
                ViewBag.signup_raw = signUp;
                return View("SignUpForm");
            }
            else
            {
                int otpValue = new Random().Next(100000, 1000000);

                SendOTPMailToCustomer(lname, email, otpValue);

                HttpContext.Session.SetInt32("otp", otpValue);
                DateTime now = DateTime.Now;
                HttpContext.Session.SetString("currentDateTime", now.ToString("o"));
                return RedirectToAction("ValidateOTP");
            }
        }

        private void SendOTPMailToCustomer(string lname, string email, int otp)
        {
            string subject = "OTP ĐĂNG KÝ TÀI KHOẢN TẠI OCEANBOOKING";
            EmailService emailService = new EmailService();
            string htmlContent = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\">\r\n<head>\r\n<!--[if gte mso 9]>\r\n<xml>\r\n  <o:OfficeDocumentSettings>\r\n    <o:AllowPNG/>\r\n    <o:PixelsPerInch>96</o:PixelsPerInch>\r\n  </o:OfficeDocumentSettings>\r\n</xml>\r\n<![endif]-->\r\n  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <meta name=\"x-apple-disable-message-reformatting\">\r\n  <!--[if !mso]><!--><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><!--<![endif]-->\r\n  <title></title>\r\n  \r\n    <style type=\"text/css\">\r\n      \r\n      @media only screen and (min-width: 620px) {\r\n        .u-row {\r\n          width: 600px !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          vertical-align: top;\r\n        }\r\n\r\n        \r\n            .u-row .u-col-50 {\r\n              width: 300px !important;\r\n            }\r\n          \r\n\r\n            .u-row .u-col-100 {\r\n              width: 600px !important;\r\n            }\r\n          \r\n      }\r\n\r\n      @media only screen and (max-width: 620px) {\r\n        .u-row-container {\r\n          max-width: 100% !important;\r\n          padding-left: 0px !important;\r\n          padding-right: 0px !important;\r\n        }\r\n\r\n        .u-row {\r\n          width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          display: block !important;\r\n          width: 100% !important;\r\n          min-width: 320px !important;\r\n          max-width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col > div {\r\n          margin: 0 auto;\r\n        }\r\n\r\n\r\n        .u-row .u-col img {\r\n          max-width: 100% !important;\r\n        }\r\n\r\n}\r\n    \r\nbody {\r\n  margin: 0;\r\n  padding: 0;\r\n}\r\n\r\ntable,\r\ntr,\r\ntd {\r\n  vertical-align: top;\r\n  border-collapse: collapse;\r\n}\r\n\r\np {\r\n  margin: 0;\r\n}\r\n\r\n.ie-container table,\r\n.mso-container table {\r\n  table-layout: fixed;\r\n}\r\n\r\n* {\r\n  line-height: inherit;\r\n}\r\n\r\na[x-apple-data-detectors='true'] {\r\n  color: inherit !important;\r\n  text-decoration: none !important;\r\n}\r\n\r\n\r\n\r\ntable, td { color: #000000; } #u_body a { color: #0000ee; text-decoration: underline; } @media (max-width: 480px) { #u_content_text_7 .v-text-align { text-align: center !important; } #u_content_text_8 .v-text-align { text-align: center !important; } }\r\n    </style>\r\n  \r\n  \r\n\r\n<!--[if !mso]><!--><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Cabin:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><!--<![endif]-->\r\n\r\n</head>\r\n\r\n<body class=\"clean-body u_body\" style=\"margin: 0;padding: 0;-webkit-text-size-adjust: 100%;background-color: #ecf0f1;color: #000000\">\r\n  <!--[if IE]><div class=\"ie-container\"><![endif]-->\r\n  <!--[if mso]><div class=\"mso-container\"><![endif]-->\r\n  <table id=\"u_body\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;min-width: 320px;Margin: 0 auto;background-color: #ecf0f1;width:100%\" cellpadding=\"0\" cellspacing=\"0\">\r\n  <tbody>\r\n  <tr style=\"vertical-align: top\">\r\n    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n    <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td align=\"center\" style=\"background-color: #ecf0f1;\"><![endif]-->\r\n    \r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:6px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n    <tbody>\r\n      <tr style=\"vertical-align: top\">\r\n        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n          <span>&#160;</span>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:20px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n  <tr>\r\n    <td class=\"v-text-align\" style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\">\r\n      \r\n      <img align=\"center\" border=\"0\" src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451038/OCBK/MAIL_ASSETS/xfasw3y2sbkptt1okql6.png\" alt=\"Image\" title=\"Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 72%;max-width: 417.6px;\" width=\"417.6\"/>\r\n      \r\n    </td>\r\n  </tr>\r\n</table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:0px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 140%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"line-height: 140%;\"><strong>Kính gửi Quý khách {{CUSTOMER_NAME}},</strong></p>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<p style=\"line-height: 140%;\">Cảm ơn Quý khách đã đăng ký tài khoản tại <strong>OceanBooking</strong>.</p>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<p style=\"line-height: 140%;\">Để hoàn tất quá trình đăng ký và xác thực tài khoản của Quý khách, vui lòng sử dụng mã OTP (One-Time Password) dưới đây:</p>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<h3><strong>Mã OTP của Quý khách là: {{OTP}}</strong></h3>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<p style=\"line-height: 140%;\">Lưu ý rằng Quý khách không được chia sẽ mã OTP này cho bất kỳ ai.</p>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<p style=\"line-height: 140%;\">Nếu Quý khách không yêu cầu mã OTP hoặc có bất kỳ thắc mắc nào, xin vui lòng liên hệ với chúng tôi qua địa chỉ email <strong>sys.oceanbooking@gmail.com</strong>, để đảm bảo an toàn cho tài khoản của Quý khách.</p>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<p style=\"line-height: 140%;\">Một lần nữa, cảm ơn Quý khách đã tin tưởng và lựa chọn <strong>OceanBooking</strong>.</p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #2d529d;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #2d529d;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table id=\"u_content_text_7\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:23px 60px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 160%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-family: 'Open Sans', sans-serif; font-size: 14px; line-height: 22.4px;\"><strong><span style=\"line-height: 22.4px; color: #ffffff; font-size: 14px;\">Trân trọng,</span></strong></span></p>\r\n<p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-size: 14px; line-height: 22.4px; font-family: 'Open Sans', sans-serif; color: #ffffff;\">OceanBooking</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:25px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n        <div align=\"center\">\r\n          <div style=\"display: table; max-width:89px;\">\r\n          <!--[if (mso)|(IE)]><table width=\"89\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"border-collapse:collapse;\" align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse; mso-table-lspace: 0pt;mso-table-rspace: 0pt; width:89px;\"><tr><![endif]-->\r\n          \r\n            \r\n            <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 13px;\" valign=\"top\"><![endif]-->\r\n              <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 13px\">\r\n                <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n                  <a href=\"https://www.facebook.com/profile.php?id=61565969259252\" title=\"Facebook\" target=\"_blank\">\r\n                    <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/uf5ctobb6rvkyqfyzymn.png\" alt=\"Facebook\" title=\"Facebook\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n                  </a>\r\n                </td></tr>\r\n              </tbody></table>\r\n              <!--[if (mso)|(IE)]></td><![endif]-->\r\n              \r\n              <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 0px;\" valign=\"top\"><![endif]-->\r\n              <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 0px\">\r\n                <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n                  <a href=\"mailto:sys.oceanbooking@gmail.com\" title=\"Email\" target=\"_blank\">\r\n                    <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/vwxmktzvydbnwggpcu5g.png\" alt=\"Email\" title=\"Email\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n                  </a>\r\n                </td></tr>\r\n              </tbody></table>\r\n              <!--[if (mso)|(IE)]></td><![endif]-->\r\n            \r\n            \r\n            <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n          </div>\r\n        </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n<table id=\"u_content_text_8\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:5px 10px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; color: #4fadc9; line-height: 140%; text-align: center; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: center;\"><span style=\"font-size: 16px; line-height: 22.4px; color: #ffffff; font-family: Cabin, sans-serif;\">www.oceanbooking.online</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n    <!--[if (mso)|(IE)]></td></tr></table><![endif]-->\r\n    </td>\r\n  </tr>\r\n  </tbody>\r\n  </table>\r\n  <!--[if mso]></div><![endif]-->\r\n  <!--[if IE]></div><![endif]-->\r\n</body>\r\n\r\n</html>\r\n";
            if (htmlContent != "")
            {
                htmlContent = htmlContent.Replace("{{CUSTOMER_NAME}}", lname);
                htmlContent = htmlContent.Replace("{{OTP}}", otp.ToString());

                Thread t = new Thread(() => EmailService.SendEmailMultiThread(email, subject, htmlContent));
                t.Start();
            }
        }

        public IActionResult ValidateOTP()
        {
            return View("EnterOTP");
        }

        [HttpPost]
        public IActionResult ValidateOTP(string digit1, string digit2, string digit3, string digit4, string digit5, string digit6)
        {
            string userResponse = HttpContext.Request.Form["g-recaptcha-response"];
            int otp = Int32.Parse($"{digit1}{digit2}{digit3}{digit4}{digit5}{digit6}");
            int? storedOtp = HttpContext.Session.GetInt32("otp");
            string dateTimeString = HttpContext.Session.GetString("currentDateTime");
            DateTime storedDateTime = DateTime.Parse(dateTimeString);
            DateTime now = DateTime.Now;
            TimeSpan timeDifference = now - storedDateTime;

            try
            {
                if (timeDifference.TotalSeconds >= 300)
                {
                    ViewBag.message = "OTP của bạn đã bị hủy! Vui lòng thử lại.";
                    HttpContext.Session.Remove("otp");
                    HttpContext.Session.Remove("currentDateTime");
                    return View("EnterOTP");
                }
                else if (otp == storedOtp)
                {
                    HttpContext.Session.Remove("otp");
                    HttpContext.Session.Remove("currentDateTime");
                    var signUp = HttpContext.Session.GetObject<SignUp>("signup");
                    dao_user.SignUp(signUp.firstName, signUp.lastName, signUp.email, signUp.password, signUp.address, signUp.phone, signUp.dob);
                    ViewBag.message = "Đã đăng ký xong. Đang chuyển hướng đến trang Đăng nhập trong vài giây nữa....";
                    HttpContext.Session.Remove("signup");
                    return View("EnterOTP");
                }
                else
                {
                    ViewBag.message = "Sai OTP";
                    return View("EnterOTP");
                }
            }
            catch (Exception)
            {
                return View("EnterOTP");
            }
        }
    }
}


