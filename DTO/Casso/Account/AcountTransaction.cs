namespace EXE202.DAO.Casso.Account;

public class AcountTransaction
{
    public int PrivateId { get; set; }
    public string Reference { get; set; }
    public DateTime? BookingDate { get; set; }
    public DateTime? TransactionDate { get; set; }
    public DateTime? TransactionDateTime { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public int RunningBalance { get; set; }
    public string VirtualAccountNumber { get; set; }
    public string VirtualAccountName { get; set; }
    public string PaymentChannel { get; set; }
    public string CounterAccountNumber { get; set; }
    public string CounterAccountName { get; set; }
    public string CounterAccountBankId { get; set; }
    public string CounterAccountBankName { get; set; }
}