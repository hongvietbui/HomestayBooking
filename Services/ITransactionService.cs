namespace EXE202.Services.Impl;

public interface ITransactionService
{
    Task OnTransactionSuccess(int bookingId);
}