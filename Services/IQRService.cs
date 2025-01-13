namespace EXE202.Services.Impl;

public interface IQRService
{
    public Task<string> GetQRDataURL(int customerId);

    public Task<string> GetQRDataURLWithBookingId(int bookingContractId, decimal? amount);
}