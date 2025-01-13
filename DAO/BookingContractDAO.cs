using EXE202.Models;
using Microsoft.EntityFrameworkCore;

namespace EXE202.DAO;

public class BookingContractDAO
{
    private readonly EXE202Context _context;

    public BookingContractDAO(EXE202Context context)
    {
        _context = context;
    }

    public async Task<BookingContract> CreateBookingContract(int customerId, int homestayId, string firstName, string lastName,
        string email, string phone, DateTime startDate, DateTime endDate, string destination, decimal totalAmount, string note, string status, string paymentMethod)
    {
        BookingContract contract = new BookingContract()
        {
            CustomerId = customerId,
            RoomId = homestayId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            StartDate = startDate,
            EndDate = endDate,
            Destination = destination,
            TotalAmount = totalAmount,
            Note = note,
            Status = status,
            PaymentMethod = paymentMethod
        };
        await _context.BookingContracts.AddAsync(contract);
        await _context.SaveChangesAsync();
        return contract;
    }

    public async Task<BookingContract> ConfirmBookingContract(int bookingId)
    {
        var bookingContract = await _context.BookingContracts.FirstOrDefaultAsync(bc => bc.BookingId == bookingId);
        if (bookingContract != null)
        {
            bookingContract.Status = "CHỜ XÁC NHẬN";
            _context.Update(bookingContract);
            await _context.SaveChangesAsync();
        }
        return bookingContract;
    }
}