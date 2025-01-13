using System.Text.Json.Serialization;

namespace EXE202.DAO.Casso.Transaction;

public class TransactionRecord
{
    public int Id { get; set; }
    public string Tid { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    [JsonPropertyName("cusumBalance")]
    public decimal? CusumBalance { get; set; }
    [JsonPropertyName("when")]
    public DateTime? When { get; set; }
    public string BankSubAccId { get; set; }
    public string PaymentChannel { get; set; }
    public string VirtualAccount { get; set; }
    public string VirtualAccountName { get; set; }
    public string CorresponsiveName { get; set; }
    public string CorresponsiveAccount { get; set; }
    public string CorresponsiveBankId { get; set; }
    public string CorresponsiveBankName { get; set; }
    public int AccountId { get; set; }
    public string BankCodeName { get; set; }
}