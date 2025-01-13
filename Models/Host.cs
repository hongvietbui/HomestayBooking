using System;
using System.Collections.Generic;

namespace EXE202.Models
{
    public partial class Host
    {
        public Host()
        {
            Homestays = new HashSet<Homestay>();
        }

        public int HostId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime? CreatedAt { get; set; }

        public virtual ICollection<Homestay> Homestays { get; set; }
    }
}
