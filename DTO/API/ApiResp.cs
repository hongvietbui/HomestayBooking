namespace EXE202.DAO.API;

public class ApiResp<T>
{
    public int Error { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }
}