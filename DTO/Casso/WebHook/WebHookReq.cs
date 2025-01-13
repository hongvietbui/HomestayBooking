namespace EXE202.DAO.Casso.WebHook;

public class WebHookReq
{
    public bool Income_only { get; set; }
    public string Secure_token { get; set; }
    public string Webhook { get; set; }
}