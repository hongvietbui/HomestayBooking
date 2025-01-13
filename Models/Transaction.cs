using System;
using System.Collections.Generic;

namespace EXE202.Models
{
    public partial class Transaction
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal Action { get; set; }
        public string? Note { get; set; }

        public virtual Customer Customer { get; set; } = null!;
    }
}
