namespace EXE202.DTO.Webhook;
using System.Text.Json.Serialization;

public class PaymentDTO
{
    public string Transaction_id { get; set; } = null!;
    public string Content { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public GateType Gate { get; set; }
    public string Account_receiver { get; set; } = null!;
}