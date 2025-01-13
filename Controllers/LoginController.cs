using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using EXE202.DAO;


namespace EXE202.Controllers
{
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }

        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }

    public class LoginController : Controller
    {
        private readonly AccountDAO dao_user;
        public LoginController(AccountDAO accountDAO)
        {
            dao_user = accountDAO;
        }
        public IActionResult Login(string homestay_id)
        {
            if (homestay_id != null)
            {
                HttpContext.Session.SetString("homestay_id", homestay_id);
            }
            ViewBag.error = TempData["error"] as string;
            return View("LoginForm");
        }

        [HttpPost]
        public IActionResult Login(String email, String password)
        {
            try
            {
                DateTime stamp = DateTime.Now;
                var account = dao_user.Login(email, password);

                if (dao_user.Login(email, password) == null)
                {
                    if (dao_user.GetPasswordAttempts(email) == 5 && dao_user.GetUnlockTime(email) == null)
                    {
                        dao_user.AddUnlockTime(email);
                    }
                    try
                    {
                        if (dao_user.GetPasswordAttempts(email) == 5 && stamp > dao_user.GetUnlockTime(email))
                        {
                            dao_user.DecreasePasswordAttempts(email);
                            dao_user.ResetUnlockTime(email);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (dao_user.GetPasswordAttempts(email) == 5)
                    {
                        ViewBag.error = "Bạn đã thử quá nhiều lần. Tài khoản của bạn sẽ bị khóa tạm thời trong 30 phút!";
                        ViewBag.email = email;
                        return View("LoginForm");
                    }
                    else
                    {
                        dao_user.UpdatePasswordAttempts(email);
                        ViewBag.error = "Email hoặc mật khẩu không chính xác";
                        ViewBag.email = email;
                        return View("LoginForm");
                    }
                }
                else
                {
                    try
                    {
                        if (dao_user.GetPasswordAttempts(email) == 5 && stamp > dao_user.GetUnlockTime(email))
                        {
                            dao_user.DecreasePasswordAttempts(email);
                        }
                    }
                    catch (Exception)
                    {
                    }
                    if (dao_user.GetPasswordAttempts(email) == 5)
                    {
                        ViewBag.error = "Bạn đã thử quá nhiều lần. Tài khoản của bạn sẽ bị khóa tạm thời trong 30 phút!";
                        ViewBag.email = email;
                        return View("LoginForm");
                    }
                    else if (dao_user.GetAccountStatus(email) == false)
                    {
                        ViewBag.error = "Tài khoản của bạn đã bị khoá. Vui lòng liên hệ quản trị viên để nhận được sự giúp đỡ";
                        ViewBag.email = email;
                        return View("LoginForm");
                    }
                    else
                    {
                        dao_user.ResetUnlockTime(email);
                        dao_user.ResetPasswordAttempts(email);
                        if (account.Role == 0)
                        {
                            HttpContext.Session.SetObject("account", account);
                            string homestay_id = HttpContext.Session.GetString("homestay_id");
                            if (homestay_id == null)
                            {
                                return RedirectToAction("LoadHomestay", "Homestay");
                            }
                            else
                            {
                                HttpContext.Session.Remove("homestay_id");
                                return RedirectToAction("ViewDetail", "Homestay", new { homestay_id = homestay_id, tab = "pills-rental" });
                            }
                        }
                        else
                        {
                            HttpContext.Session.SetObject("account", account);
                            return RedirectToAction("LoadHomestay", "Homestay");
                        }
                    }
                }
            }
            catch
            {
                ViewBag.error = "Có lỗi đã xảy ra! Vui lòng thử lại sau.";
                return View("LoginForm");
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Login");
        }
    }
}


