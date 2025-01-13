namespace EXE202.DAO.Casso.UserInfo;

public class BankAcc
{
    public int Id { get; set; }
    public Bank Bank { get; set; }
    public string BankAccountName { get; set; }
    public string BankSubAccId { get; set; }
    public decimal? Balance { get; set; }
    public string Memo { get; set; }
    public int ConnectStatus { get; set; }
    public int PlanStatus { get; set; }
    public DateTime BeginDate { get; set; }
}