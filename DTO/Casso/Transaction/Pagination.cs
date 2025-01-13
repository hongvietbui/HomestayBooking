namespace EXE202.DAO.Casso.Transaction;

public class Pagination<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int NextPage { get; set; }
    public int PrevPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalRecords { get; set; }
    public List<T> Records { get; set; }
}

