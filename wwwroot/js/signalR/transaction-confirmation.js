const connection = new signalR.HubConnectionBuilder()
    .withUrl("/transactionHub")
    .build();

connection.start().then(() => {
    console.log('Connected to the hub');
}).catch(err => console.error(err.toString()));

connection.on("TransactionSuccess", () => {
    alert('Transaction successful');
    window.location.href = '@Url.Action("PurchaseConfirmation", "Purchase")';
});
