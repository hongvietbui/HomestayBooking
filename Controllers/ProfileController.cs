using Microsoft.AspNetCore.Mvc;
using EXE202.DAO;
using EXE202.Models;
using System.Text.RegularExpressions;
using EXE202.Services.Impl;
using EXE202.Utils;

namespace EXE202.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AccountDAO dao_user;
        private readonly FinanceDAO finance_dao;
        private readonly HomestayDAO homestay_dao;

        private readonly IQRService _qrService;
        //private readonly CarDAO car_dao;
        public ProfileController(AccountDAO accountDAO, FinanceDAO financeDAO, HomestayDAO homestayDAO, IQRService qrService)
        {
            dao_user = accountDAO;
            finance_dao = financeDAO;
            homestay_dao = homestayDAO;
            _qrService = qrService;
        }
        public IActionResult Profile()
        {
            ViewBag.customer = HttpContext.Session.GetObject<Customer>("account");
            return View("Profile");
        }

        [HttpPost]
        public IActionResult UpdateProfile(string fname, string lname, string phone, string address)
        {
            var account_raw = HttpContext.Session.GetObject<Customer>("account");
            dao_user.UpdateProfile(account_raw.Email, fname, lname, phone, address);
            var account = dao_user.CheckAccountExist(account_raw.Email);
            HttpContext.Session.SetObject("account", account);
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public IActionResult UpdateImageProfile(IFormFile imageFile)
        {
            var fileName = Path.GetFileName(imageFile.FileName);
            var account_raw = HttpContext.Session.GetObject<Customer>("account");
            dao_user.UpdateImageProfile(account_raw.Email, fileName);
            var account = dao_user.CheckAccountExist(account_raw.Email);
            HttpContext.Session.SetObject("account", account);
            return RedirectToAction("Profile");
        }

        public IActionResult Security()
        {
            return View("Security");
        }

        [HttpPost]
        public IActionResult ChangePassword(string oldpassword, string newpassword, string renewpassword)
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (dao_user.GetPasswordAttempts(account.Email) == 5)
            {
                dao_user.AddUnlockTime(account.Email);
                HttpContext.Session.Clear();
                TempData["error"] = "You try many times. Your account is temporarily locked in 30 mins!";
                return RedirectToAction("Login", "Login");
            }
            else
            {
                if (newpassword != renewpassword)
                {
                    ViewBag.message = "Re-password is not match with New Password!";
                    return View("Security");
                }
                else if (oldpassword != account.Password)
                {
                    dao_user.UpdatePasswordAttempts(account.Email);
                    ViewBag.message = "Old password is wrong!";
                    return View("Security");
                }
                else if (newpassword.Contains(" "))
                {
                    ViewBag.message = "Password should not contain spaces.";
                    return View("Security");
                }
                else if (Regex.IsMatch(newpassword, @".*\s{2,}.*"))
                {
                    ViewBag.message = "Password should not contain consecutive spaces.";
                    return View("Security");
                }
                else if (!Regex.IsMatch(newpassword, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[@#$%^&+=!]).{8,}$"))
                {
                    string passError = "Password does not meet complexity requirements. You are missing:\n";
                    if (!Regex.IsMatch(newpassword, @".*[A-Z].*"))
                    {
                        passError += "  - at least one uppercase letter\n";
                    }
                    if (!Regex.IsMatch(newpassword, @".*[a-z].*"))
                    {
                        passError += "  - at least one lowercase letter\n";
                    }
                    if (!Regex.IsMatch(newpassword, @".*\d.*"))
                    {
                        passError += "  - at least one digit\n";
                    }
                    if (!Regex.IsMatch(newpassword, @".*[@#$%^&+=!].*"))
                    {
                        passError += "  - at least one special character\n";
                    }
                    if (newpassword.Length < 8)
                    {
                        passError += "  - a minimum length of 8 characters\n";
                    }
                    ViewBag.message = passError;
                    return View("Security");
                }
                else
                {
                    dao_user.ChangePassword(account.Email, newpassword);
                    dao_user.ResetPasswordAttempts(account.Email);
                    ViewBag.message = "Change password successfully! Redirecting to Profile page in a few second....";
                    return View("Security");
                }
            }
        }

        public IActionResult CheckpointPurchase()
        {
            return View("CheckpointPurchase");
        }

        [HttpPost]
        public ActionResult CheckpointPurchase(string password)
        {
            string userResponse = HttpContext.Request.Form["g-recaptcha-response"];
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (dao_user.GetPasswordAttempts(account.Email) == 5)
            {
                dao_user.AddUnlockTime(account.Email);
                HttpContext.Session.Clear();
                TempData["error"] = "You try many times. Your account is temporarily locked in 30 mins!";
                return RedirectToAction("Login", "Login");
            }
            else if (!account.Password.Equals(password))
            {
                dao_user.UpdatePasswordAttempts(account.Email);
                ViewBag.error = "Wrong Password!";
                return View("CheckpointPurchase");
            }
            else
            {
                return RedirectToAction("Finance", "Profile");
            }
        }

        public async Task<IActionResult> Finance()
        {
            var account_raw = HttpContext.Session.GetObject<Customer>("account");
            var account = dao_user.CheckAccountExist(account_raw.Email);
            string formattedBalance = account.Balance.ToString("#,##0.00");
            ViewBag.balance = formattedBalance;
            ViewBag.list_transaction = finance_dao.GetAllTransactions(account.CustomerId);
            ViewBag.QrCode = await _qrService.GetQRDataURL(account.CustomerId);
            return View("Finance");
        }
        public IActionResult RedirectPaypal()
        {
            return View("RedirectPayPal");
        }

        public IActionResult SubmitPurchase(decimal amount)
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            finance_dao.UpdateBalance(account.CustomerId, amount);
            finance_dao.AddTransaction(account.CustomerId, amount);
            return RedirectToAction("Finance");
        }

        [HttpPost]
        public IActionResult FinanceSearch(string search)
        {
            if (search == null)
            {
                return RedirectToAction("Finance");
            }
            else
            {
                var account = HttpContext.Session.GetObject<Customer>("account");
                ViewBag.list_transaction = finance_dao.SearchTransaction(account.CustomerId, search);
                string formattedBalance = account.Balance.ToString("#,##0.00");
                ViewBag.balance = formattedBalance;
                ViewBag.search = search;
                return View("Finance");
            }
        }

        public IActionResult Rental()
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            ViewBag.listRentalContract = finance_dao.GetAllRentalContracts(account.CustomerId);
            return View("Rental");
        }

        public IActionResult RentalRequest()
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (account != null && account.Role == 1) 
            {
                var listRentalContract = finance_dao.GetAllWaitingRentalContractsForAdmin();
                var listHomes = new List<Homestay>();
                foreach (var ct in listRentalContract)
                {
                    listHomes.AddRange(homestay_dao.GetHomestaysByBookingContractRoomID(ct.RoomId));
                }
                ViewBag.listRentalContract = listRentalContract;
                ViewBag.listHomes = listHomes;
                ViewBag.customer = HttpContext.Session.GetObject<Customer>("account");
                return View("RentalRequest");
            }
            return RedirectToAction("LoadHomestay", "Homestay");
        }

        public IActionResult ConfirmRental(int rental_id)
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (account != null && account.Role == 1)
            {
                finance_dao.UpdateStatusRentalContract(rental_id, "ĐANG THUÊ");
                var thisContract = finance_dao.GetRentalContractByID(rental_id);
                var thisHomestay = homestay_dao.getHomestayByID(thisContract.RoomId);

                SendConfirmedBookingMailToCustomer(thisContract, thisHomestay);

                var listRentalContract = finance_dao.GetAllWaitingRentalContractsForAdmin();
                var listHomes = new List<Homestay>();
                foreach (var ct in listRentalContract)
                {
                    listHomes.AddRange(homestay_dao.GetHomestaysByBookingContractRoomID(ct.RoomId));
                }
                ViewBag.listRentalContract = listRentalContract;
                ViewBag.listHomes = listHomes;
                ViewBag.customer = HttpContext.Session.GetObject<Customer>("account");
                return View("RentalRequest");
            }
            return RedirectToAction("LoadHomestay", "Homestay");
        }

        private void SendConfirmedBookingMailToCustomer(BookingContract contract, Homestay homestay)
        {
            string subject = "XÁC NHẬN THÀNH CÔNG ĐƠN ĐẶT PHÒNG TẠI TamDaoStay";
            EmailService emailService = new EmailService();
            string htmlContent = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\">\r\n<head>\r\n<!--[if gte mso 9]>\r\n<xml>\r\n  <o:OfficeDocumentSettings>\r\n    <o:AllowPNG/>\r\n    <o:PixelsPerInch>96</o:PixelsPerInch>\r\n  </o:OfficeDocumentSettings>\r\n</xml>\r\n<![endif]-->\r\n  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <meta name=\"x-apple-disable-message-reformatting\">\r\n  <!--[if !mso]><!--><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><!--<![endif]-->\r\n  <title></title>\r\n  \r\n    <style type=\"text/css\">\r\n      \r\n      @media only screen and (min-width: 620px) {\r\n        .u-row {\r\n          width: 600px !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          vertical-align: top;\r\n        }\r\n\r\n        \r\n            .u-row .u-col-50 {\r\n              width: 300px !important;\r\n            }\r\n          \r\n\r\n            .u-row .u-col-100 {\r\n              width: 600px !important;\r\n            }\r\n          \r\n      }\r\n\r\n      @media only screen and (max-width: 620px) {\r\n        .u-row-container {\r\n          max-width: 100% !important;\r\n          padding-left: 0px !important;\r\n          padding-right: 0px !important;\r\n        }\r\n\r\n        .u-row {\r\n          width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          display: block !important;\r\n          width: 100% !important;\r\n          min-width: 320px !important;\r\n          max-width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col > div {\r\n          margin: 0 auto;\r\n        }\r\n\r\n\r\n        .u-row .u-col img {\r\n          max-width: 100% !important;\r\n        }\r\n\r\n}\r\n    \r\nbody {\r\n  margin: 0;\r\n  padding: 0;\r\n}\r\n\r\ntable,\r\ntr,\r\ntd {\r\n  vertical-align: top;\r\n  border-collapse: collapse;\r\n}\r\n\r\np {\r\n  margin: 0;\r\n}\r\n\r\n.ie-container table,\r\n.mso-container table {\r\n  table-layout: fixed;\r\n}\r\n\r\n* {\r\n  line-height: inherit;\r\n}\r\n\r\na[x-apple-data-detectors='true'] {\r\n  color: inherit !important;\r\n  text-decoration: none !important;\r\n}\r\n\r\n\r\n\r\ntable, td { color: #000000; } #u_body a { color: #0000ee; text-decoration: underline; } @media (max-width: 480px) { #u_content_text_7 .v-text-align { text-align: center !important; } #u_content_text_8 .v-text-align { text-align: center !important; } }\r\n    </style>\r\n  \r\n  \r\n\r\n<!--[if !mso]><!--><link href=\"https://fonts.googleapis.com/css?family=Cabin:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><!--<![endif]-->\r\n\r\n</head>\r\n\r\n<body class=\"clean-body u_body\" style=\"margin: 0;padding: 0;-webkit-text-size-adjust: 100%;background-color: #ecf0f1;color: #000000\">\r\n  <!--[if IE]><div class=\"ie-container\"><![endif]-->\r\n  <!--[if mso]><div class=\"mso-container\"><![endif]-->\r\n  <table id=\"u_body\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;min-width: 320px;Margin: 0 auto;background-color: #ecf0f1;width:100%\" cellpadding=\"0\" cellspacing=\"0\">\r\n  <tbody>\r\n  <tr style=\"vertical-align: top\">\r\n    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n    <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td align=\"center\" style=\"background-color: #ecf0f1;\"><![endif]-->\r\n    \r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:6px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n    <tbody>\r\n      <tr style=\"vertical-align: top\">\r\n        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n          <span>&#160;</span>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:20px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n  <tr>\r\n    <td class=\"v-text-align\" style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\">\r\n      \r\n      <img align=\"center\" border=\"0\" src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451038/OCBK/MAIL_ASSETS/xfasw3y2sbkptt1okql6.png\" alt=\"Image\" title=\"Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 72%;max-width: 417.6px;\" width=\"417.6\"/>\r\n      \r\n    </td>\r\n  </tr>\r\n</table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:30px 10px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; color: #4fadc9; line-height: 140%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"line-height: 140%; text-align: center;\"><span style=\"font-family: tahoma, arial, helvetica, sans-serif; line-height: 19.6px;\"><span style=\"font-size: 22px; line-height: 30.8px;\"><strong><span style=\"font-size: 20px; line-height: 28px;\">CHÚC MỪNG QUÝ KHÁCH ĐÃ ĐẶT PHÒNG THÀNH CÔNG</span></strong></span></span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:0px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 140%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\"><strong>Kính gửi Quý khách {{CUSTOMER_NAME}},</strong></span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Chúng tôi rất vui mừng thông báo rằng đơn đặt phòng của Quý khách tại <strong>Homestay {{HOMESTAY_NAME}}</strong> thuộc <strong>Tam Đảo, Vĩnh Phúc</strong> đã được xác nhận thành công!</span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Quý khách sẽ có những trải nghiệm tuyệt vời với không gian nghỉ dưỡng sang trọng, tiện nghi và dịch vụ tận tâm tại khu đô thị hiện đại bậc nhất này. Dưới đây là thông tin chi tiết về đặt phòng của Quý khách:</span></p>\r\n<ul>\r\n<li style=\"line-height: 22.4px;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\"><strong>Ngày nhận phòng:</strong> {{CHECK_IN_DATE}}</span></li>\r\n<li style=\"line-height: 22.4px;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\"><strong>Ngày trả phòng:</strong> {{CHECK_OUT_DATE}}</span></li>\r\n<li style=\"line-height: 22.4px;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\"><strong>Địa chỉ homestay:</strong> {{HOMESTAY_ADDRESS}}</span></li>\r\n</ul>\r\n<p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Chúng tôi cam kết sẽ mang đến cho Quý khách những phút giây thư giãn và thoải mái nhất. Nếu Quý khách có bất kỳ yêu cầu hay thắc mắc nào, đừng ngần ngại liên hệ với chúng tôi qua địa chỉ email <strong>sys.TamDaoStay@gmail.com</strong> để được hỗ trợ. </span></p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n<p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Một lần nữa, xin chúc mừng Quý khách và chúc Quý khách có kỳ nghỉ tuyệt vời tại Tam Đảo, Vĩnh Phúc!</span></p>\r\n<p style=\"line-height: 140%;\"> </p>\r\n<p style=\"line-height: 140%;\"><em><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Lưu ý: Vui lòng xuất trình hoá đơn ở liên kết bên dưới khi nhận phòng.</span></em></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px 10px 50px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]-->\r\n<div class=\"v-text-align\" align=\"center\">\r\n  <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"https://www.facebook.com/tranhiwp/\" style=\"height:47px; v-text-anchor:middle; width:295px;\" arcsize=\"40.5%\"  stroke=\"f\" fillcolor=\"#4fadc9\"><w:anchorlock/><center style=\"color:#FFFFFF;\"><![endif]-->\r\n    <a href=\"{{INVOICE_LINK}}\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #4fadc9; border-radius: 19px;-webkit-border-radius: 19px; -moz-border-radius: 19px; width:auto; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\">\r\n      <span style=\"display:block;padding:13px 24px 12px;line-height:120%;\"><span style=\"font-size: 18px; line-height: 21.6px; font-family: tahoma, arial, helvetica, sans-serif;\"><strong><span style=\"line-height: 21.6px; font-size: 18px;\">XEM HOÁ ĐƠN ĐẶT PHÒNG</span></strong></span></span>\r\n    </a>\r\n    <!--[if mso]></center></v:roundrect><![endif]-->\r\n</div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #2d529d;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #2d529d;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table id=\"u_content_text_7\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:23px 60px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 160%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-family: 'Open Sans', sans-serif; font-size: 14px; line-height: 22.4px;\"><strong><span style=\"line-height: 22.4px; color: #ffffff; font-size: 14px;\">Trân trọng,</span></strong></span></p>\r\n<p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-size: 14px; line-height: 22.4px; font-family: 'Open Sans', sans-serif; color: #ffffff;\">TamDaoStay</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:25px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<div align=\"center\">\r\n  <div style=\"display: table; max-width:89px;\">\r\n  <!--[if (mso)|(IE)]><table width=\"89\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"border-collapse:collapse;\" align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse; mso-table-lspace: 0pt;mso-table-rspace: 0pt; width:89px;\"><tr><![endif]-->\r\n  \r\n    \r\n    <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 13px;\" valign=\"top\"><![endif]-->\r\n      <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 13px\">\r\n        <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n          <a href=\"https://www.facebook.com/profile.php?id=61565969259252\" title=\"Facebook\" target=\"_blank\">\r\n            <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/uf5ctobb6rvkyqfyzymn.png\" alt=\"Facebook\" title=\"Facebook\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n          </a>\r\n        </td></tr>\r\n      </tbody></table>\r\n      <!--[if (mso)|(IE)]></td><![endif]-->\r\n      \r\n      <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 0px;\" valign=\"top\"><![endif]-->\r\n      <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 0px\">\r\n        <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n          <a href=\"mailto:sys.TamDaoStay@gmail.com\" title=\"Email\" target=\"_blank\">\r\n            <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/vwxmktzvydbnwggpcu5g.png\" alt=\"Email\" title=\"Email\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n          </a>\r\n        </td></tr>\r\n      </tbody></table>\r\n      <!--[if (mso)|(IE)]></td><![endif]-->\r\n    \r\n    \r\n    <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n  </div>\r\n</div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n<table id=\"u_content_text_8\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:5px 10px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; color: #4fadc9; line-height: 140%; text-align: center; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: center;\"><span style=\"font-size: 16px; line-height: 22.4px; color: #ffffff; font-family: Cabin, sans-serif;\">www.TamDaoStay.online</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n    <!--[if (mso)|(IE)]></td></tr></table><![endif]-->\r\n    </td>\r\n  </tr>\r\n  </tbody>\r\n  </table>\r\n  <!--[if mso]></div><![endif]-->\r\n  <!--[if IE]></div><![endif]-->\r\n</body>\r\n\r\n</html>\r\n";
            if (htmlContent != "")
            {
                htmlContent = htmlContent.Replace("{{CUSTOMER_NAME}}", contract.LastName);
                htmlContent = htmlContent.Replace("{{HOMESTAY_NAME}}", homestay.Name);
                htmlContent = htmlContent.Replace("{{CHECK_IN_DATE}}", DateTimeHelper.GetFormatedDate(contract.StartDate));
                htmlContent = htmlContent.Replace("{{CHECK_OUT_DATE}}", DateTimeHelper.GetFormatedDate(contract.EndDate));
                htmlContent = htmlContent.Replace("{{HOMESTAY_ADDRESS}}", homestay.Address + " khu " + homestay.City);

                string invoice_link = $"{Request.Scheme}://{Request.Host}" + "/Profile/ViewRentalDetail?rental_id=" + contract.BookingId;
                htmlContent = htmlContent.Replace("{{INVOICE_LINK}}", invoice_link);
                Thread t = new Thread(() => EmailService.SendEmailMultiThread(contract.Email, subject, htmlContent));
                t.Start();
            }
        }

        [HttpPost]
        public IActionResult RentalSearch(string search)
        {
            if (search == null)
            {
                return RedirectToAction("Rental");
            }
            else
            {
                var account = HttpContext.Session.GetObject<Customer>("account");
                ViewBag.listRentalContract = finance_dao.SearchRentalContracts(account.CustomerId, search);
                ViewBag.search = search;
            }
            return View("Rental");
        }

        public IActionResult ViewRentalDetail(int rental_id)
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (account != null)
            {
                var rental_contract = finance_dao.GetRentalContractByID(rental_id);
                if ((account.Role == 0 && account.CustomerId == rental_contract.CustomerId) || account.Role == 1)
                {
                    DateTime start_date = rental_contract.StartDate;
                    DateTime end_date = rental_contract.EndDate;
                    TimeSpan duration = end_date - start_date;
                    int numberOfDays = duration.Days;
                    if (numberOfDays == 0)
                    {
                        numberOfDays = 1;
                    }
                    var homestay = homestay_dao.getHomestayByID(rental_contract.RoomId);
                    ViewBag.NumberOfDays = numberOfDays;
                    ViewBag.transaction = finance_dao.GetTransactionByRentalId(rental_id);
                    ViewBag.contract = rental_contract;
                    ViewBag.homestay = homestay;
                    ViewBag.host = homestay_dao.GetHostByID(rental_contract.RoomId);

                    return View("ViewRentalDetail");
                }
            }
            return RedirectToAction("Login", "Login");
        }

        public IActionResult RentalCanceled(int rental_id)
        {
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (account != null && account.Role == 1)
            {
                finance_dao.UpdateStatusRentalContract(rental_id, "ĐÃ HUỶ");

                var thisContract = finance_dao.GetRentalContractByID(rental_id);
                var thisHomestay = homestay_dao.getHomestayByID(thisContract.RoomId);

                SendCanceledBookingMailToCustomer(thisContract, thisHomestay);

                var listRentalContract = finance_dao.GetAllWaitingRentalContractsForAdmin();
                var listHomes = new List<Homestay>();
                foreach (var ct in listRentalContract)
                {
                    listHomes.AddRange(homestay_dao.GetHomestaysByBookingContractRoomID(ct.RoomId));
                }
                ViewBag.listRentalContract = listRentalContract;
                ViewBag.listHomes = listHomes;
                ViewBag.customer = HttpContext.Session.GetObject<Customer>("account");
                return View("RentalRequest");
            }
            return RedirectToAction("LoadHomestay", "Homestay");
        }

        private void SendCanceledBookingMailToCustomer(BookingContract contract, Homestay homestay)
        {
            string subject = "ĐƠN ĐẶT PHÒNG TẠI TamDaoStay CỦA BẠN ĐÃ BỊ TỪ CHỐI";
            EmailService emailService = new EmailService();
            string htmlContent = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\">\r\n<head>\r\n<!--[if gte mso 9]>\r\n<xml>\r\n  <o:OfficeDocumentSettings>\r\n    <o:AllowPNG/>\r\n    <o:PixelsPerInch>96</o:PixelsPerInch>\r\n  </o:OfficeDocumentSettings>\r\n</xml>\r\n<![endif]-->\r\n  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <meta name=\"x-apple-disable-message-reformatting\">\r\n  <!--[if !mso]><!--><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><!--<![endif]-->\r\n  <title></title>\r\n  \r\n    <style type=\"text/css\">\r\n      \r\n      @media only screen and (min-width: 620px) {\r\n        .u-row {\r\n          width: 600px !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          vertical-align: top;\r\n        }\r\n\r\n        \r\n            .u-row .u-col-50 {\r\n              width: 300px !important;\r\n            }\r\n          \r\n\r\n            .u-row .u-col-100 {\r\n              width: 600px !important;\r\n            }\r\n          \r\n      }\r\n\r\n      @media only screen and (max-width: 620px) {\r\n        .u-row-container {\r\n          max-width: 100% !important;\r\n          padding-left: 0px !important;\r\n          padding-right: 0px !important;\r\n        }\r\n\r\n        .u-row {\r\n          width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          display: block !important;\r\n          width: 100% !important;\r\n          min-width: 320px !important;\r\n          max-width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col > div {\r\n          margin: 0 auto;\r\n        }\r\n\r\n\r\n        .u-row .u-col img {\r\n          max-width: 100% !important;\r\n        }\r\n\r\n}\r\n    \r\nbody {\r\n  margin: 0;\r\n  padding: 0;\r\n}\r\n\r\ntable,\r\ntr,\r\ntd {\r\n  vertical-align: top;\r\n  border-collapse: collapse;\r\n}\r\n\r\np {\r\n  margin: 0;\r\n}\r\n\r\n.ie-container table,\r\n.mso-container table {\r\n  table-layout: fixed;\r\n}\r\n\r\n* {\r\n  line-height: inherit;\r\n}\r\n\r\na[x-apple-data-detectors='true'] {\r\n  color: inherit !important;\r\n  text-decoration: none !important;\r\n}\r\n\r\n\r\n\r\ntable, td { color: #000000; } #u_body a { color: #0000ee; text-decoration: underline; } @media (max-width: 480px) { #u_content_text_7 .v-text-align { text-align: center !important; } #u_content_text_8 .v-text-align { text-align: center !important; } }\r\n    </style>\r\n  \r\n  \r\n\r\n<!--[if !mso]><!--><link href=\"https://fonts.googleapis.com/css?family=Cabin:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><!--<![endif]-->\r\n\r\n</head>\r\n\r\n<body class=\"clean-body u_body\" style=\"margin: 0;padding: 0;-webkit-text-size-adjust: 100%;background-color: #ecf0f1;color: #000000\">\r\n  <!--[if IE]><div class=\"ie-container\"><![endif]-->\r\n  <!--[if mso]><div class=\"mso-container\"><![endif]-->\r\n  <table id=\"u_body\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;min-width: 320px;Margin: 0 auto;background-color: #ecf0f1;width:100%\" cellpadding=\"0\" cellspacing=\"0\">\r\n  <tbody>\r\n  <tr style=\"vertical-align: top\">\r\n    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n    <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td align=\"center\" style=\"background-color: #ecf0f1;\"><![endif]-->\r\n    \r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:6px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n    <tbody>\r\n      <tr style=\"vertical-align: top\">\r\n        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n          <span>&#160;</span>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:20px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n  <tr>\r\n    <td class=\"v-text-align\" style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\">\r\n      \r\n      <img align=\"center\" border=\"0\" src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451038/OCBK/MAIL_ASSETS/xfasw3y2sbkptt1okql6.png\" alt=\"Image\" title=\"Image\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: inline-block !important;border: none;height: auto;float: none;width: 72%;max-width: 417.6px;\" width=\"417.6\"/>\r\n      \r\n    </td>\r\n  </tr>\r\n</table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:0px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 140%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\"><strong>Kính gửi Quý khách {{CUSTOMER_NAME}},</strong></span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Cảm ơn Quý khách đã tin tưởng và lựa chọn <strong>Homestay {{HOMESTAY_NAME}}</strong> tại <strong>Tam Đảo, Vĩnh Phúc</strong> cho kỳ nghỉ của mình.</span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Chúng tôi rất tiếc phải thông báo rằng, do một số lý do bất khả kháng, đơn đặt phòng của Quý khách không thể được xác nhận vào thời gian mong muốn. Chúng tôi hiểu rằng điều này có thể gây bất tiện cho Quý khách và chúng tôi thành thật xin lỗi về sự cố này.</span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Nếu có thể, chúng tôi rất mong Quý khách sẽ cân nhắc một khoảng thời gian khác hoặc lựa chọn một trong những homestay khác trong hệ thống của chúng tôi. Đội ngũ của chúng tôi luôn sẵn sàng hỗ trợ Quý khách tìm kiếm những phương án thay thế phù hợp.</span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Chúng tôi sẽ hoàn tiền đặt phòng vào tài khoản OceanPAY của Quý khách trong thời gian sớm nhất.</span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Nếu Quý khách có bất kỳ thắc mắc hoặc cần hỗ trợ thêm, xin vui lòng liên hệ với chúng tôi qua địa chỉ email <strong>sys.TamDaoStay@gmail.com</strong>, chúng tôi sẽ nỗ lực hết sức để hỗ trợ Quý khách trong thời gian sớm nhất. </span></p>\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n    <p style=\"line-height: 140%;\"><span style=\"font-family: Cabin, sans-serif; line-height: 22.4px;\">Một lần nữa, chúng tôi chân thành xin lỗi và mong Quý khách sẽ tiếp tục ủng hộ chúng tôi trong tương lai.</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #2d529d;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #2d529d;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table id=\"u_content_text_7\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:23px 60px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 160%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-family: 'Open Sans', sans-serif; font-size: 14px; line-height: 22.4px;\"><strong><span style=\"line-height: 22.4px; color: #ffffff; font-size: 14px;\">Trân trọng,</span></strong></span></p>\r\n<p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-size: 14px; line-height: 22.4px; font-family: 'Open Sans', sans-serif; color: #ffffff;\">TamDaoStay</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:25px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<div align=\"center\">\r\n  <div style=\"display: table; max-width:89px;\">\r\n  <!--[if (mso)|(IE)]><table width=\"89\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"border-collapse:collapse;\" align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse; mso-table-lspace: 0pt;mso-table-rspace: 0pt; width:89px;\"><tr><![endif]-->\r\n  \r\n    \r\n    <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 13px;\" valign=\"top\"><![endif]-->\r\n      <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 13px\">\r\n        <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n          <a href=\"https://www.facebook.com/profile.php?id=61565969259252\" title=\"Facebook\" target=\"_blank\">\r\n            <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/uf5ctobb6rvkyqfyzymn.png\" alt=\"Facebook\" title=\"Facebook\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n          </a>\r\n        </td></tr>\r\n      </tbody></table>\r\n      <!--[if (mso)|(IE)]></td><![endif]-->\r\n      \r\n      <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 0px;\" valign=\"top\"><![endif]-->\r\n      <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 0px\">\r\n        <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n          <a href=\"mailto:sys.TamDaoStay@gmail.com\" title=\"Email\" target=\"_blank\">\r\n            <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/vwxmktzvydbnwggpcu5g.png\" alt=\"Email\" title=\"Email\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n          </a>\r\n        </td></tr>\r\n      </tbody></table>\r\n      <!--[if (mso)|(IE)]></td><![endif]-->\r\n    \r\n    \r\n    <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n  </div>\r\n</div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n<table id=\"u_content_text_8\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:5px 10px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; color: #4fadc9; line-height: 140%; text-align: center; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: center;\"><span style=\"font-size: 16px; line-height: 22.4px; color: #ffffff; font-family: Cabin, sans-serif;\">www.TamDaoStay.online</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n    <!--[if (mso)|(IE)]></td></tr></table><![endif]-->\r\n    </td>\r\n  </tr>\r\n  </tbody>\r\n  </table>\r\n  <!--[if mso]></div><![endif]-->\r\n  <!--[if IE]></div><![endif]-->\r\n</body>\r\n\r\n</html>\r\n";
            if (htmlContent != "")
            {
                htmlContent = htmlContent.Replace("{{CUSTOMER_NAME}}", contract.LastName);
                htmlContent = htmlContent.Replace("{{HOMESTAY_NAME}}", homestay.Name);

                Thread t = new Thread(() => EmailService.SendEmailMultiThread(contract.Email, subject, htmlContent));
                t.Start();
            }
        }
    }
}
