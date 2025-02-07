using System.Text;
using System.Text.Json;
using EXE202.DTO.VietQR;
using EXE202.Models;
using EXE202.Services.Impl;
using Microsoft.EntityFrameworkCore;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EXE202.Services;

public class QRService : IQRService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly EXE202Context _context;
        
    public QRService(IHttpClientFactory clientFactory, IConfiguration config, EXE202Context context)
    {
        _context = context;
        _config = config;
        _httpClientFactory = clientFactory;
        _httpClient = clientFactory.CreateClient("VietQRAPI");
    }
    
    public async Task<string> GetQRDataURL(int customerId)
    {
        var contentReq = new VietQRReq
        {
            Format = _config.GetValue<string>("VietQR:Format"),
            Template = _config.GetValue<string>("VietQR:Template"),
            AccountName = _config.GetValue<string>("VietQR:AccountName"),
            AccountNo = _config.GetValue<string>("VietQR:AccountNo"),
            AcqId = _config.GetValue<int>("VietQR:AcqId"),
            AddInfo = customerId.ToString()
        };

        _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.GetValue<string>("VietQR:APIKey"));
        _httpClient.DefaultRequestHeaders.Add("x-client-id", _config.GetValue<string>("VietQR:ClientID"));

        var jsonContent = JsonSerializer.Serialize(contentReq, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("generate", httpContent);
        
        // Kiểm tra response và xử lý kết quả
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadFromJsonAsync<VietQRResp>();
            return responseContent.Data.QrDataURL;
        }
        else
        {
            return "";
        }
    }

    public async Task<string> GetQRDataURLWithBookingId(int bookingContractId, decimal? amount)
    {
        //var bookingContract = await _context.BookingContracts.FirstOrDefaultAsync(bc => bc.BookingId == bookingContractId);
        var contentReq = new VietQRReq
        {
            Format = _config.GetValue<string>("VietQR:Format"),
            Template = _config.GetValue<string>("VietQR:Template"),
            AccountName = _config.GetValue<string>("VietQR:AccountName"),
            AccountNo = _config.GetValue<string>("VietQR:AccountNo"),
            AcqId = _config.GetValue<int>("VietQR:AcqId"),
            AddInfo = "Chuyen khoan cho bookingId "+bookingContractId,
            Amount = amount
        };

        _httpClient.DefaultRequestHeaders.Add("x-api-key", _config.GetValue<string>("VietQR:APIKey"));
        _httpClient.DefaultRequestHeaders.Add("x-client-id", _config.GetValue<string>("VietQR:ClientID"));

        var jsonContent = JsonSerializer.Serialize(contentReq, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("generate", httpContent);
        
        // Kiểm tra response và xử lý kết quả
        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadFromJsonAsync<VietQRResp>();
            return responseContent.Data.QrDataURL;
        }
        else
        {
            return "";
        }
    }
}