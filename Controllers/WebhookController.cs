using System.Text.Json;
using System.Text.RegularExpressions;
using EXE202.DAO;
using EXE202.DTO.Webhook;
using EXE202.Services.Impl;
using Microsoft.AspNetCore.Mvc;

namespace EXE202.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController(BookingContractDAO bookingContractDao, ITransactionService transactionService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetAsync([FromBody] PaymentRequest request)
    {
        if (request.Token != "123456789:ABCDEF")
        {
            return Problem("Unauthorized", statusCode: 403);
        }
        
        var transactionIdStr = ExtractBookingId(request.Payment.Content);

        if (!int.TryParse(transactionIdStr, out var transactionId))
            return Ok("Invalid transaction ID");
        
        if (await bookingContractDao.ConfirmBookingContract(transactionId, request.Payment.Amount))
        {
            await transactionService.OnTransactionSuccess(transactionId);
            return Ok("Transaction confirmed");
        }

        return BadRequest("Invalid transaction");
    }
    
    static string ExtractBookingId(string input)
    {
        input = input.ToLower();
        Match match = Regex.Match(input, @"bookingid\s+(\d+)");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }
}