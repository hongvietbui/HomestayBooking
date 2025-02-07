
using EXE202.DAO;
using EXE202.Hubs;
using EXE202.Services.Impl;
using Microsoft.AspNetCore.SignalR;

namespace EXE202.Services;

public class TransactionService : ITransactionService
{
    private readonly IHubContext<TransactionHub> _hubContext;
    private readonly BookingContractDAO _bookingContractDao;
    private readonly TransactionDAO _transactionDao;
    private readonly HomestayDAO _homestayDao;

    public TransactionService(IHubContext<TransactionHub> hubContext, BookingContractDAO bookingContractDao, TransactionDAO transactionDao, HomestayDAO homestayDao)
    {
        _hubContext = hubContext;
        _bookingContractDao = bookingContractDao;
        _transactionDao = transactionDao;
        _homestayDao = homestayDao;
    }

    public async Task OnTransactionSuccess(int bookingId)
    {
        var booking = await _bookingContractDao.GetBookingContractById(bookingId);
        
        if (booking == null)
        {
            return;
        }

        //await _homestayDao.UpdateHomestayStatusAsync(booking.RoomId);
        _homestayDao.UpdateHomestayStatus(booking.RoomId);
        await _transactionDao.CreateTransaction("Thanh toán thành công cho id: "+booking.BookingId, DateTime.Now, booking.TotalAmount, booking.CustomerId);
        await _hubContext.Clients.Group(booking.CustomerId.ToString()).SendAsync("TransactionSuccess");
    }
}