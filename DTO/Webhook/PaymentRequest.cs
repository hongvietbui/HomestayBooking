namespace EXE202.DTO.Webhook;

public class PaymentRequest
{
    public string Token { get; set; }
    public PaymentDTO Payment { get; set; } = null!;
}