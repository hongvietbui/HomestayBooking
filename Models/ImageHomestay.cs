using System;
using System.Collections.Generic;

namespace EXE202.Models
{
    public partial class ImageHomestay
    {
        public int ImageId { get; set; }
        public int HomestayId { get; set; }
        public string Image1 { get; set; } = null!;
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }
        public string? Image5 { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Homestay Homestay { get; set; } = null!;
    }
}
