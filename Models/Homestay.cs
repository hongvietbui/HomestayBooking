using System;
using System.Collections.Generic;

namespace EXE202.Models
{
    public partial class Homestay
    {
        public Homestay()
        {
            Feedbacks = new HashSet<Feedback>();
            ImageHomestays = new HashSet<ImageHomestay>();
        }

        public int HomestayId { get; set; }
        public int HostId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Country { get; set; } = null!;
        public decimal PricePerNight { get; set; }
        public int MaxGuests { get; set; }
        public bool? Wifi { get; set; }
        public bool? Cafe { get; set; }
        public bool? AirConditional { get; set; }
        public bool? Parking { get; set; }
        public bool? Pool { get; set; }
        public bool? Kitchen { get; set; }
        public bool? PetFriendly { get; set; }
        public bool? Laundry { get; set; }
        public bool? BreakfirstIncluded { get; set; }
        public bool? Gym { get; set; }
        public bool? SmokingAllowed { get; set; }
        public bool? Balcony { get; set; }
        public string? RoomSize { get; set; }
        public bool? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Host Host { get; set; } = null!;
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        public virtual ICollection<ImageHomestay> ImageHomestays { get; set; }
    }
}
