using EXE202.Hubs;
using EXE202.Services.Impl;
using Microsoft.AspNetCore.SignalR;

namespace EXE202.Services;

public class TransactionService : ITransactionService
{
    private readonly IHubContext<TransactionHub> _hubContext;

    public TransactionService(IHubContext<TransactionHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task OnTransactionSuccess(int customerId)
    {
        await _hubContext.Clients.Group(customerId.ToString()).SendAsync("TransactionSuccess");
    }
}