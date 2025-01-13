using System.Text.RegularExpressions;
using EXE202.DAO;
using EXE202.DAO.API;
using EXE202.DAO.Casso.Transaction;
using EXE202.Models;
using EXE202.Services.Impl;
using Microsoft.EntityFrameworkCore;

namespace EXE202.Services;

public class ScheduleService : IScheduleService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly EXE202Context _context;
    private readonly ILogger<ScheduleService> _logger;
    private readonly TransactionDAO _transactionDao;
    private readonly BookingContractDAO _bookingContractDao;
    private readonly ITransactionService _transactionService;
        
    public ScheduleService(IHttpClientFactory clientFactory, IConfiguration config, EXE202Context context, ILogger<ScheduleService> logger, TransactionDAO transactionDao, BookingContractDAO bookingContractDao, ITransactionService transactionService)
    {
        _config = config;
        _httpClientFactory = clientFactory;
        _httpClient = clientFactory.CreateClient("CassoAPI");
        _context = context;
        _logger = logger;
        _transactionDao = transactionDao;
        _bookingContractDao = bookingContractDao;
        _transactionService = transactionService;
    }
    
    public async Task ScanTransactionAsync()
    {
        Regex regex = new Regex(@"^(\w+)ChuyenKhoan(\w+)");
        if (!_httpClient.DefaultRequestHeaders.Contains("Authorization"))
        {
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Apikey {_config.GetValue<string>("Casso:Apikey")}");
        }
    
        var response = await _httpClient.GetAsync("transactions");
        if (response.IsSuccessStatusCode)
        {   
            var userInfo = await response.Content.ReadFromJsonAsync<ApiResp<Pagination<TransactionRecord>>>();
            for (int page = 1; page <= userInfo.Data.TotalPages; page++)
            {
                var responseByPage = await _httpClient.GetAsync($"transactions?page={page}");
                var userInfoByPage = await responseByPage.Content.ReadFromJsonAsync<ApiResp<Pagination<TransactionRecord>>>();
                foreach(var item in userInfoByPage.Data.Records)
                {
                    if (int.TryParse(item.Description, out int customerId))
                    {
                        //Check if the transaction is existed or not
                        var existedTransaction = await _context.Transactions.FirstOrDefaultAsync(t =>
                            t.IssueDate == item.When && t.CustomerId == customerId);
                        if (existedTransaction != null)
                        {
                            continue;
                        }
                        
                        var transaction = _context.Transactions.FirstOrDefault(t =>
                            t.IssueDate == item.When && t.CustomerId == customerId);
                        if (transaction == null)
                        {
                            await _transactionDao.CreateTransaction(
                                $"Nạp tiền vào tài khoản {customerId} {item.Amount} đồng", item.When.Value, item.Amount,
                                customerId);

                            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
                            customer.Balance += item.Amount;
                        }
                    }
                    else if(regex.Match(item.Description).Success)
                    {
                        var match = regex.Match(item.Description);
                        
                        //Check if the transaction is existed or not
                        var transaction = await _context.Transactions.FirstOrDefaultAsync(t =>
                            t.IssueDate == item.When && t.CustomerId == int.Parse(match.Groups[1].Value));
                        if (transaction != null)
                        {
                            continue;
                        }
                        
                        await _transactionDao.CreateTransaction(
                            $"Nạp tiền vào tài khoản {match.Groups[1].Value} {item.Amount} đồng cho {match.Groups[2].Value}", item.When.Value, item.Amount,
                            int.Parse(match.Groups[1].Value));
                        
                        await _bookingContractDao.ConfirmBookingContract(int.Parse(match.Groups[2].Value));
                        await _transactionService.OnTransactionSuccess(int.Parse(match.Groups[1].Value));
                        
                    }
                    else
                    {
                        // _logger.LogWarning("failed transaction with description: "+item.Description);
                    }
                }
            }
            await _context.SaveChangesAsync();
        }
    
        _logger.LogInformation("Hangfire: SCANNED Successfully!");
    }
}