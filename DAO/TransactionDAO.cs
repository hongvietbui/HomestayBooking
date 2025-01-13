using EXE202.Models;

namespace EXE202.DAO;

public class TransactionDAO
{
    private readonly EXE202Context _context;

    public TransactionDAO(EXE202Context context)
    {
        _context = context;
    }

    public async Task<Transaction> CreateTransaction(string note, DateTime issueDate, decimal action, int customerId)
    {
        var newTransaction = new Transaction
        {
            Note = note,
            IssueDate = issueDate,
            Action = action,
            CustomerId = customerId
        };

        await _context.Transactions.AddAsync(newTransaction);
        await _context.SaveChangesAsync();
        return newTransaction;
    }
}