namespace EXE202.DTO.VietQR;

public class VietQRReq
{
    public string AccountNo { get; set; }
    public string AccountName { get; set; }
    public decimal AcqId { get; set; }
    public decimal? Amount { get; set; }
    public string AddInfo { get; set; }
    public string Format { get; set; }
    public string Template { get; set; }
}