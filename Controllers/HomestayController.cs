using EXE202.DAO;
using EXE202.Models;
using Microsoft.AspNetCore.Mvc;

namespace EXE202.Controllers
{
    public class HomestayController : Controller
    {
        private readonly AccountDAO dao_user;
        private readonly FinanceDAO finance_dao;
        private readonly HomestayDAO homestay_dao;
        private readonly BookingContractDAO _bookingContractDao;
        public HomestayController(AccountDAO accountDAO, FinanceDAO financeDAO, HomestayDAO homestayDAO, BookingContractDAO bookingContractDao)
        {
            dao_user = accountDAO;
            finance_dao = financeDAO;
            homestay_dao = homestayDAO;
            _bookingContractDao = bookingContractDao;
        }
        public IActionResult LoadHomestay(string host, string homestayName, decimal? minPrice, decimal? maxPrice, int? max_guests, string description, string is_available, string sortBy, string orderBy, string param)
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
            }
            ViewBag.getAllHost = homestay_dao.GetAllHost();

            var homestays = homestay_dao.FilterHomestay(host, homestayName, minPrice, maxPrice, max_guests, description, is_available);

            if (string.IsNullOrEmpty(sortBy) && string.IsNullOrEmpty(orderBy))
            {
                homestays = homestays.OrderBy(h => h.Name).ToList();
            }
            else if (!string.IsNullOrEmpty(sortBy) && !string.IsNullOrEmpty(orderBy))
            {
                if (sortBy == "HomestayName")
                {
                    homestays = orderBy == "Ascending" ? homestays.OrderBy(h => h.Name).ToList() : homestays.OrderByDescending(h => h.Name).ToList();
                }
                else if (sortBy == "Price")
                {
                    homestays = orderBy == "Ascending" ? homestays.OrderBy(h => h.PricePerNight).ToList() : homestays.OrderByDescending(h => h.PricePerNight).ToList();
                }
            }

            var homestayImages = new List<ImageHomestay>();
            foreach (var home in homestays)
            {
                homestayImages.Add(homestay_dao.GetImageHomestaysByHomestayId(home.HomestayId).FirstOrDefault());
            }

            ViewBag.homestayImages = homestayImages;
            ViewBag.getAllHomestay = homestays;

            // Send back search and sort parameters to the view
            ViewBag.host_sentback = host;
            ViewBag.description_sentback = description;
            ViewBag.homestayName_sentback = homestayName;
            ViewBag.minRentalPrice_sentback = minPrice;
            ViewBag.maxRentalPrice_sentback = maxPrice;
            ViewBag.maxGuests_sentback = max_guests;
            ViewBag.isAvailable_sentback = is_available;
            ViewBag.sortBy_sentback = sortBy;
            ViewBag.orderBy_sentback = orderBy;
            ViewBag.param = param;
            return View("ShowHomestay");
        }

        public IActionResult ViewDetail(int homestay_id)
        {

            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            if (customer != null)
            {
                ViewBag.name = customer.LastName;
                var check = homestay_dao.CheckBookingContractCompleted(homestay_id, customer.CustomerId);
                if (check == true)
                {
                    ViewBag.check = true;
                }
            }
            if (TempData.ContainsKey("homestay_id"))
            {
                homestay_id = (int)TempData["homestay_id"];
            }
            var contract_draft = HttpContext.Session.GetObject<BookingContract>("contract_draft");
            if (contract_draft == null)
            {
                Customer thisCustomer = HttpContext.Session.GetObject<Customer>("account");
                if (thisCustomer != null && thisCustomer.Role == 0)
                {
                    contract_draft = new BookingContract();
                    contract_draft.FirstName = thisCustomer.FirstName;
                    contract_draft.LastName = thisCustomer.LastName;
                    contract_draft.Phone = thisCustomer.PhoneNumber;
                    contract_draft.Email = thisCustomer.Email;
                    contract_draft.StartDate = DateTime.Now.AddDays(2);
                    contract_draft.EndDate = contract_draft.StartDate.AddDays(1);
                    contract_draft.Destination = thisCustomer.Address;
                }
            }

            ViewBag.host = homestay_dao.GetHostByID(homestay_id);
            ViewBag.homestay_detail = homestay_dao.getHomestayByID(homestay_id);
            ViewBag.related_homestay = homestay_dao.GetRelatedHomestay(homestay_id);
            ViewBag.getAllHost = homestay_dao.GetAllHost();
            ViewBag.contract_draft = contract_draft;
            ViewBag.account = customer;
            ViewBag.message = TempData["message"] as string;

            var homestayImages = homestay_dao.GetImageHomestaysByHomestayId(homestay_id);
            ViewBag.homestayImages = homestayImages;

            var rental_contract = homestay_dao.GetBookingContractByHomestayID(homestay_id);
            if (rental_contract != null)
            {
                ViewBag.end_date = rental_contract.EndDate.ToString("yyyy-MM-dd");
            }
            return View("HomestayDetail");
        }
        public async Task<IActionResult> GetRentalInfo(int homestay_id, string fname, string lname, string email, string phone, DateTime start_date, DateTime end_date, string destination, string note)
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            var homestay = homestay_dao.getHomestayByID(homestay_id);

            if (start_date >= DateTime.Now && end_date >= start_date && (start_date - DateTime.Now).TotalDays <= 2)
            {
                HttpContext.Session.Remove("contract_draft");
                TimeSpan duration = end_date - start_date;
                int numberOfDays = duration.Days;
                if (numberOfDays == 0)
                {
                    numberOfDays = 1;
                }
                decimal total = homestay.PricePerNight * numberOfDays;
                
                var contract = await _bookingContractDao.CreateBookingContract(customer.CustomerId, homestay_id, fname, lname, email, phone,
                    start_date, end_date, destination, total, note, "CHỜ XÁC NHẬN", "VietQR");
                HttpContext.Session.SetObject("contract", contract);
                return RedirectToAction("PurchaseByOceanPAY", "Purchase");
            }
            else
            {
                TempData["message"] = "Error Date! Please try again.";
                TempData["homestay_id"] = homestay_id;

                BookingContract contract_draft = new BookingContract()
                {
                    FirstName = fname,
                    LastName = lname,
                    Email = email,
                    Phone = phone,
                    StartDate = start_date,
                    EndDate = end_date,
                    Destination = destination,
                    Note = note
                };
                HttpContext.Session.SetObject("contract_draft", contract_draft);
                return RedirectToAction("ViewDetail", new { tab = "pills-rental" });
            }
        }

        [HttpPost]
        public IActionResult AddFeedback(string feedback_content, int rating, int homestay_id)
        {
            Customer customer = HttpContext.Session.GetObject<Customer>("account");
            homestay_dao.AddFeedback(feedback_content, rating, customer.CustomerId, homestay_id);
            return RedirectToAction("ViewDetail", new { homestay_id = homestay_id, tab = "pills-review" });
        }

        public IActionResult DeleteFeedback(int feedback_id, int homestay_id)
        {
            homestay_dao.DeleteFeedback(feedback_id);
            return RedirectToAction("ViewDetail", new { homestay_id = homestay_id, tab = "pills-review" });
        }
    }

}
