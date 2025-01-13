namespace EXE202.DAO.Casso.Account;

public class AccountDTO
{
    public int Id { get; set; }
    public string AccountNumber { get; set; }
    public string AccountName { get; set; }
    public string AccountType { get; set; }
    public int Balance { get; set; }
    public string Currency { get; set; }
    public string Swift { get; set; }
    public string Citad { get; set; }
    public string ServiceType { get; set; }
    public string BankName { get; set; }
    public int BIN { get; set; }
    public string BankCodeName { get; set; }
    public string Memo { get; set; }
    public int ConnectStatus { get; set; }
    public DateTime? BeginningSettingDate { get; set; }
    public DateTime? BeginningTxnDate { get; set; }
    public int BeginningBalance { get; set; }
    public int CreditTxnTotal { get; set; }
    public int CreditTxnAmount { get; set; }
    public DateTime? LockSynDate { get; set; }
    public int EndingBalance { get; set; }
    public DateTime? EndingTxnDate { get; set; }
}