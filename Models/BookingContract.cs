using System;
using System.Collections.Generic;

namespace EXE202.Models
{
    public partial class BookingContract
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Destination { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentMethod { get; set; } = null!;
        public string? Note { get; set; }
        public string Status { get; set; } = null!;

        public virtual Customer Customer { get; set; } = null!;
    }
}
