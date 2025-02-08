using Microsoft.AspNetCore.Mvc;
using EXE202.DAO;
using EXE202.Models;
using EXE202.Controllers;
using EXE202.Services.Impl;
using EXE202.Utils;

namespace Project_RentACar.Controllers
{
    public class PurchaseController : Controller
    {
        private IQRService _qrService;
        
        private static int ADDITIONAL_FEE = 100000;

        EXE202Context context = new EXE202Context();
        private readonly AccountDAO dao_user;
        private readonly FinanceDAO finance_dao;
        private readonly HomestayDAO homestay_dao;
        public PurchaseController(AccountDAO accountDAO, FinanceDAO financeDAO, HomestayDAO homestayDAO, IQRService qrService)
        {
            _qrService = qrService;
            dao_user = accountDAO;
            finance_dao = financeDAO;
            homestay_dao = homestayDAO;
        }
        public async Task<IActionResult> PurchaseByOceanPAY()
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            var rental_contract = HttpContext.Session.GetObject<BookingContract>("contract");
            DateTime start_date = rental_contract.StartDate;
            DateTime end_date = rental_contract.EndDate;
            TimeSpan duration = end_date - start_date;
            int numberOfDays = duration.Days;
            if (numberOfDays == 0)
            {
                numberOfDays = 1;
            }
            var account = HttpContext.Session.GetObject<Customer>("account");
            var homestay = homestay_dao.getHomestayByID(rental_contract.RoomId);
            string formattedBalance = account.Balance.ToString("#,##0.00");
            string formattedDiscount = (rental_contract.TotalAmount / 100 * 5).ToString("#,##0.00");
            var total = rental_contract.TotalAmount;
            var totalStr = total.ToString("#,##0.00");
            if (account.Balance < total)
            {
                ViewBag.error = "Your OceanPAY balance is not enough!";
            }
            HttpContext.Session.SetString("total_CRP", totalStr);
            ViewBag.homestay = homestay;
            ViewBag.host = homestay_dao.GetHostByID(rental_contract.RoomId);
            ViewBag.contract = rental_contract;
            ViewBag.account_balance = formattedBalance;
            ViewBag.number_of_day = numberOfDays;
            ViewBag.formattedDiscount = formattedDiscount;
            ViewBag.total = totalStr;
            ViewBag.QrCode = await _qrService.GetQRDataURLWithBookingId(rental_contract.BookingId, total);
            ViewBag.CustomerId = customer.CustomerId;
            ViewBag.rentalId = rental_contract.BookingId;
            return View("Checkout");
        }

        public IActionResult PurchaseByPaypal()
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            var rental_contract = HttpContext.Session.GetObject<BookingContract>("contract");
            DateTime start_date = rental_contract.StartDate;
            DateTime end_date = rental_contract.EndDate;
            TimeSpan duration = end_date - start_date;
            int numberOfDays = duration.Days;
            if (numberOfDays == 0)
            {
                numberOfDays = 1;
            }
            var account = HttpContext.Session.GetObject<Customer>("account");
            var homestay = homestay_dao.getHomestayByID(rental_contract.RoomId);
            string formattedBalance = account.Balance.ToString("#,##0.00");
            string vatTax = (rental_contract.TotalAmount / 100 * 10).ToString("#,##0.00");
            string total = (rental_contract.TotalAmount + ADDITIONAL_FEE + rental_contract.TotalAmount / 100 * 10).ToString("#,##0.00");
            HttpContext.Session.SetString("total_paypal", total);
            ViewBag.homestay = homestay;
            ViewBag.host = homestay_dao.getHomestayByID(rental_contract.RoomId);
            ViewBag.contract = rental_contract;
            ViewBag.account_balance = formattedBalance;
            ViewBag.number_of_day = numberOfDays;
            ViewBag.formattedDiscount = "0";
            ViewBag.vat_tax = vatTax;
            ViewBag.total = total;
            return View("Checkout");
        }
        public IActionResult CheckpointPurchase()
        {
            return View("CheckpointPurchase");
        }

        [HttpPost]
        public ActionResult CheckpointPurchase(string password)
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            string userResponse = HttpContext.Request.Form["g-recaptcha-response"];
            var account = HttpContext.Session.GetObject<Customer>("account");
            if (userResponse.Equals(""))
            {
                ViewBag.error = "You must verify captcha!";
                return View("CheckpointPurchase");
            }
            else if (dao_user.GetPasswordAttempts(account.Email) == 5)
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
                return RedirectToAction("CheckoutByOceanPAY", "Purchase");
            }
        }

        public IActionResult CheckoutByOceanPAY()
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            var rental_contract = HttpContext.Session.GetObject<BookingContract>("contract");
            string selectedPaymentMethod = "OceanPAY";
            string total = HttpContext.Session.GetString("total_CRP");
            decimal total_amount = 0;
            Decimal.TryParse(total, out total_amount);
            BookingContract rentalContract = new BookingContract()
            {
                RoomId = rental_contract.RoomId,
                CustomerId = rental_contract.CustomerId,
                FirstName = rental_contract.FirstName,
                LastName = rental_contract.LastName,
                Email = rental_contract.Email,
                Phone = rental_contract.Phone,
                Destination = rental_contract.Destination,
                StartDate = rental_contract.StartDate,
                EndDate = rental_contract.EndDate,
                TotalAmount = total_amount,
                PaymentMethod = selectedPaymentMethod,
                Note = rental_contract.Note,
                Status = "CHỜ XÁC NHẬN",
            };
            context.BookingContracts.Add(rentalContract);
            context.SaveChanges();

            var thisHomestay = homestay_dao.getHomestayByID(rentalContract.RoomId);
            SendPurchaseSuccessfulMailToCustomer(rentalContract, thisHomestay);

            HttpContext.Session.SetObject("contract", rentalContract);

            decimal amount = Convert.ToDecimal(HttpContext.Session.GetString("total_CRP"));
            finance_dao.UpdateBalance1(rentalContract.CustomerId, amount);
            finance_dao.AddTransaction1(rentalContract.CustomerId, amount, rentalContract.BookingId);
            return RedirectToAction("Rental", "Profile");
        }

        private void SendPurchaseSuccessfulMailToCustomer(BookingContract contract, Homestay thisHomestay)
        {
            string subject = "THÔNG BÁO ĐẶT PHÒNG THÀNH CÔNG TẠI TamDaoStay";
            EmailService emailService = new EmailService();
            string htmlContent = "<!DOCTYPE HTML PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional //EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n<html xmlns=\"http://www.w3.org/1999/xhtml\" xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:o=\"urn:schemas-microsoft-com:office:office\">\r\n<head>\r\n<!--[if gte mso 9]>\r\n<xml>\r\n  <o:OfficeDocumentSettings>\r\n    <o:AllowPNG/>\r\n    <o:PixelsPerInch>96</o:PixelsPerInch>\r\n  </o:OfficeDocumentSettings>\r\n</xml>\r\n<![endif]-->\r\n  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=UTF-8\">\r\n  <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n  <meta name=\"x-apple-disable-message-reformatting\">\r\n  <!--[if !mso]><!--><meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\"><!--<![endif]-->\r\n  <title></title>\r\n  \r\n    <style type=\"text/css\">\r\n      \r\n      @media only screen and (min-width: 620px) {\r\n        .u-row {\r\n          width: 600px !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          vertical-align: top;\r\n        }\r\n\r\n        \r\n            .u-row .u-col-50 {\r\n              width: 300px !important;\r\n            }\r\n          \r\n\r\n            .u-row .u-col-100 {\r\n              width: 600px !important;\r\n            }\r\n          \r\n      }\r\n\r\n      @media only screen and (max-width: 620px) {\r\n        .u-row-container {\r\n          max-width: 100% !important;\r\n          padding-left: 0px !important;\r\n          padding-right: 0px !important;\r\n        }\r\n\r\n        .u-row {\r\n          width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col {\r\n          display: block !important;\r\n          width: 100% !important;\r\n          min-width: 320px !important;\r\n          max-width: 100% !important;\r\n        }\r\n\r\n        .u-row .u-col > div {\r\n          margin: 0 auto;\r\n        }\r\n\r\n\r\n        .u-row .u-col img {\r\n          max-width: 100% !important;\r\n        }\r\n\r\n}\r\n    \r\nbody {\r\n  margin: 0;\r\n  padding: 0;\r\n}\r\n\r\ntable,\r\ntr,\r\ntd {\r\n  vertical-align: top;\r\n  border-collapse: collapse;\r\n}\r\n\r\np {\r\n  margin: 0;\r\n}\r\n\r\n.ie-container table,\r\n.mso-container table {\r\n  table-layout: fixed;\r\n}\r\n\r\n* {\r\n  line-height: inherit;\r\n}\r\n\r\na[x-apple-data-detectors='true'] {\r\n  color: inherit !important;\r\n  text-decoration: none !important;\r\n}\r\n\r\n\r\n\r\ntable, td { color: #000000; } #u_body a { color: #0000ee; text-decoration: underline; } @media (max-width: 480px) { #u_content_text_7 .v-text-align { text-align: center !important; } #u_content_text_8 .v-text-align { text-align: center !important; } }\r\n    </style>\r\n  \r\n  \r\n\r\n<!--[if !mso]><!--><link href=\"https://fonts.googleapis.com/css?family=Cabin:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><link href=\"https://fonts.googleapis.com/css?family=Open+Sans:400,700&display=swap\" rel=\"stylesheet\" type=\"text/css\"><!--<![endif]-->\r\n\r\n</head>\r\n\r\n<body class=\"clean-body u_body\" style=\"margin: 0;padding: 0;-webkit-text-size-adjust: 100%;background-color: #ecf0f1;color: #000000\">\r\n  <!--[if IE]><div class=\"ie-container\"><![endif]-->\r\n  <!--[if mso]><div class=\"mso-container\"><![endif]-->\r\n  <table id=\"u_body\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;min-width: 320px;Margin: 0 auto;background-color: #ecf0f1;width:100%\" cellpadding=\"0\" cellspacing=\"0\">\r\n  <tbody>\r\n  <tr style=\"vertical-align: top\">\r\n    <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n    <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td align=\"center\" style=\"background-color: #ecf0f1;\"><![endif]-->\r\n    \r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: transparent;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: transparent;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:6px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <table height=\"0px\" align=\"center\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" style=\"border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;border-top: 0px solid #ffffff;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n    <tbody>\r\n      <tr style=\"vertical-align: top\">\r\n        <td style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top;font-size: 0px;line-height: 0px;mso-line-height-rule: exactly;-ms-text-size-adjust: 100%;-webkit-text-size-adjust: 100%\">\r\n          <span>&#160;</span>\r\n        </td>\r\n      </tr>\r\n    </tbody>\r\n  </table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:20px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\">\r\n  <tr>\r\n    <td class=\"v-text-align\" style=\"padding-right: 0px;padding-left: 0px;\" align=\"center\">\r\n      \r\n      \r\n      \r\n    </td>\r\n  </tr>\r\n</table>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:30px 10px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; color: #4fadc9; line-height: 140%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"line-height: 140%; text-align: center;\"><span style=\"font-family: tahoma, arial, helvetica, sans-serif; line-height: 19.6px;\"><span style=\"font-size: 22px; line-height: 30.8px;\"><strong>CẢM ƠN BẠN ĐÃ LỰA CHỌN TamDaoStay</strong></span></span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:0px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 140%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"line-height: 140%;\"><span style=\"font-size: 16px; line-height: 22.4px; font-family: Cabin, sans-serif;\"><strong>Kính gửi Quý khách {{CUSTOMER_NAME}},</strong></span></p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"> </p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: justify;\"><span style=\"font-size: 16px; line-height: 22.4px; font-family: Cabin, sans-serif;\">Chúng tôi xin chân thành cảm ơn Quý khách đã lựa chọn <strong>{{HOMESTAY_NAME}}</strong> tại <strong>Tam Đảo, Vĩnh Phúc</strong> cho kỳ nghỉ của mình!</span></p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-size: 16px; line-height: 22.4px; font-family: Cabin, sans-serif;\">Xin Quý khách vui lòng đợi, chúng tôi sẽ gửi thông tin xác nhận chính thức trong thời gian sớm nhất. Nếu có bất kỳ thắc mắc hoặc yêu cầu bổ sung, Quý khách vui lòng liên hệ với chúng tôi qua địa chỉ email <strong>sys.TamDaoStay@gmail.com</strong>, và chúng tôi sẽ hỗ trợ ngay.</span></p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: left;\"><span style=\"font-size: 16px; line-height: 22.4px; font-family: Cabin, sans-serif;\">Một lần nữa, chúng tôi rất cảm ơn sự ủng hộ của Quý khách và hy vọng sẽ mang lại trải nghiệm nghỉ dưỡng tuyệt vời nhất cho Quý khách tại <strong>{{HOMESTAY_NAME}}</strong>.</span></p>\r\n<p style=\"font-size: 14px; line-height: 140%; text-align: left;\"> </p>\r\n\r\n</div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #ffffff;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #ffffff;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"600\" style=\"width: 600px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-100\" style=\"max-width: 320px;min-width: 600px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:10px 10px 50px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <!--[if mso]><style>.v-button {background: transparent !important;}</style><![endif]-->\r\n<div class=\"v-text-align\" align=\"center\">\r\n  <!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\" href=\"https://www.facebook.com/tranhiwp/\" style=\"height:47px; v-text-anchor:middle; width:367px;\" arcsize=\"40.5%\"  stroke=\"f\" fillcolor=\"#4fadc9\"><w:anchorlock/><center style=\"color:#FFFFFF;\"><![endif]-->\r\n    <a href=\"{{INVOICE_LINK}}\" target=\"_blank\" class=\"v-button\" style=\"box-sizing: border-box;display: inline-block;text-decoration: none;-webkit-text-size-adjust: none;text-align: center;color: #FFFFFF; background-color: #4fadc9; border-radius: 19px;-webkit-border-radius: 19px; -moz-border-radius: 19px; width:auto; max-width:100%; overflow-wrap: break-word; word-break: break-word; word-wrap:break-word; mso-border-alt: none;font-size: 14px;\">\r\n      <span style=\"display:block;padding:13px 24px 12px;line-height:120%;\"><span style=\"font-size: 18px; line-height: 21.6px; font-family: tahoma, arial, helvetica, sans-serif;\"><strong><span style=\"line-height: 21.6px; font-size: 18px;\">Theo dõi hoá đơn đặt phòng tại đây</span></strong></span></span>\r\n    </a>\r\n    <!--[if mso]></center></v:roundrect><![endif]-->\r\n</div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n  \r\n  \r\n<div class=\"u-row-container\" style=\"padding: 0px;background-color: transparent\">\r\n  <div class=\"u-row\" style=\"margin: 0 auto;min-width: 320px;max-width: 600px;overflow-wrap: break-word;word-wrap: break-word;word-break: break-word;background-color: #2d529d;\">\r\n    <div style=\"border-collapse: collapse;display: table;width: 100%;height: 100%;background-color: transparent;\">\r\n      <!--[if (mso)|(IE)]><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"padding: 0px;background-color: transparent;\" align=\"center\"><table cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"width:600px;\"><tr style=\"background-color: #2d529d;\"><![endif]-->\r\n      \r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table id=\"u_content_text_7\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:23px 60px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; line-height: 160%; text-align: left; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-family: 'Open Sans', sans-serif; font-size: 14px; line-height: 22.4px;\"><strong><span style=\"line-height: 22.4px; color: #ffffff; font-size: 14px;\">Trân trọng,</span></strong></span></p>\r\n<p style=\"font-size: 14px; line-height: 160%;\"><span style=\"font-size: 14px; line-height: 22.4px; font-family: 'Open Sans', sans-serif; color: #ffffff;\">TamDaoStay</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n<!--[if (mso)|(IE)]><td align=\"center\" width=\"300\" style=\"background-color: #4fadc9;width: 300px;padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\" valign=\"top\"><![endif]-->\r\n<div class=\"u-col u-col-50\" style=\"max-width: 320px;min-width: 300px;display: table-cell;vertical-align: top;\">\r\n  <div style=\"background-color: #4fadc9;height: 100%;width: 100% !important;\">\r\n  <!--[if (!mso)&(!IE)]><!--><div style=\"box-sizing: border-box; height: 100%; padding: 0px;border-top: 0px solid transparent;border-left: 0px solid transparent;border-right: 0px solid transparent;border-bottom: 0px solid transparent;\"><!--<![endif]-->\r\n  \r\n<table style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:25px 10px 10px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n<div align=\"center\">\r\n  <div style=\"display: table; max-width:89px;\">\r\n  <!--[if (mso)|(IE)]><table width=\"89\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\"><tr><td style=\"border-collapse:collapse;\" align=\"center\"><table width=\"100%\" cellpadding=\"0\" cellspacing=\"0\" border=\"0\" style=\"border-collapse:collapse; mso-table-lspace: 0pt;mso-table-rspace: 0pt; width:89px;\"><tr><![endif]-->\r\n  \r\n    \r\n    <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 13px;\" valign=\"top\"><![endif]-->\r\n    <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 13px\">\r\n      <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n        <a href=\"https://www.facebook.com/profile.php?id=61565969259252\" title=\"Facebook\" target=\"_blank\">\r\n          <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/uf5ctobb6rvkyqfyzymn.png\" alt=\"Facebook\" title=\"Facebook\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n        </a>\r\n      </td></tr>\r\n    </tbody></table>\r\n    <!--[if (mso)|(IE)]></td><![endif]-->\r\n    \r\n    <!--[if (mso)|(IE)]><td width=\"32\" style=\"width:32px; padding-right: 0px;\" valign=\"top\"><![endif]-->\r\n    <table align=\"center\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"32\" height=\"32\" style=\"width: 32px !important;height: 32px !important;display: inline-block;border-collapse: collapse;table-layout: fixed;border-spacing: 0;mso-table-lspace: 0pt;mso-table-rspace: 0pt;vertical-align: top;margin-right: 0px\">\r\n      <tbody><tr style=\"vertical-align: top\"><td align=\"center\" valign=\"middle\" style=\"word-break: break-word;border-collapse: collapse !important;vertical-align: top\">\r\n        <a href=\"mailto:sys.TamDaoStay@gmail.com\" title=\"Email\" target=\"_blank\">\r\n          <img src=\"https://res.cloudinary.com/duzrv35z5/image/upload/v1727451037/OCBK/MAIL_ASSETS/vwxmktzvydbnwggpcu5g.png\" alt=\"Email\" title=\"Email\" width=\"32\" style=\"outline: none;text-decoration: none;-ms-interpolation-mode: bicubic;clear: both;display: block !important;border: none;height: auto;float: none;max-width: 32px !important\">\r\n        </a>\r\n      </td></tr>\r\n    </tbody></table>\r\n    <!--[if (mso)|(IE)]></td><![endif]-->\r\n    \r\n    \r\n    <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n  </div>\r\n</div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n<table id=\"u_content_text_8\" style=\"font-family:'Open Sans',sans-serif;\" role=\"presentation\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\" border=\"0\">\r\n  <tbody>\r\n    <tr>\r\n      <td style=\"overflow-wrap:break-word;word-break:break-word;padding:5px 10px 20px;font-family:'Open Sans',sans-serif;\" align=\"left\">\r\n        \r\n  <div class=\"v-text-align\" style=\"font-size: 14px; color: #4fadc9; line-height: 140%; text-align: center; word-wrap: break-word;\">\r\n    <p style=\"font-size: 14px; line-height: 140%; text-align: center;\"><span style=\"font-size: 16px; line-height: 22.4px; color: #ffffff; font-family: Cabin, sans-serif;\">www.TamDaoStay.online</span></p>\r\n  </div>\r\n\r\n      </td>\r\n    </tr>\r\n  </tbody>\r\n</table>\r\n\r\n  <!--[if (!mso)&(!IE)]><!--></div><!--<![endif]-->\r\n  </div>\r\n</div>\r\n<!--[if (mso)|(IE)]></td><![endif]-->\r\n      <!--[if (mso)|(IE)]></tr></table></td></tr></table><![endif]-->\r\n    </div>\r\n  </div>\r\n  </div>\r\n  \r\n\r\n\r\n    <!--[if (mso)|(IE)]></td></tr></table><![endif]-->\r\n    </td>\r\n  </tr>\r\n  </tbody>\r\n  </table>\r\n  <!--[if mso]></div><![endif]-->\r\n  <!--[if IE]></div><![endif]-->\r\n</body>\r\n\r\n</html>\r\n";
            if (htmlContent != "")
            {
                htmlContent = htmlContent.Replace("{{CUSTOMER_NAME}}", contract.LastName);
                htmlContent = htmlContent.Replace("{{HOMESTAY_NAME}}", thisHomestay.Name);

                string invoice_link = $"{Request.Scheme}://{Request.Host}" + "/Profile/ViewRentalDetail?rental_id=" + contract.BookingId;
                htmlContent = htmlContent.Replace("{{INVOICE_LINK}}", invoice_link);
                Thread t = new Thread(() => EmailService.SendEmailMultiThread(contract.Email, subject, htmlContent));
                t.Start();
            }
        }

        public IActionResult CheckoutRedirectPaypal(decimal amount)
        {
            ViewBag.amount = amount;
            return View("CheckoutRedirectPaypal");
        }

        public IActionResult CheckoutByPaypal()
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            var rental_contract = HttpContext.Session.GetObject<BookingContract>("contract");
            string selectedPaymentMethod = "Paypal";

            BookingContract rentalContract = new BookingContract()
            {
                RoomId = rental_contract.RoomId,
                CustomerId = rental_contract.CustomerId,
                FirstName = rental_contract.FirstName,
                LastName = rental_contract.LastName,
                Email = rental_contract.Email,
                Phone = rental_contract.Phone,
                Destination = rental_contract.Destination,
                StartDate = rental_contract.StartDate,
                EndDate = rental_contract.EndDate,
                TotalAmount = rental_contract.TotalAmount,
                PaymentMethod = selectedPaymentMethod,
                Note = rental_contract.Note,
                Status = "CHỜ XÁC NHẬN",
            };
            context.BookingContracts.Add(rentalContract);
            context.SaveChanges();

            HttpContext.Session.SetObject("contract", rentalContract);

            decimal amount = Convert.ToDecimal(HttpContext.Session.GetString("total_paypal"));
            finance_dao.AddTransaction3(rentalContract.CustomerId, amount, rentalContract.BookingId);
            return RedirectToAction("Invoice");
        }

        public IActionResult Invoice()
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            var rental_contract = HttpContext.Session.GetObject<BookingContract>("contract");
            DateTime start_date = rental_contract.StartDate;
            DateTime end_date = rental_contract.EndDate;
            TimeSpan duration = end_date - start_date;
            int numberOfDays = duration.Days;
            if (numberOfDays == 0)
            {
                numberOfDays = 1;
            }

            var homestay = homestay_dao.getHomestayByID(rental_contract.RoomId);
            homestay_dao.UpdateHomestayStatus(rental_contract.RoomId);
            ViewBag.NumberOfDays = numberOfDays;
            ViewBag.transaction = finance_dao.GetTop1Transaction();
            ViewBag.contract = rental_contract;
            ViewBag.homestay = homestay;
            ViewBag.host = homestay_dao.GetHostByID(rental_contract.RoomId);
            return View("Invoice");
        }
        public IActionResult Exit()
        {
            var sessionKeys = HttpContext.Session.Keys.ToList();

            foreach (var key in sessionKeys)
            {
                if (key != "account")
                {
                    HttpContext.Session.Remove(key);
                }
            }
            return RedirectToAction("LoadHomestay", "Homestay");
        }

        public async Task<IActionResult> PurchaseConfirmation()
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            var rental_contract = HttpContext.Session.GetObject<BookingContract>("contract");
            DateTime start_date = rental_contract.StartDate;
            DateTime end_date = rental_contract.EndDate;
            TimeSpan duration = end_date - start_date;
            int numberOfDays = duration.Days;
            if (numberOfDays == 0)
            {
                numberOfDays = 1;
            }

            var homestay = homestay_dao.getHomestayByID(rental_contract.RoomId);
            homestay_dao.UpdateHomestayStatus(rental_contract.RoomId);
            ViewBag.NumberOfDays = numberOfDays;
            ViewBag.transaction = finance_dao.GetTop1Transaction();
            ViewBag.contract = rental_contract;
            ViewBag.homestay = homestay;
            ViewBag.host = homestay_dao.GetHostByID(rental_contract.RoomId);
            SendPurchaseSuccessfulMailToCustomer(rental_contract, homestay);

            return RedirectToAction("ViewRentalDetail", "Profile", new { rental_id = rental_contract.BookingId });
        }
    }
}
