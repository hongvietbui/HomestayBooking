using Microsoft.AspNetCore.SignalR;
namespace EXE202.Hubs;

public class TransactionHub : Hub
{
    public async Task NotifyTransactionSuccess(int customerId)
    {
        await Clients.User(customerId.ToString()).SendAsync("TransactionSuccess");
    }
    
    public override async Task OnConnectedAsync()
    {
        // Lấy customerId từ query string
        var customerId = Context.GetHttpContext().Request.Query["customerId"].ToString();

        // Thêm kết nối hiện tại vào một nhóm dựa trên customerId
        if (!string.IsNullOrEmpty(customerId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, customerId);
        }

        await base.OnConnectedAsync();
    }
}