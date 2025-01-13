using EXE202.DAO.API;
using EXE202.DAO.Casso.Transaction;
using EXE202.DAO.Casso.UserInfo;
using Microsoft.AspNetCore.Mvc;

namespace EXE202.Controllers
{
    public class TestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public TestController(IHttpClientFactory clientFactory, IConfiguration config)
        {
            _config = config;
            _httpClientFactory = clientFactory;
            _httpClient = clientFactory.CreateClient("CassoAPI");
        }
        
        public async Task<IActionResult> Index()
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {_config.GetValue<string>("Casso:Apikey")}");
            var response = await _httpClient.GetAsync("userInfo");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.TestResult = true;
                var userInfo = await response.Content.ReadFromJsonAsync<ApiResp<UserInfo>>();
                Console.WriteLine(userInfo.Data.User);
            }
            else
                ViewBag.TestResult = false;
            return View("CassoTest");
        }
        
        public async Task<IActionResult> Transaction()
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {_config.GetValue<string>("Casso:Apikey")}");
            var response = await _httpClient.GetAsync("transactions");
            if (response.IsSuccessStatusCode)
            {
                ViewBag.TestResult = await response.Content.ReadAsStringAsync();
                var userInfo = await response.Content.ReadFromJsonAsync<ApiResp<Pagination<TransactionRecord>>>();
                Console.WriteLine(userInfo.Data);
            }
            else
                ViewBag.TestResult = false;
            return View("CassoTest");
        }
    }
}
